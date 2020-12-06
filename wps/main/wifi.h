//
// Created by Stephan Stofer on 06.11.20.
// inspired by https://github.com/espressif/esp-idf/blob/release/v4.1/examples/common_components/protocol_examples_common/connect.c
//

#include "esp_err.h"
#include "esp_netif.h"

#define INTERFACE get_netif()

/**
 * @brief Configure Wi-Fi, connect, wait for IP
 *
 * @return ESP_OK on successful connection
 */
esp_err_t connect_to_wifi(void);

/**
 * Counterpart to connect, de-initializes Wi-Fi
 */
esp_err_t disconnect_wifi(void);

/**
 * @brief Returns esp-netif pointer created by connect()
 */
esp_netif_t *get_netif(void);

/**
 * @brief Returns esp-netif pointer created by connect() described by
 * the supplied desc field
 *
 * @param desc Textual interface of created network interface, for example "sta"
 * indicate default WiFi station.
 */
esp_netif_t *get_netif_from_desc(const char *desc);