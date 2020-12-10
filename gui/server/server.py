import socket
import json
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
	json_str = json.dumps(data)
	resp = json.loads(json_str)
	oldItem = 0
	diff1 = []
	for item in resp['piezo_0']:
		diff1.append(oldItem-item)
		oldItem = item
	print(diff1)
	diff2 = []
	oldItem = 0
	for item in diff1:
		diff2.append(oldItem-item)
		oldItem = item
	print(diff2)
	with open("test.json", "wb") as f:
		f.write(data)