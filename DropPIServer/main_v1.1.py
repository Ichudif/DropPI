
import socket
import threading
import os
import hashlib


class ThreadedServer(object):
    def __init__(self, host, port):
        self.HOST = host
        self.PORT = port
        self.HashSave = {}

        self.s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.s.bind((self.HOST, self.PORT))
        #self.s.setblocking(0)

    def listen(self):
        timeout = 6000

        self.s.listen(1)
        while True:
            client, addr = self.s.accept()
            client.settimeout(timeout)
            threading.Thread(target=self.listenToClient, args=(client, addr)).start()

    def listenToClient(self, client, addr):
        size = 4096

        while True:
            try:
                input = client.recv(1).decode('utf-8')
                if(input == ''):
                    continue
                Typ = int(input)
                print(Typ)
                
                if(Typ == 0):
                    PathByteCount = int(client.recv(3).decode('ISO 8859-1'))
                    Path = ''
                    for i in range(0, PathByteCount):
                        Path = Path + client.recv(1).decode('ISO 8859-1')
                    client.sendall(self.GetHash(Path).encode('utf-8'))
                elif(Typ == 1):
                    PathByteCount = int(client.recv(3).decode('utf-8'))
                    DataByteCount = int(client.recv(3).decode('utf-8'))
                    
                    Path = ''
                    for i in range(0, PathByteCount):
                        Path = Path + client.recv(1).decode('ISO 8859-1')
                    
                    data = []
                    for i in range(0, DataByteCount):
                        data.append(client.recv(1))
                        if(len(data) >= 50):
                            self.WriteFile(Path, data)
                    if(len(data) > 0):
                        self.WriteFile(Path, data)
                        
            except BaseException as e:
                import traceback
                
                traceback.print_exc()
                
                client.close()
                print(str(e))
                return False
            
    def GetHash(self, Path):
        if(Path[-1] == '\\' or Path[-1] == '/'):
            Path = Path[:-1]
            
        Path = Path.replace('\\', '/')
        try:
            return self.HashSave[Path]
        except:
            return '00000000000000000000000000000000'
        return '00000000000000000000000000000000'
            
    def WriteFile(self, Path, bytestowrite):
        Path = Path.replace('\\', '/')
        
        if(Path[0] != '/'):
            Path = '/' + Path
        
        f = open(os.getcwd() + Path, 'wb+')
        for byte in bytestowrite:
            f.write(byte)
        f.close()
    
    def updateHashs(self, Path):
        HashSave = {}
        if(Path[:-1] != '/'):
            Path = Path + '/'
        
        hashsreturn = []
        for item in os.listdir(os.fsencode(Path)):                      #os.getcwd())
            hashs = []
            if(os.path.isdir(Path + item.decode('utf-8'))):
                hashs = hashs + self.updateHashs(Path + item.decode('utf-8'))
                hashsstring = ''
                if(len(hashs) > 0):
                    hashs.sort()
                    for hash1 in hashs:
                        hashsstring = hashsstring + hash1
                
                finalhash = str(hashlib.md5(hashsstring.encode('utf-8')).hexdigest())
                self.HashSave[(Path + item.decode('utf-8')).replace(os.getcwd()+'/', '')] = finalhash
                hashsreturn.append(finalhash)
            else:
                help = open(Path + item.decode('utf-8'),'rb').read()
                hashsreturn.append(str(hashlib.md5(help).hexdigest()))
                self.HashSave[(Path + item.decode('utf-8')).replace(os.getcwd()+'/', '')] = str(hashlib.md5(help).hexdigest())
        
        return hashsreturn

if(False):
    while True:
        port_num = 12345     #input("Port? ")
        try:
            port_num = int(port_num)
            break
        except ValueError:
            pass

    ThreadedServer('', port_num).listen()

import os
os.chdir('/home/pi/Desktop/PyServer')
Server = ThreadedServer('', 12345)
Server.updateHashs('/home/pi/Desktop/PyServer')
#print(Server.GetHash('AllFiles'))

#print(Server.HashSave)

Server.listen()
