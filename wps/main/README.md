# WPS Application

The application measures at the ADC-Port and send data to a Server. 
First it will create a TCP socket and tries to connect to the server with predefined IP address and port number over a Wifi-Connection.
When a connection is successfully established, the application sends message.

## How to test without a TCP server

In order to create TCP server that communicates with the application, following these steps.

There are many host-side tools which can be used to interact with the UDP/TCP server/client. 
One command line tool is [netcat](http://netcat.sourceforge.net) which can send and receive many kinds of packets. 
Note: please replace `192.168.0.167 3333` with desired IPV4 address and port number in the following command.

### TCP server using netcat
```
nc -klv 192.168.0.167 3333
```

## Configure the project

```
idf.py menuconfig
```

Set following parameters under Connection Settings:

* Set `WiFi Settings` with SSID and password of your local network.

* Set `Server Address` to the IP and Port the application will connect to.

## Build and Flash

Build the project and flash it to the board, then run monitor tool to view serial output:

```
idf.py -p PORT flash monitor
```

(To exit the serial monitor, type ``Ctrl-]``.)

## Troubleshooting

Start TCP server first, to receive data sent from the client (application).