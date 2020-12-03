import socket
while True:
	server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
	server.bind(('192.168.133.79', 3333))
	server.listen(3)
	print ("[*] Started listening ")
	client,addr= server.accept()
	print ("[*] Got a connection from ", addr[0], ":", addr[1])
	data = client.recv(10041)
	print ("[*] Recevied '", data, "' from the client")
	print ("    Processing data")
	client.close()
	with open("test.json", "wb") as f:
		f.write(data)