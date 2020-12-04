//
// Created by Stephan Stofer on 06.11.20.
// Updated 2.12.20 inspired by https://github.com/espressif/esp-idf/blob/master/examples/protocols/sockets/tcp_client/main/tcp_client.c
//

#include <string.h>
#include <sys/param.h>
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "freertos/event_groups.h"
#include "esp_system.h"
#include "esp_wifi.h"
#include "esp_sleep.h"
#include "esp_event.h"
#include "esp_log.h"
#include "nvs_flash.h"
#include "esp_netif.h"
#include "wifi.h"
#include "lwip/err.h"
#include "lwip/sockets.h"


#define HOST_IP_ADDR CONFIG_IPV4_ADDR
#define PORT CONFIG_PORT

static const char *TAG = "TCP-Client";
static const char *payload = "some message";


static void tcp_client_task(void *pvParameters) {
    char host_ip[] = HOST_IP_ADDR;
    int addr_family = 0;
    int ip_protocol = 0;

    while (1) {
        struct sockaddr_in dest_addr;
        dest_addr.sin_addr.s_addr = inet_addr(host_ip);
        dest_addr.sin_family = AF_INET;
        dest_addr.sin_port = htons(PORT);
        addr_family = AF_INET;
        ip_protocol = IPPROTO_IP;

        int sock = socket(addr_family, SOCK_STREAM, ip_protocol);
        if (sock < 0) {
            ESP_LOGE(TAG, "Unable to create socket: errno %d", errno);
            break;
        }
        ESP_LOGI(TAG, "Socket created, connecting to %s:%d", host_ip, PORT);

        int err = connect(sock, (struct sockaddr *) &dest_addr, sizeof(struct sockaddr));
        if (err != 0) {
            ESP_LOGE(TAG, "Socket unable to connect: errno %d", errno);
            break;
        }
        ESP_LOGI(TAG, "Successfully connected");

        int rc = send(sock, payload, strlen(payload), 0);
        vTaskDelay(2000 / portTICK_PERIOD_MS);

        if (rc >= 0) {
            ESP_LOGI(TAG, "Successfully sent payload");
            ESP_LOGI(TAG, "Shutting down socket and closing connection...");
            shutdown(sock, 0);
            close(sock);
            break;
        }

        ESP_LOGE(TAG, "Error occurred during sending: errno %d", errno);
        break;
    }

    ESP_ERROR_CHECK(disconnect_wifi());
    ESP_ERROR_CHECK(esp_event_loop_delete_default());
    ESP_LOGI(TAG, "Disconnect WiFi, delete Event-Loop and Task");
    vTaskDelete(NULL);
}

static void send_data(const char sender[], const char data[]) {
    payload = data;
    ESP_LOGI(TAG, "Got data from: %s, sending.. %s", sender, payload);

    ESP_ERROR_CHECK(nvs_flash_init());
    ESP_ERROR_CHECK(esp_netif_init());
    ESP_ERROR_CHECK(esp_event_loop_create_default());
    ESP_ERROR_CHECK(connect_to_wifi());

    xTaskCreate(tcp_client_task, TAG, 4096, NULL, 5, NULL);
}