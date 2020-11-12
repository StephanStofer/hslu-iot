import socket
server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server.bind(('127.0.0.1', 1234))
server.listen(3)
print ("[*] Started listening ")
client,addr= server.accept()
print ("[*] Got a connection from ", addr[0], ":", addr[1])
while True:
    data = client.recv(1024)
    print ("[*] Recevied '", data, "' from the client")
    print ("    Processing data")
    if (data=="Hello server"):
        client.send("Hello client")
        print("      Processing done. \n[*] Reply sent")
    elif(data=="disconnect"):
        client.send("Goodbye")
        client.close()
        break
    else:
        client.send("Invalid data")
        print("      Processing done Invalid data \n[*] Reply sent")