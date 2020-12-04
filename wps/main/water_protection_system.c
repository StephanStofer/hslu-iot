/* ADC1 Example
   This example code is in the Public Domain (or CC0 licensed, at your option.)
   Unless required by applicable law or agreed to in writing, this
   software is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
   CONDITIONS OF ANY KIND, either express or implied.
*/
#include <stdio.h>
#include <stdlib.h>
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "driver/gpio.h"
#include "driver/adc.h"
#include "esp_adc_cal.h"
#include "tcp_client.c"

#define DEFAULT_VREF	1100		//Use adc2_vref_to_gpio() to obtain a better estimate
#define NO_OF_SAMPLES   64		  //Multisampling

static esp_adc_cal_characteristics_t *adc_chars;
#if CONFIG_IDF_TARGET_ESP32
static const adc_channel_t channel_piezo_0 = ADC_CHANNEL_4;
static const adc_channel_t channel_piezo_1 = ADC_CHANNEL_5;
static const adc_channel_t channel_water_0 = ADC_CHANNEL_6;
static const adc_bits_width_t width = ADC_WIDTH_BIT_12;
#elif CONFIG_IDF_TARGET_ESP32S2
static const adc_channel_t channel_piezo_0 = ADC_CHANNEL_4;
static const adc_channel_t channel_piezo_1 = ADC_CHANNEL_5;
static const adc_channel_t channel_water_0 = ADC_CHANNEL_6;
static const adc_bits_width_t width = ADC_WIDTH_BIT_13;
#endif
static const adc_atten_t atten = ADC_ATTEN_DB_0;
static const char *APPTAG = "WPS";


uint32_t intCharCpy(uint32_t num, char* str) 
{
	uint32_t i = 0;
	char moduloNum;
	bool gotNumber = false;
	if (num == 0)
	{
		str[i++] = '0';
	} else {
		while (num != 0)
		{
			moduloNum = num % 10;
			if (moduloNum > 0 || gotNumber) {
				gotNumber = true;
				str[i++] = moduloNum + '0';
			}
			num /= 10;
		}
	}
	str[i++] = ',';
	return i;
}

static void check_efuse(void)
{
#if CONFIG_IDF_TARGET_ESP32
	//Check if TP is burned into eFuse
	if (esp_adc_cal_check_efuse(ESP_ADC_CAL_VAL_EFUSE_TP) == ESP_OK) {
		printf("eFuse Two Point: Supported\n");
	} else {
		printf("eFuse Two Point: NOT supported\n");
	}
	//Check Vref is burned into eFuse
	if (esp_adc_cal_check_efuse(ESP_ADC_CAL_VAL_EFUSE_VREF) == ESP_OK) {
		printf("eFuse Vref: Supported\n");
	} else {
		printf("eFuse Vref: NOT supported\n");
	}
#elif CONFIG_IDF_TARGET_ESP32S2
	if (esp_adc_cal_check_efuse(ESP_ADC_CAL_VAL_EFUSE_TP) == ESP_OK) {
		printf("eFuse Two Point: Supported\n");
	} else {
		printf("Cannot retrieve eFuse Two Point calibration values. Default calibration values will be used.\n");
	}
#else
#error "This example is configured for ESP32/ESP32S2."
#endif
}


static void print_char_val_type(esp_adc_cal_value_t val_type)
{
	if (val_type == ESP_ADC_CAL_VAL_EFUSE_TP) {
		printf("Characterized using Two Point Value\n");
	} else if (val_type == ESP_ADC_CAL_VAL_EFUSE_VREF) {
		printf("Characterized using eFuse Vref\n");
	} else {
		printf("Characterized using Default Vref\n");
	}
}


void app_main(void)
{
	//Check if Two Point or Vref are burned into eFuse
	check_efuse();

	//Configure ADC
	adc1_config_width(width);
	adc1_config_channel_atten(channel_water_0, atten);
	adc1_config_channel_atten(channel_piezo_0, atten);
	adc1_config_channel_atten(channel_piezo_1, atten);
	
	//Characterize ADC
	adc_chars = calloc(1, sizeof(esp_adc_cal_characteristics_t));
	esp_adc_cal_value_t val_type = esp_adc_cal_characterize(ADC_UNIT_1, atten, width, DEFAULT_VREF, adc_chars);
	print_char_val_type(val_type);

	char* data_to_send = (char*)malloc(11+5+11+1000*5-1+13+1000*5-1+3);
	
	while(true)
	{
		uint32_t pos = 0;
		uint32_t raw = 0;
		memcpy(data_to_send, "{\"water_0\":", 11); pos += 11;
		//Multisampling
		uint32_t adc_reading = 0;
		for (uint32_t i = 0; i < NO_OF_SAMPLES; ++i) {
			adc_reading += adc1_get_raw((adc1_channel_t)channel_water_0);
		}
		adc_reading /= NO_OF_SAMPLES;
		pos += intCharCpy(adc_reading, data_to_send+pos);
		memcpy(data_to_send+pos, "\"piezo_0\":[", 11); pos += 11;
		for (uint32_t i = 0; i < 1000; ++i) {
			raw = adc1_get_raw((adc1_channel_t)channel_piezo_0);
			pos += intCharCpy(raw, data_to_send+pos);
		}
		--pos;
		memcpy(data_to_send+pos, "],\"piezo_1\":[", 13); pos += 13;
		for (uint32_t i = 0; i < 1000; ++i) {
			raw = adc1_get_raw((adc1_channel_t)channel_piezo_1);
			pos += intCharCpy(raw, data_to_send+pos);
		}
		--pos;
		memcpy(data_to_send+pos, "]}\0", 3); pos += 3;
		
		send_data(APPTAG, data_to_send);
		ESP_LOGI(APPTAG, "%s\n", data_to_send);
		vTaskDelay(pdMS_TO_TICKS(10000));
	}
	free(data_to_send);
}
