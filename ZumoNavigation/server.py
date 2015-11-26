#!/usr/bin/python

from cgi import parse_qs
import os
import signal
import time
from wsgiref.util import request_uri

from subprocess import check_output
def get_pid(name):
	processList = check_output(['ps', '-ef'])
	for process in processList.split('\n'):
		if "python" in process and name in process:
			return process.split()[1]
	return None 


ARDUINO_PATH="arduino/"
COMMAND_SERVER_PATH="command_server/"

def getCmdServStatut():
	pid = get_pid("command_server.py")
	if pid is None:
		return "Stopped"
	else:
		return "Running"
def killCmdServer():
	pid = get_pid("command_server.py")
	if pid is not None:
		os.kill(int(pid), signal.SIGTERM)
def startCmdServer():
	os.system("python " + COMMAND_SERVER_PATH + "command_server.py &")


class SimpleApp:
	def __init__(self, environ, start_response):
		self.environ = environ
		self.start = start_response
		file = open('index.html', 'r')
		self.indexFile = file.read()
		file.close()
	def convertString(self, code):
		strCode = str(code)[2:-2]
		strCode = strCode.replace("\\r\\n", "\n");
		strCode = strCode.replace("\\\\n", "\\n");
		strCode = strCode.replace("\\t", "\t");
		strCode = strCode.replace("\\\'", "\'");
		strCode = strCode.replace("\\\"", "\"");
		return strCode
	def postUpload(self):
		request_body_size = int(self.environ.get('CONTENT_LENGTH', 0))
		request_body = self.environ['wsgi.input'].read(request_body_size)
		d = parse_qs(request_body)
		result = d['code']
		file = open(ARDUINO_PATH + "blink.ino", "w")
		file.write(self.convertString(result))
		file.close()
		killCmdServer()
		os.system("cd " + ARDUINO_PATH + " ; make upload ; cd -")
		startCmdServer()
	def postStartCmdServer(self):
		print("REQUEST: Start command server")
		startCmdServer()
	def postStopCmdServer(self):
		print("REQUEST: Stop command server")
		killCmdServer()
		
	def __iter__(self):
		status = '200 OK'
		response_headers = [('Content-type','text/html')]
		self.start(status, response_headers)
		if self.environ['REQUEST_METHOD'] == 'POST':
			url = request_uri(self.environ)
			if url.endswith("/upload"):
				self.postUpload()
				yield 'Uploaded'
			elif url.endswith("/startCmdServ"):
				self.postStartCmdServer()
				yield 'Started'
			elif url.endswith("/stopCmdServ"):
				self.postStopCmdServer()
				yield 'Stopped'
			else:
				yield 'Unknown command'
		else:
			url = request_uri(self.environ)
			if url.endswith("1337/"):
				file = open(ARDUINO_PATH + "blink.ino", "r")
				webpage = self.indexFile.replace("SOURCECODE", file.read())
				webpage = webpage.replace("COMMANDSERVERSTATUT", getCmdServStatut())
				file.close()
				yield webpage
			elif url.endswith("/log"):
				file = open("/tmp/log_nav", "r")
				yield file.read()
			else:
				print("Trying to access : " + url)
				yield "Nothing here"



from wsgiref.simple_server import make_server, demo_app

if getCmdServStatut() == "Running":
	print("Killing command server...")
	killCmdServer()
	print("Sleep 2 sec to wait the socket...")
	time.sleep(2)
httpd = make_server('', 1337, SimpleApp)
print "Serving HTTP on port 1337..."
# Respond to requests until process is killed
if getCmdServStatut() == "Stopped":
	print "Starting command server..."
	startCmdServer()
print "Done."
httpd.serve_forever()
