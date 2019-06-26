import socket
import threading


class ThreadedServer(object):
    def __init__(self, host, port):
        self.HOST = host
        self.PORT = port

        self.s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.s.bind((self.HOST, self.PORT))

    def listen(self):
        timeout = 60

        self.s.listen(10)
        while True:
            client, addr = self.sock.accept()
            client.settimeout(timeout)
            threading.Thread(target=self.listenToClient, args=(client, addr)).start()

    def listenToClient(self, client, addr):
        size = 4096

        while True:
            try:
                data = client.recv(size)
                if data:

                    response = data
                    client.send(response)
                else:
                    raise Exception('Client disconnected')
            except Exception:
                client.close()
                return False


if __name__ == "__main__":
    while True:
        port_num = input("Port? ")
        try:
            port_num = int(port_num)
            break
        except ValueError:
            pass

    ThreadedServer('', port_num).listen()
