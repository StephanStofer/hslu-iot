import socket
while True:
	server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
	server.bind(('192.168.137.1', 3333))
	server.listen(3)
	print("[*] Started listening for ESP32")
	client,addr= server.accept()
	print("[*] Got a connection from ESP32 on ", addr[0], ":", addr[1])
	data = client.recv(20000)
	print("[*] Recevied '", data, "' from the ESP32")
	client.close()
	
	try:
		dest = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
		dest.settimeout(5)
		print("[*] Connecting to Server")
		dest.connect(("25.92.19.65", 3333))
		print("[*] Sendin g data to Server")
		dest.sendall(data)
		dest.close()
	except:
		print("[*] Server timed out")
		pass
