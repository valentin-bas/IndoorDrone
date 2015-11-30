#!/usr/bin/python

import serial
import Queue
import threading
import time

class CommandSerial(threading.Thread):
	def __init__(self):
		super(CommandSerial, self).__init__()
		self.readqueue = Queue.Queue(0) #the received data is put in a queue
		self.writequeue = Queue.Queue(0) #the data to send is put in a queue
		self._buffer = ''
		#configure serial connection
		self._ser = serial.Serial('/dev/ttyACM0', 9600)

	def run(self):
		while True:
			self._buffer += self._ser.read(self._ser.inWaiting()) #or 1) #read all char in buffer
			while "\r\n" in self._buffer: #split data line by line and store it in var
				var, self._buffer = self._buffer.split("\r\n", 1)
				self.readqueue.put(var) #put received line in the queue
			try:
				var = self.writequeue.get(False) #try to fetch a value from queuexcept Queue.Empty: 
			except Queue.Empty:
				pass #if it is empty, do nothing
			else:
				self._ser.write(var + "\n")
			time.sleep(0.01)   #do not monopolize CPU

import socket
import sys
from thread import *

class CommandSocket():
	def __init__(self, serial):
		self.HOST = ''   # Symbolic name meaning all available interfaces
		self.PORT = 1338 # Arbitrary non-privileged port
 
		self.cmdserial = serial
		self.s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
		print 'Socket created'
 		try:
    			self.s.bind((self.HOST, self.PORT))
		except socket.error as msg:
			print 'Bind failed. Error Code : ' + str(msg[0]) + ' Message ' + msg[1]
			sys.exit()
		print 'Socket bind complete'
 
		self.s.listen(10)
 	def loop(self):
		while 1:
			conn, addr = self.s.accept()
			print 'Connected with ' + addr[0] + ':' + str(addr[1])
			start_new_thread(clientthread ,(conn,self.cmdserial,))
	def close(self):
		self.s.close()
 

#Function for handling connections. This will be used to create threads
def clientthread(conn, cmdserial):
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
				cmdserial.writequeue.put(command)
			data = data[pos+1:]
        		reply = 'Command : ' + command + "\r\n"
        		conn.sendall(reply)
		if running:
        		#Receiving from serial
			try:
				serreply = cmdserial.readqueue.get(False)
			except Queue.Empty:
				pass
			else:
        			conn.sendall('Reply : ' + serreply + "\r\n")
        		#Receiving from client
        		tmpdata = conn.recv(1024)
        		if not tmpdata: 
        		    break
			data = data + tmpdata
     
	#came out of loop
	conn.close()



if __name__ == '__main__':
	cmdserial = CommandSerial()
	cmdserial.start()

	cmdsocket = CommandSocket(cmdserial)
	cmdsocket.loop()
	cmdsocket.close() 
