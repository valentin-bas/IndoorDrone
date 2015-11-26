#!/usr/bin/python

import serial

ser = serial.Serial('/dev/ttyACM0', 9600)
#while True:
#	print(ser.readline())
#	ser.readline()

'''
    Simple socket server using threads
'''
 
import socket
import sys
from thread import *
 
HOST = ''   # Symbolic name meaning all available interfaces
PORT = 1338 # Arbitrary non-privileged port
 
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
print 'Socket created'
 
#Bind socket to local host and port
try:
    s.bind((HOST, PORT))
except socket.error as msg:
    print 'Bind failed. Error Code : ' + str(msg[0]) + ' Message ' + msg[1]
    sys.exit()
     
print 'Socket bind complete'
 
#Start listening on socket
s.listen(10)
print 'Socket now listening'
 
#Function for handling connections. This will be used to create threads
def clientthread(conn):
    #Sending message to connected client
    conn.send('Command Server Connection\r\n\r\n') #send only takes string
     
    data = conn.recv(1024)
    if not data: 
	conn.close()
	return
    #infinite loop so that function do not terminate and thread do not end.
    running=True
    while running:
        while ("\n" in data):
		pos = data.find("\n")
		command = data[:pos]
		command = command.replace("\r","")
		if (command == "quit"):
			running = False
		else:
			ser.write(command + "\n")
		data = data[pos+1:]
        	reply = 'Command : ' + command + "\r\n"
        	conn.sendall(reply)
        #Receiving from client
        tmpdata = conn.recv(1024)
        if not tmpdata: 
            break
	data = data + tmpdata
     
     
    #came out of loop
    conn.close()
 
#now keep talking with the client
while 1:
    #wait to accept a connection - blocking call
    conn, addr = s.accept()
    print 'Connected with ' + addr[0] + ':' + str(addr[1])
     
    #start new thread takes 1st argument as a function name to be run, second is the tuple of arguments to the function.
    start_new_thread(clientthread ,(conn,))
 
s.close()
ser.close()
