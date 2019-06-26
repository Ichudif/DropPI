
import socket
import threading
import os


class ThreadedServer(object):
    def __init__(self, host, port):
        self.HOST = host
        self.PORT = port

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
                bytesize = int(client.recv(4).decode('utf-8'))
                total_data = ''
                
                print(bytesize)
                for i in range(0, bytesize):
                    total_data = total_data + client.recv(1).decode('ISO 8859-1')
                
                if(total_data):
                    response = str.encode('response: ' + total_data, 'ISO 8859-1')
                    print(total_data)
                    client.sendall(response)
                else:
                    raise Exception('Client disconnected')
            except BaseException as e:
                client.close()
                print(str(e))
                return False
            
    def GetHash(self, Path):
        if(os.path.exists(Path)):
            splitchar = '\\'
            if(Path.contains('\\\\')):
                splitchar = '\\'
                
            if(os.path.isdir(Path)):
                newPath = os.getcwd() + '/' + '/'.join(Path.split(splitchar)[:-1])
                FolderName = Path.split(splitchar)[-1:]
                HashFile = 'FolderHash_' + FolderName + '.hfile'
                
                return open(newPath + '/' + HashFile,'rb').read()
            else:
                return hashlib.md5(open(os.getcwd() + '/' + Path.replace(splitchar, '/'),'rb').read()).hexdigest()
            
    def WriteFile(self, Path, bytestowrite):
        f = open(os.getcwd() + Path, 'wb')
        for byte in bytestowrite:
            f.write(byte)
        f.close()
    
    def updateHashs(self, Path):
        splitchar = '\\'
        if(Path.contains('\\\\')):
            splitchar = '\\'
        
        
        for item in os.listdir(Path):                      #os.fsencode(os.getcwd())
            hashs = []
            if(os.isfolder(item)):
                hashs = updateHashs(item)
                md5var = hashlib.md5()
                for hash1 in hashs:
                    md5var.update(hash1)
                
                newPath = os.getcwd() + '/' + '/'.join(Path.split(splitchar)[:-1])
                FolderName = Path.split(splitchar)[-1:]
                HashFile = 'FolderHash_' + FolderName + '.hfile'
                
                HashFileWriter = open(newPath + '/' + HashFile, 'w+')
                HashFileWriter.write(md5var.hexdigest())
                hashs.append(md5var.hexdigest())
            else:
                hashs.append(hashlib.md5(open(item,'rb').read()).hexdigest())
        return hashs

if __name__ == "__main__":
    while True:
        port_num = 12345     #input("Port? ")
        try:
            port_num = int(port_num)
            break
        except ValueError:
            pass

    ThreadedServer('', port_num).listen()



