using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace P2PDist
{
    public class HostData
    {
        public string IP;
        public string Name;
        public int SendPort;
        public int ListenPort;

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

    class DistributedClient
    {
        private HostData hostData = new HostData();

        public Dictionary<string, List<HostData>> FileDirectory = new Dictionary<string, List<HostData>>();

        public List<HostData> Hosts = new List<HostData>();

        private string filesDirectory = "C:\\TempDir1\\";

        private string joinIP = "130.70.82.158";

        private int listenPort = 8886;

        private int sendPort = 8887;

        private int joinerListenPort = 8888;

        private string fileToBeReceived;

        public DistributedClient()
        {

        }

        public DistributedClient(string joinIP)
        {
            StartClient();
        }

        public void AddSelfToCloud()
        {
            byte[] dataBuffer = new Byte[1024];

            // first add self to joiner 

            IPHostEntry hostInfo = Dns.GetHostEntry(joinIP);

            // get ipv4 address

            IPAddress ip = Array.Find(
                hostInfo.AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);

            Console.WriteLine(ip.ToString());

            IPEndPoint endPoint = new IPEndPoint(ip, joinerListenPort);

            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // send message to joiner, adding self as a host
            sender.Connect(endPoint);

            Console.WriteLine("Connected to: ", sender.RemoteEndPoint.ToString());

            byte[] message = Encoding.ASCII.GetBytes("addHost" + "-"
                + hostData.IP + "-"
                + hostData.ListenPort.ToString() + "-"
                + hostData.SendPort.ToString() + "-"
                + hostData.Name + "<EOF>");

            dataBuffer = new Byte[1024];

            int sent = sender.Send(message);

            // joiner responds with his host/file structure 
            int received = sender.Receive(dataBuffer);

            Console.WriteLine("From joined client: " + Encoding.ASCII.GetString(dataBuffer, 0, received));

            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        public void RemoveSelfFromCloud()
        {
            byte[] dataBuffer = new Byte[1024];

            // first add self to joiner 

            IPHostEntry hostInfo = Dns.GetHostEntry(joinIP);

            // get ipv4 address

            IPAddress ip = Array.Find(
                hostInfo.AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);

            Console.WriteLine(ip.ToString());

            IPEndPoint endPoint = new IPEndPoint(ip, joinerListenPort);

            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // send message to server, removing self as a host
            sender.Connect(endPoint);

            Console.WriteLine("Connected to: ", sender.RemoteEndPoint.ToString());

            byte[] message = Encoding.ASCII.GetBytes("removeHost" + "-"
                + hostData.IP + "-"
                + hostData.ListenPort.ToString() + "-"
                + hostData.SendPort.ToString() + "-"
                + hostData.Name + "<EOF>");

            dataBuffer = new Byte[1024];

            int sent = sender.Send(message);

            int received = sender.Receive(dataBuffer);

            Console.WriteLine("From server: " + Encoding.ASCII.GetString(dataBuffer, 0, received));

            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        public void RequestFileFromCloud()
        {
            byte[] dataBuffer = new Byte[1024];

            // first add self to joiner 

            IPHostEntry hostInfo = Dns.GetHostEntry(joinIP);

            // get ipv4 address

            IPAddress ip = Array.Find(
                hostInfo.AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);

            Console.WriteLine(ip.ToString());

            IPEndPoint endPoint = new IPEndPoint(ip, joinerListenPort);

            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // request file from server
            sender.Connect(endPoint);

            //Console.WriteLine("Connected to: ", (IPEndPoint)(sender.RemoteEndPoint).

            Console.WriteLine("File name:");

            string fileName = Console.ReadLine();

            byte[] message = Encoding.ASCII.GetBytes("requestFile" + "-"
                + hostData.IP + "-"
                + hostData.ListenPort.ToString() + "-"
                + hostData.SendPort.ToString() + "-"
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

        public void AddFileToCloud()
        {
            byte[] dataBuffer = new Byte[1024];

            // first add self to joiner 

            IPHostEntry hostInfo = Dns.GetHostEntry(joinIP);

            // get ipv4 address

            IPAddress ip = Array.Find(
                hostInfo.AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);

            Console.WriteLine(ip.ToString());

            IPEndPoint endPoint = new IPEndPoint(ip, joinerListenPort);

            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // send message to server, add file to server
            sender.Connect(endPoint);

            Console.WriteLine("Connected to: ", sender.RemoteEndPoint.ToString());

            Console.WriteLine("File name:");


            string fileName = Console.ReadLine();

            byte[] message = Encoding.ASCII.GetBytes("addFile" + "-"
                + hostData.IP + "-"
                + hostData.ListenPort.ToString() + "-"
                + hostData.SendPort.ToString() + "-"
                + hostData.Name + "-"
                + fileName + "<EOF>");

            dataBuffer = new Byte[1024];

            int sent = sender.Send(message);

            int received = sender.Receive(dataBuffer);

            Console.WriteLine("From server: " + Encoding.ASCII.GetString(dataBuffer, 0, received));

            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        public void RemoveFileFromCloud()
        {
            byte[] dataBuffer = new Byte[1024];

            // first add self to joiner 

            IPHostEntry hostInfo = Dns.GetHostEntry(joinIP);

            // get ipv4 address

            IPAddress ip = Array.Find(
                hostInfo.AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);

            Console.WriteLine(ip.ToString());

            IPEndPoint endPoint = new IPEndPoint(ip, joinerListenPort);

            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // send message to server, remove file from server
            sender.Connect(endPoint);

            Console.WriteLine("Connected to: ", sender.RemoteEndPoint.ToString());

            Console.WriteLine("File name:");

            string fileName = Console.ReadLine();

            byte[] message = Encoding.ASCII.GetBytes("removeFile" + "-"
                + hostData.IP + "-"
                + hostData.ListenPort.ToString() + "-"
                + hostData.SendPort.ToString() + "-"
                + hostData.Name + "-"
                + fileName + "<EOF>");

            dataBuffer = new Byte[1024];

            int sent = sender.Send(message);

            int received = sender.Receive(dataBuffer);

            Console.WriteLine("From server: " + Encoding.ASCII.GetString(dataBuffer, 0, received));

            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        public void SetJoinIP()
        {
            byte[] dataBuffer = new Byte[1024];

            // first add self to joiner 

            IPHostEntry hostInfo = Dns.GetHostEntry(joinIP);

            // get ipv4 address

            IPAddress ip = Array.Find(
                hostInfo.AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);

            Console.WriteLine(ip.ToString());

            IPEndPoint endPoint = new IPEndPoint(ip, joinerListenPort);

            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void SetJoinPort()
        {
            byte[] dataBuffer = new Byte[1024];

            // first add self to joiner 

            IPHostEntry hostInfo = Dns.GetHostEntry(joinIP);

            // get ipv4 address

            IPAddress ip = Array.Find(
                hostInfo.AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);

            Console.WriteLine(ip.ToString());

            IPEndPoint endPoint = new IPEndPoint(ip, joinerListenPort);

            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void SetListenPort()
        {
            byte[] dataBuffer = new Byte[1024];

            // first add self to joiner 

            IPHostEntry hostInfo = Dns.GetHostEntry(joinIP);

            // get ipv4 address

            IPAddress ip = Array.Find(
                hostInfo.AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);

            Console.WriteLine(ip.ToString());

            IPEndPoint endPoint = new IPEndPoint(ip, joinerListenPort);

            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // set port for receiving files
            Console.WriteLine("Give port:");
            string portString = Console.ReadLine();
            int port = Convert.ToInt32(portString);

            hostData.ListenPort = port;
            listenPort = port;
        }

        public void SetSendPort()
        {
            byte[] dataBuffer = new Byte[1024];

            // first add self to joiner 

            IPHostEntry hostInfo = Dns.GetHostEntry(joinIP);

            // get ipv4 address

            IPAddress ip = Array.Find(
                hostInfo.AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);

            Console.WriteLine(ip.ToString());

            IPEndPoint endPoint = new IPEndPoint(ip, joinerListenPort);

            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // set port for sending files
            Console.WriteLine("Give port:");
            string portString = Console.ReadLine();
            int port = Convert.ToInt32(portString);

            hostData.SendPort = port;
            sendPort = port;
        }

        public void SetHostName()
        {
            // set host name
            Console.WriteLine("Give name:");
            string name = Console.ReadLine();

            hostData.Name = name;
        }

        public void StartThreads()
        {
            byte[] dataBuffer = new Byte[1024];

            // first add self to joiner 

            IPHostEntry hostInfo = Dns.GetHostEntry(joinIP);

            // get ipv4 address

            IPAddress ip = Array.Find(
                hostInfo.AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);

            Console.WriteLine(ip.ToString());

            IPEndPoint endPoint = new IPEndPoint(ip, joinerListenPort);

            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Console.WriteLine("starting to listen...");

            Thread requestListening = new Thread(SendFileThread);
            requestListening.Start();

            Thread receiveListening = new Thread(ListenThread);
            receiveListening.Start();
        }

        public void SendFileThread()
        {
            // this thread listens for requests to send files and send them

            IPHostEntry listenerInfo = Dns.GetHostEntry(Dns.GetHostName());

            // get ipv4 address for self (listener)

            IPAddress ip = Array.Find(
                listenerInfo.AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);

            IPEndPoint endPoint = new IPEndPoint(ip, sendPort);

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

                    // some client sends request to this client to send another client a file

                    // requestFileHeader - clientWantsFileIP - clientWantsFilePort - clientWantsFileName - wantedFile

                    // split clientData to get request information

                    // strip <EOF>

                    clientData = clientData.Substring(0, clientData.Length - 5);

                    Console.WriteLine("LISTENER SEND THREAD:: " + clientData);

                    // split on hypen

                    string[] clientSplit = clientData.Split('-');

                    HostData requestClient = new HostData();

                    string header = clientSplit[0];

                    switch (header)
                    {
                        case "requestFile":
                            {
                                requestClient.IP = clientSplit[1];
                                requestClient.ListenPort = Convert.ToInt32(clientSplit[2]);
                                requestClient.Name = clientSplit[3];

                                // open socket to request client


                                // get ipv4 address for requesting client


                                IPHostEntry requestClientInfo = Dns.GetHostEntry(requestClient.IP);

                                IPAddress requestClientIP = Array.Find(
                                    requestClientInfo.AddressList,
                                    a => a.AddressFamily == AddressFamily.InterNetwork);

                                IPEndPoint requestClientEndPoint = new IPEndPoint(requestClientIP, requestClient.ListenPort);

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

        public void ListenThread()
        {
            // this thread listens for requests (send file to client, add file to hosts table, remove file, add host, etc)

            IPHostEntry listenerInfo = Dns.GetHostEntry(Dns.GetHostName());

            // get ipv4 address for self (listener)

            IPAddress ip = Array.Find(
                listenerInfo.AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);

            IPEndPoint endPoint = new IPEndPoint(ip, listenPort);

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
                    Console.WriteLine("waiting...");

                    Socket handler = serverListener.Accept();

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
                            Console.WriteLine("breaking");
                            break;
                        }

                    }

                    Console.WriteLine("received: " + clientData);

                    // handle messages

                    // strip <EOF>

                    clientData = clientData.Substring(0, clientData.Length - 5);

                    Console.WriteLine(clientData);

                    // split on hypen

                    string[] clientSplit = clientData.Split('-');

                    foreach (string section in clientSplit)
                    {
                        Console.WriteLine(section);
                    }

                    // switch on header

                    switch (clientSplit[0])
                    {
                        #region add host
                        case "addHost":
                            {
                                // create hostdata from client data string

                                HostData client = new HostData();

                                client.IP = clientSplit[1];
                                client.ListenReceivePort = Convert.ToInt32(clientSplit[2]);
                                client.ListenSendPort = Convert.ToInt32(clientSplit[3]);
                                client.Name = clientSplit[4];


                                if (Hosts.Contains(client) == false)
                                {
                                    // add host to list

                                    Hosts.Add(client);

                                    // let client know addition was successful

                                    byte[] msg = Encoding.ASCII.GetBytes("Addition of host successful");

                                    handler.Send(msg);
                                }

                                else
                                {
                                    // let client know host was already added

                                    byte[] msg = Encoding.ASCII.GetBytes("Host already added");

                                    handler.Send(msg);
                                }

                                break;
                            }
                        #endregion

                        #region remove host
                        case "removeHost":
                            {
                                // create hostdata from client data string

                                HostData client = new HostData();

                                client.IP = clientSplit[1];
                                client.ListenReceivePort = Convert.ToInt32(clientSplit[2]);
                                client.ListenSendPort = Convert.ToInt32(clientSplit[3]);
                                client.Name = clientSplit[4];

                                if (Hosts.Contains(client) == true)
                                {
                                    // remove host from list

                                    Hosts.Remove(client);

                                    // let client know removal was successful

                                    byte[] msg = Encoding.ASCII.GetBytes("Removal of host successful");

                                    handler.Send(msg);
                                }

                                else
                                {
                                    // let client know host doesnt exist

                                    byte[] msg = Encoding.ASCII.GetBytes("Host doesnt exist");

                                    handler.Send(msg);
                                }

                                break;
                            }
                        #endregion

                        #region add file
                        case "addFile":
                            {
                                // create hostdata from client data string

                                HostData client = new HostData();

                                client.IP = clientSplit[1];
                                client.ListenReceivePort = Convert.ToInt32(clientSplit[2]);
                                client.ListenSendPort = Convert.ToInt32(clientSplit[3]);
                                client.Name = clientSplit[4];

                                if (Hosts.Contains(client) == true)
                                {
                                    string file = clientSplit[5];

                                    // add file to file listing

                                    if (FileDirectory.ContainsKey(file) == true)
                                    {
                                        if (FileDirectory[file].Contains(client) == false)
                                        {
                                            FileDirectory[file].Add(client);

                                            byte[] msg = Encoding.ASCII.GetBytes("File added!");

                                            handler.Send(msg);
                                        }

                                        else
                                        {
                                            byte[] msg = Encoding.ASCII.GetBytes("File already added by this host");

                                            handler.Send(msg);
                                        }

                                    }

                                    else
                                    {
                                        FileDirectory[file] = new List<HostData>();

                                        FileDirectory[file].Add(client);

                                        byte[] msg = Encoding.ASCII.GetBytes("File added!");

                                        handler.Send(msg);
                                    }
                                }

                                else
                                {
                                    // let client know host doesnt exist

                                    byte[] msg = Encoding.ASCII.GetBytes("Host hasn't been added to host list yet");

                                    handler.Send(msg);
                                }

                                break;
                            }
                        #endregion

                        #region remove file
                        case "removeFile":
                            {
                                HostData client = new HostData();

                                client.IP = clientSplit[1];
                                client.ListenReceivePort = Convert.ToInt32(clientSplit[2]);
                                client.ListenSendPort = Convert.ToInt32(clientSplit[3]);
                                client.Name = clientSplit[4];

                                if (Hosts.Contains(client) == true)
                                {
                                    string file = clientSplit[5];

                                    // remove file from file listing

                                    if (FileDirectory.ContainsKey(file) == true)
                                    {
                                        if (FileDirectory[file].Contains(client) == true)
                                        {
                                            FileDirectory[file].Remove(client);

                                            byte[] msg = Encoding.ASCII.GetBytes("File removed successfully!");

                                            handler.Send(msg);
                                        }

                                        else
                                        {
                                            byte[] msg = Encoding.ASCII.GetBytes("File not added by this host");

                                            handler.Send(msg);
                                        }

                                    }

                                    else
                                    {
                                        byte[] msg = Encoding.ASCII.GetBytes("File not added by any hosts!");

                                        handler.Send(msg);
                                    }
                                }

                                else
                                {
                                    // let client know host doesnt exist

                                    byte[] msg = Encoding.ASCII.GetBytes("Host hasn't been added to host list yet");

                                    handler.Send(msg);
                                }

                                break;
                            }

                        #endregion

                        #region request file
                        case "requestFile":
                            {
                                // create hostdata from client data string

                                HostData client = new HostData();

                                client.IP = clientSplit[1];
                                client.ListenReceivePort = Convert.ToInt32(clientSplit[2]);
                                client.ListenSendPort = Convert.ToInt32(clientSplit[3]);
                                client.Name = clientSplit[4];
                                Console.WriteLine("Request file from client");
                                if (Hosts.Contains(client) == true)
                                {
                                    string file = clientSplit[5];

                                    if (FileDirectory.ContainsKey(file) == true)
                                    {
                                        if (FileDirectory[file].Count > 0)
                                        {
                                            // find first host that has file requested
                                            HostData fromClient = FileDirectory[file][0];

                                            // open socket to fromClient

                                            IPHostEntry fromClientHostInfo = Dns.GetHostEntry(fromClient.IP);

                                            IPAddress fromClientIP = Array.Find(
                                                fromClientHostInfo.AddressList,
                                                a => a.AddressFamily == AddressFamily.InterNetwork);

                                            IPEndPoint fromClientEndPoint = new IPEndPoint(fromClientIP, fromClient.ListenSendPort);

                                            Socket fromClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                                            fromClientSocket.Connect(fromClientEndPoint);

                                            // build request to fromClient to send file to Client

                                            Console.WriteLine("Connected to: ", fromClientSocket.RemoteEndPoint.ToString());

                                            byte[] fileRequestMessage = Encoding.ASCII.GetBytes("requestFile" + "-" + client.IP + "-" + client.ListenReceivePort.ToString() + "-" + client.Name + "-" + file + "<EOF>");

                                            dataBuffer = new Byte[1024];

                                            // send request to fromClient to send file to client

                                            int sent = fromClientSocket.Send(fileRequestMessage);

                                            //int received = fromClientSocket.Receive(dataBuffer);

                                            //Console.WriteLine("From request client: " + Encoding.ASCII.GetString(dataBuffer, 0, received));

                                            fromClientSocket.Shutdown(SocketShutdown.Both);
                                            fromClientSocket.Close();
                                        }

                                        else
                                        {
                                            byte[] msg = Encoding.ASCII.GetBytes("No hosts are hosting this file!");

                                            handler.Send(msg);
                                        }


                                    }

                                    else
                                    {
                                        byte[] msg = Encoding.ASCII.GetBytes("File not in directory!");

                                        handler.Send(msg);
                                    }


                                }

                                else
                                {
                                    // let client know host doesnt exist

                                    byte[] msg = Encoding.ASCII.GetBytes("Host hasn't been added to host list yet");

                                    handler.Send(msg);
                                }

                                break;
                            }

                        #endregion

                        default:
                            break;
                    }


                    //byte[] msg = Encoding.ASCII.GetBytes(clientData);

                    //handler.Send(msg);
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
                    try
                    {
                        Console.WriteLine("choose thing:");
                        Console.WriteLine("----------------------------------------------");
                        Console.WriteLine("1: add host (self) to cloud");
                        Console.WriteLine("2: remove host (self) from cloud");
                        Console.WriteLine("3: add file to cloud (hosted by self)");
                        Console.WriteLine("4: remove file from cloud (hosted by self)");
                        Console.WriteLine("5: request file from cloud (hosted by other)");
                        Console.WriteLine("6: set unique host name (self)");
                        Console.WriteLine("7: set port for listening for file requests (self)");
                        Console.WriteLine("8: set port for sending files (self)");
                        Console.WriteLine("9: set port for join listener");
                        Console.WriteLine("0: set ip for join listener");
                        Console.WriteLine("-: start the listening threads  (self)");
                        Console.WriteLine("----------------------------------------------");

                        string response = Console.ReadLine();


                        switch (response)
                        {
                            case "1":
                                {
                                    AddSelfToCloud();
                                }

                                break;

                            case "2":
                                {
                                    RemoveSelfFromCloud();
                                }

                                break;

                            case "3":
                                {
                                    AddFileToCloud();
                                }

                                break;

                            case "4":
                                {
                                    RemoveFileFromCloud();
                                }

                                break;
                            case "5":
                                {
                                    RequestFileFromCloud();
                                }

                                break;

                            case "6":
                                {
                                    SetHostName();
                                }

                                break;

                            case "7":
                                {
                                    SetListenPort();
                                }

                                break;

                            case "8":
                                {
                                    SetSendPort();
                                }

                                break;

                            case "9":
                                {
                                    SetJoinPort();
                                }

                                break;

                            case "0":
                                {
                                    SetJoinIP();
                                }

                                break;

                            case "-":
                                {
                                    StartThreads();
                                }

                                break;

                            default:
                                Console.WriteLine("You goofed! Try again.");
                                break;
                        }

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
                DistributedClient client = new DistributedClient(args[0]);
            }

            else
            {
                DistributedClient client = new DistributedClient();
            }
        }

    }
}
