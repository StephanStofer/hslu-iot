//
// Created by Stephan Stofer on 06.11.20.
//

#include "string.h"
#include "wifi.h"
#include "sdkconfig.h"
#include "esp_event.h"
#include "esp_wifi.h"
#include "esp_wifi_default.h"
#include "esp_log.h"
#include "esp_netif.h"
#include "driver/gpio.h"
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "freertos/event_groups.h"
#include "lwip/err.h"
#include "lwip/sys.h"

#define DO_CONNECT CONFIG_CONNECT_WIFI
#if CONFIG_CONNECT_WIFI
static esp_netif_t* wifi_start(void);
static void wifi_stop(void);
#endif
#define NR_OF_IP_ADDRESSES_TO_WAIT_FOR (s_active_interfaces)


static int s_active_interfaces = 0;
static xSemaphoreHandle s_semph_get_ip_addrs;
static esp_netif_t *s_esp_netif = NULL;
static const char *TAG = "WiFi";


/**
 * @brief Checks the netif description if it contains specified prefix.
 * All netifs created withing common connect component are prefixed with the module TAG,
 * so it returns true if the specified netif is owned by this module
 */
static bool is_our_netif(const char *prefix, esp_netif_t *netif)
{
    return strncmp(prefix, esp_netif_get_desc(netif), strlen(prefix)-1) == 0;
}

/* set up connection, Wi-Fi and/or Ethernet */
static void start(void)
{

#if CONFIG_CONNECT_WIFI
    s_esp_netif = wifi_start();
    s_active_interfaces++;
#endif

#if DO_CONNECT
    /* create semaphore if at least one interface is active */
    s_semph_get_ip_addrs = xSemaphoreCreateCounting(NR_OF_IP_ADDRESSES_TO_WAIT_FOR, 0);
#endif

}

/* tear down connection, release resources */
static void stop(void)
{
#if CONFIG_CONNECT_WIFI
    wifi_stop();
    s_active_interfaces--;
#endif
}

#if DO_CONNECT
static esp_ip4_addr_t s_ip_addr;

static void on_got_ip(void *arg, esp_event_base_t event_base,
                      int32_t event_id, void *event_data)
{
    ip_event_got_ip_t *event = (ip_event_got_ip_t *)event_data;
    if (!is_our_netif(TAG, event->esp_netif)) {
        ESP_LOGW(TAG, "Got IPv4 from another interface \"%s\": ignored", esp_netif_get_desc(event->esp_netif));
        return;
    }
    ESP_LOGI(TAG, "Got IPv4 event: Interface \"%s\" address: " IPSTR, esp_netif_get_desc(event->esp_netif), IP2STR(&event->ip_info.ip));
    memcpy(&s_ip_addr, &event->ip_info.ip, sizeof(s_ip_addr));
    xSemaphoreGive(s_semph_get_ip_addrs);
}
#endif

esp_err_t connect_to_wifi(void)
{
#if DO_CONNECT
    if (s_semph_get_ip_addrs != NULL) {
        return ESP_ERR_INVALID_STATE;
    }
#endif
    start();
    ESP_ERROR_CHECK(esp_register_shutdown_handler(&stop));
    ESP_LOGI(TAG, "Waiting for IP(s)");
    for (int i=0; i<NR_OF_IP_ADDRESSES_TO_WAIT_FOR; ++i) {
        xSemaphoreTake(s_semph_get_ip_addrs, portMAX_DELAY);
    }
    // iterate over active interfaces, and print out IPs of "our" netifs
    esp_netif_t *netif = NULL;
    esp_netif_ip_info_t ip;
    for (int i=0; i<esp_netif_get_nr_of_ifs(); ++i) {
        netif = esp_netif_next(netif);
        if (is_our_netif(TAG, netif)) {
            ESP_LOGI(TAG, "Connected to %s", esp_netif_get_desc(netif));
            ESP_ERROR_CHECK(esp_netif_get_ip_info(netif, &ip));

            ESP_LOGI(TAG, "- IPv4 address: " IPSTR, IP2STR(&ip.ip));

        }
    }
    return ESP_OK;
}

esp_err_t disconnect_wifi(void)
{
    if (s_semph_get_ip_addrs == NULL) {
        return ESP_ERR_INVALID_STATE;
    }
    vSemaphoreDelete(s_semph_get_ip_addrs);
    s_semph_get_ip_addrs = NULL;
    stop();
    ESP_ERROR_CHECK(esp_unregister_shutdown_handler(&stop));
    return ESP_OK;
}

#ifdef CONFIG_CONNECT_WIFI

static void on_wifi_disconnect(void *arg, esp_event_base_t event_base,
                               int32_t event_id, void *event_data)
{
    ESP_LOGI(TAG, "Wi-Fi disconnected, trying to reconnect...");
    esp_err_t err = esp_wifi_connect();
    if (err == ESP_ERR_WIFI_NOT_STARTED) {
        return;
    }
    ESP_ERROR_CHECK(err);
}

static esp_netif_t* wifi_start(void)
{
    char *desc;
    wifi_init_config_t cfg = WIFI_INIT_CONFIG_DEFAULT();
    ESP_ERROR_CHECK(esp_wifi_init(&cfg));

    esp_netif_inherent_config_t esp_netif_config = ESP_NETIF_INHERENT_DEFAULT_WIFI_STA();
    // Prefix the interface description with the module TAG
    // Warning: the interface desc is used in tests to capture actual connection details (IP, gw, mask)
    asprintf(&desc, "%s: %s", TAG, esp_netif_config.if_desc);
    esp_netif_config.if_desc = desc;
    esp_netif_config.route_prio = 128;
    esp_netif_t *netif = esp_netif_create_wifi(WIFI_IF_STA, &esp_netif_config);
    free(desc);
    esp_wifi_set_default_wifi_sta_handlers();

    ESP_ERROR_CHECK(esp_event_handler_register(WIFI_EVENT, WIFI_EVENT_STA_DISCONNECTED, &on_wifi_disconnect, NULL));
    ESP_ERROR_CHECK(esp_event_handler_register(IP_EVENT, IP_EVENT_STA_GOT_IP, &on_got_ip, NULL));

    ESP_ERROR_CHECK(esp_wifi_set_storage(WIFI_STORAGE_RAM));
    wifi_config_t wifi_config = {
        .sta = {
            .ssid = CONFIG_WIFI_SSID,
            .password = CONFIG_WIFI_PASSWORD,
        },
    };
    ESP_LOGI(TAG, "Connecting to %s...", wifi_config.sta.ssid);
    ESP_ERROR_CHECK(esp_wifi_set_mode(WIFI_MODE_STA));
    ESP_ERROR_CHECK(esp_wifi_set_config(ESP_IF_WIFI_STA, &wifi_config));
    ESP_ERROR_CHECK(esp_wifi_start());
    ESP_ERROR_CHECK(esp_wifi_connect());
    return netif;
}

static void wifi_stop(void)
{
    esp_netif_t *wifi_netif = get_netif_from_desc("sta");
    ESP_ERROR_CHECK(esp_event_handler_unregister(WIFI_EVENT, WIFI_EVENT_STA_DISCONNECTED, &on_wifi_disconnect));
    ESP_ERROR_CHECK(esp_event_handler_unregister(IP_EVENT, IP_EVENT_STA_GOT_IP, &on_got_ip));

    esp_err_t err = esp_wifi_stop();
    if (err == ESP_ERR_WIFI_NOT_INIT) {
        return;
    }
    ESP_ERROR_CHECK(err);
    ESP_ERROR_CHECK(esp_wifi_deinit());
    ESP_ERROR_CHECK(esp_wifi_clear_default_wifi_driver_and_handlers(wifi_netif));
    esp_netif_destroy(wifi_netif);
    s_esp_netif = NULL;
}
#endif // CONFIG_EXAMPLE_CONNECT_WIFI