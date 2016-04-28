using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace P2PClient
{
    public class HostData
    {
        public string IP;
        public string Name;
        public int ListenSendPort;
        public int ListenReceivePort;

        public override bool Equals(object obj)
        {
            HostData other = (HostData)obj;

            return this.Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

    }

    class Client
    {
        private HostData hostData = new HostData();

        private string filesDirectory = "C:\\TempDir1\\";

        private string serverIP = "130.70.82.158";

        private int serverPort = 8888;

        private int listenerSendPort = 8886;

        private int listenerReceivePort = 8887;

        private string fileToBeReceived;

        public Client()
        {
            hostData.Name = "default";
            hostData.ListenSendPort = listenerSendPort;
            IPHostEntry hostInfo = Dns.GetHostEntry(Dns.GetHostName());

            // get ipv4 address

            IPAddress ip = Array.Find(
                hostInfo.AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);

            hostData.IP = ip.ToString();
            Thread client = new Thread(StartClient);
            client.Start();
        }

        public Client(int listenPort, int sendPort, string sIP, int server, string name)
        {
            serverIP = sIP;
            hostData.Name = name;
            hostData.ListenSendPort = sendPort;
            hostData.ListenReceivePort = listenPort;

            listenerReceivePort = listenPort;
            listenerSendPort = sendPort;
            serverPort = server;

            IPHostEntry hostInfo = Dns.GetHostEntry(Dns.GetHostName());

            // get ipv4 address

            IPAddress ip = Array.Find(
                hostInfo.AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);

            hostData.IP = ip.ToString();
            Thread client = new Thread(StartClient);
            client.Start();
        }

        public void ListenSend()
        {
            IPHostEntry listenerInfo = Dns.GetHostEntry(Dns.GetHostName());

            // get ipv4 address for self (listener)

            IPAddress ip = Array.Find(
                listenerInfo.AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);

            IPEndPoint endPoint = new IPEndPoint(ip, listenerSendPort);

            Socket clientListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // listening loop

            Console.WriteLine("LISTENER SEND THREAD:: Listener started");

            string clientData = null;

            byte[] dataBuffer = new Byte[1024];

            try
            {
                clientListener.Bind(endPoint);
                clientListener.Listen(10);

                // listen for connections forever
                while (true)
                {
                    Console.WriteLine("LISTENER SEND THREAD:: waiting...");

                    Socket handler = clientListener.Accept();

                    clientData = null;

                    // while there's data to accept
                    while (true)
                    {
                        dataBuffer = new Byte[1024];
                        int received = handler.Receive(dataBuffer);

                        // decode data sent

                        clientData += Encoding.ASCII.GetString(dataBuffer, 0, received);

                        if (clientData.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }

                    }

                    Console.WriteLine("LISTENER SEND THREAD:: received: " + clientData);

                    // server sends request to this client to send another client a file

                    // requestFileHeader - clientWantsFileIP - clientWantsFilePort - clientWantsFileName - wantedFile

                    // split clientData to get request information

                    // strip <EOF>

                    clientData = clientData.Substring(0, clientData.Length - 5);

                    Console.WriteLine("LISTENER SEND THREAD:: " + clientData);

                    // split on hypen

                    string[] clientSplit = clientData.Split('-');

                    HostData requestClient = new HostData();

                    string header = clientSplit[0];

                    switch(header)
                    {
                        case "requestFile":
                        {
                            requestClient.IP = clientSplit[1];
                            requestClient.ListenReceivePort = Convert.ToInt32(clientSplit[2]);
                            requestClient.Name = clientSplit[3];

                            // open socket to request client


                            // get ipv4 address for requesting client


                            IPHostEntry requestClientInfo = Dns.GetHostEntry(requestClient.IP);

                            IPAddress requestClientIP = Array.Find(
                                requestClientInfo.AddressList,
                                a => a.AddressFamily == AddressFamily.InterNetwork);

                            IPEndPoint requestClientEndPoint = new IPEndPoint(requestClientIP, requestClient.ListenReceivePort);

                            Socket requestClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                            requestClientSocket.Connect(requestClientEndPoint);

                            // send file to client

                            string fileName = filesDirectory + clientSplit[4];

                            try
                            {
                                Console.WriteLine("LISTENER SEND THREAD:: Sending file: " + fileName);

                                //byte[] preBuffer = Encoding.ASCII.GetBytes(fileName + "-");
                                //byte[] postBuffer = Encoding.ASCII.GetBytes("<EOF>");

                                

                                //requestClientSocket.SendFile(fileName, preBuffer, postBuffer, TransmitFileOptions.UseDefaultWorkerThread);
                                requestClientSocket.SendFile(fileName);
                                Console.WriteLine("LISTENER SEND THREAD:: SENT FILE");
                                requestClientSocket.Shutdown(SocketShutdown.Both);
                                requestClientSocket.Close();
                            }

                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                            }


                            break;
                        }

                        default:
                            byte[] msg = Encoding.ASCII.GetBytes("Invalid request header!");
                            handler.Send(msg);
                            handler.Shutdown(SocketShutdown.Both);
                            handler.Close();

                            break;
                    }

                    
                    
                }


            }

            catch (Exception e)
            {
                Console.WriteLine("LISTENER SEND THREAD:: " + e.ToString());
            }

            Console.WriteLine("LISTENER SEND THREAD:: shutting down listener...");
            Console.Read();
        }

        public void ListenReceive()
        {
            IPHostEntry listenerInfo = Dns.GetHostEntry(Dns.GetHostName());

            // get ipv4 address for self (listener)

            IPAddress ip = Array.Find(
                listenerInfo.AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);

            IPEndPoint endPoint = new IPEndPoint(ip, listenerReceivePort);

            Socket clientListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // listening loop

            Console.WriteLine("LISTENER RECEIVE THREAD:: Listener started");

            string clientData = null;

            byte[] dataBuffer = new Byte[1024];

            try
            {
                clientListener.Bind(endPoint);
                clientListener.Listen(10);

                // listen for connections forever
                while (true)
                {
                    Console.WriteLine("LISTENER RECEIVE THREAD:: waiting...");

                    Socket handler = clientListener.Accept();

                    clientData = null;

                    string fileName = fileToBeReceived;

                    // dump file to disk

                    string filePath = System.IO.Path.Combine(filesDirectory, fileName);

                    Console.WriteLine("LISTENER RECEIVE THREAD:: writing to file: " + filePath);

                    FileStream fileStream = File.Create(filePath);

                    dataBuffer = new Byte[1024];
                    int received = handler.Receive(dataBuffer);
                    Console.WriteLine("received bytes: " + received);
                    // while there's data to accept
                    while (received > 0)
                    {
                        Console.WriteLine("received bytes: " + received);

                        // decode data sent

                        fileStream.Write(dataBuffer, 0, received);

                        //clientData += Encoding.ASCII.GetString(dataBuffer, 0, received);

                        dataBuffer = new Byte[1024];
                        received = handler.Receive(dataBuffer);
                    }

                    Console.WriteLine("LISTENER RECEIVE THREAD:: file written");

                    fileStream.Close();

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }

            }

            catch (Exception e)
            {
                Console.WriteLine("LISTENER RECEIVE THREAD:: " + e.ToString());
            }

            Console.WriteLine("LISTENER RECEIVE THREAD:: shutting down listener...");
            Console.Read();
        }

        public void StartClient()
        {
            byte[] dataBuffer = new Byte[1024];

            while (true)
            {
                try
                {
                    IPHostEntry hostInfo = Dns.GetHostEntry(serverIP);

                    // get ipv4 address

                    IPAddress ip = Array.Find(
                        hostInfo.AddressList,
                        a => a.AddressFamily == AddressFamily.InterNetwork);

                    Console.WriteLine(ip.ToString());

                    IPEndPoint endPoint = new IPEndPoint(ip, serverPort);

                    Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    try
                    {

                        Console.WriteLine("choose thing:");
                        Console.WriteLine("----------------------------------------------");
                        
                        Console.WriteLine("1: add host (self) to server");
                        Console.WriteLine("2: remove host (self) from server");
                        Console.WriteLine("3: add file to server (hosted by self)");
                        Console.WriteLine("4: remove file from server (hosted by self)");
                        Console.WriteLine("5: request file from server (hosted by other)");
                        Console.WriteLine("6: set unique host name (self)");
                        Console.WriteLine("7: set listening port for sending files (self)");
                        Console.WriteLine("8: set listening port for receiving files (self)");
                        Console.WriteLine("9: start listening threads (self)");
                        Console.WriteLine("0: set download folder location (self)");
                        Console.WriteLine("----------------------------------------------");

                        string response = Console.ReadLine();

                        //byte[] message = Encoding.ASCII.GetBytes("addHost" + "-" + hostData.IP + "-" + hostData.Name + "<EOF>");

                        //dataBuffer = new Byte[1024];

                        //int sent = sender.Send(message);

                        //int received = sender.Receive(dataBuffer);

                        //Console.WriteLine("From server: " + Encoding.ASCII.GetString(dataBuffer, 0, received));

                        
                        switch(response)
                        {
                            case "1":
                            {
                                // send message to server, adding self as a host
                                sender.Connect(endPoint);

                                Console.WriteLine("Connected to: ", sender.RemoteEndPoint.ToString());

                                byte[] message = Encoding.ASCII.GetBytes("addHost" + "-" 
                                    + hostData.IP + "-" 
                                    + hostData.ListenReceivePort.ToString() + "-"
                                    + hostData.ListenSendPort.ToString() + "-" 
                                    + hostData.Name + "<EOF>");

                                dataBuffer = new Byte[1024];

                                int sent = sender.Send(message);

                                int received = sender.Receive(dataBuffer);

                                Console.WriteLine("From server: " + Encoding.ASCII.GetString(dataBuffer, 0, received));

                                sender.Shutdown(SocketShutdown.Both);
                                sender.Close();
                            }

                                break;

                            case "2":
                            {
                                // send message to server, removing self as a host
                                sender.Connect(endPoint);

                                Console.WriteLine("Connected to: ", sender.RemoteEndPoint.ToString());

                                byte[] message = Encoding.ASCII.GetBytes("removeHost" + "-" 
                                    + hostData.IP + "-"
                                    + hostData.ListenReceivePort.ToString() + "-"
                                    + hostData.ListenSendPort.ToString() + "-" 
                                    + hostData.Name + "<EOF>");

                                dataBuffer = new Byte[1024];

                                int sent = sender.Send(message);

                                int received = sender.Receive(dataBuffer);

                                Console.WriteLine("From server: " + Encoding.ASCII.GetString(dataBuffer, 0, received));

                                sender.Shutdown(SocketShutdown.Both);
                                sender.Close();
                            }

                                break;

                            case "3":
                            {
                                // send message to server, add file to server
                                sender.Connect(endPoint);

                                Console.WriteLine("Connected to: ", sender.RemoteEndPoint.ToString());

                                Console.WriteLine("File name:");

                                string fileName = Console.ReadLine();

                                byte[] message = Encoding.ASCII.GetBytes("addFile" + "-" 
                                    + hostData.IP + "-"
                                    + hostData.ListenReceivePort.ToString() + "-"
                                    + hostData.ListenSendPort.ToString() + "-" 
                                    + hostData.Name + "-" 
                                    + fileName + "<EOF>");

                                dataBuffer = new Byte[1024];

                                int sent = sender.Send(message);

                                int received = sender.Receive(dataBuffer);

                                Console.WriteLine("From server: " + Encoding.ASCII.GetString(dataBuffer, 0, received));

                                sender.Shutdown(SocketShutdown.Both);
                                sender.Close();
                            }

                                break;

                            case "4":
                            {
                                // send message to server, remove file from server
                                sender.Connect(endPoint);

                                Console.WriteLine("Connected to: ", sender.RemoteEndPoint.ToString());

                                Console.WriteLine("File name:");

                                string fileName = Console.ReadLine();

                                byte[] message = Encoding.ASCII.GetBytes("removeFile" + "-" 
                                    + hostData.IP + "-"
                                    + hostData.ListenReceivePort.ToString() + "-"
                                    + hostData.ListenSendPort.ToString() + "-" 
                                    + hostData.Name + "-" 
                                    + fileName + "<EOF>");

                                dataBuffer = new Byte[1024];

                                int sent = sender.Send(message);

                                int received = sender.Receive(dataBuffer);

                                Console.WriteLine("From server: " + Encoding.ASCII.GetString(dataBuffer, 0, received));

                                sender.Shutdown(SocketShutdown.Both);
                                sender.Close();
                            }

                                break;
                            case "5":
                            {
                                // request file from server
                                sender.Connect(endPoint);



                                //Console.WriteLine("Connected to: ", (IPEndPoint)(sender.RemoteEndPoint).

                                Console.WriteLine("File name:");

                                string fileName = Console.ReadLine();

                                byte[] message = Encoding.ASCII.GetBytes("requestFile" + "-" 
                                    + hostData.IP + "-"
                                    + hostData.ListenReceivePort.ToString() + "-"
                                    + hostData.ListenSendPort.ToString() + "-" 
                                    + hostData.Name + "-" 
                                    + fileName + "<EOF>");

                                dataBuffer = new Byte[1024];

                                int sent = sender.Send(message);

                                fileToBeReceived = fileName;

                                //int received = sender.Receive(dataBuffer);

                                //Console.WriteLine("From server: " + Encoding.ASCII.GetString(dataBuffer, 0, received));

                                sender.Shutdown(SocketShutdown.Both);
                                sender.Close();

                            }

                                break;

                            case "6":
                            {
                                // set host name
                                Console.WriteLine("Give name:");
                                string name = Console.ReadLine();

                                hostData.Name = name;
                            }

                                break;

                            case "7":
                            {
                                // set port for sending files
                                Console.WriteLine("Give port:");
                                string portString = Console.ReadLine();
                                int port = Convert.ToInt32(portString);

                                hostData.ListenSendPort = port;
                                listenerSendPort = port;
                            }

                                break;

                            case "8":
                            {
                                // set port for receiving files
                                Console.WriteLine("Give port:");
                                string portString = Console.ReadLine();
                                int port = Convert.ToInt32(portString);

                                hostData.ListenReceivePort = port;
                                listenerReceivePort = port;


                            }

                                break;

                            case "9":
                            {
                                Console.WriteLine("starting to listen...");

                                Thread requestListening = new Thread(ListenSend);
                                requestListening.Start();

                                Thread receiveListening = new Thread(ListenReceive);
                                receiveListening.Start();

                            }

                                break;

                            case "0":
                            {
                                // set download folder
                                Console.WriteLine("Give download folder location with double backslashes:");
                                string folder = Console.ReadLine();

                                filesDirectory = folder;
                            }

                                break;

                            default:
                                Console.WriteLine("You goofed! Try again.");
                                break;
                        }
                        

                        


                        //}


                    }

                    catch (ArgumentNullException ane)
                    {
                        Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                    }
                    catch (SocketException se)
                    {
                        Console.WriteLine("SocketException : {0}", se.ToString());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Unexpected exception : {0}", e.ToString());
                    }


                }

                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Console.WriteLine("Done...");
                Console.ReadLine();
            }

            
        }

        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Client client = new Client(
                    Convert.ToInt32(args[0]),
                    Convert.ToInt32(args[1]),
                    args[2],
                    Convert.ToInt32(args[3]),
                    args[4]
                    );
            }

            else
            {
                Client client = new Client();
            }
            

            
            
            
            
            
            //StartClient();
        }
    }
}
