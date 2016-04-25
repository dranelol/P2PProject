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
            hostData.Name = "default";
            hostData.ListenPort = listenPort;
            hostData.SendPort = sendPort;
            IPHostEntry hostInfo = Dns.GetHostEntry(Dns.GetHostName());

            // get ipv4 address

            IPAddress ip = Array.Find(
                hostInfo.AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);

            hostData.IP = ip.ToString();

            

            Thread client = new Thread(StartClient);
            client.Start();
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

            IPEndPoint endPoint = new IPEndPoint(ip, joinerListenPort);

            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // send message to joiner, adding self as a host
            sender.Connect(endPoint);

            Console.WriteLine("CLIENT THREAD:: Connected to: ", sender.RemoteEndPoint.ToString());

            byte[] message = Encoding.ASCII.GetBytes("joinCloud" + "-"
                + hostData.IP + "-"
                + hostData.ListenPort.ToString() + "-"
                + hostData.SendPort.ToString() + "-"
                + hostData.Name + "<EOF>");

            dataBuffer = new Byte[1024];

            int sent = sender.Send(message);

            Console.WriteLine("CLIENT THREAD:: receiving...");

            int received = sender.Receive(dataBuffer);

            string receiveString = "";

            Console.WriteLine("CLIENT THREAD:: received bytes: " + received);
            // while there's data to accept
            while (received > 0)
            {
                Console.WriteLine("CLIENT THREAD:: received bytes: " + received);

                // decode data sent

                receiveString += Encoding.ASCII.GetString(dataBuffer, 0, received);

                //clientData += Encoding.ASCII.GetString(dataBuffer, 0, received);

                dataBuffer = new Byte[1024];

                Console.WriteLine("CLIENT THREAD:: receiving...");

                received = sender.Receive(dataBuffer);
            }

            Console.WriteLine("CLIENT THREAD:: received string from client: " + receiveString);

            if(receiveString == "Host already added")
            {

            }

            else
            {
                DeserializeFileDirectoryString(receiveString);
            }

            // add self to filehost data
            if(Hosts.Contains(hostData) == false)
            {
                Hosts.Add(hostData);
            }
            

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

            Console.WriteLine("File name:");

            string fileName = Console.ReadLine();

            byte[] message = Encoding.ASCII.GetBytes("addFile" + "-"
                + hostData.IP + "-"
                + hostData.ListenPort.ToString() + "-"
                + hostData.SendPort.ToString() + "-"
                + hostData.Name + "-"
                + fileName + "<EOF>");

            dataBuffer = new Byte[1024];

            // send addfile request to all known clients

            foreach(HostData host in Hosts)
            {
                string hostIP = host.IP;

                IPHostEntry hostInfo = Dns.GetHostEntry(hostIP);

                // get ipv4 address

                IPAddress ip = Array.Find(
                    hostInfo.AddressList,
                    a => a.AddressFamily == AddressFamily.InterNetwork);
                 
                Console.WriteLine(ip.ToString());

                IPEndPoint endPoint = new IPEndPoint(ip, host.ListenPort);

                Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // send message to server, add file to server
                sender.Connect(endPoint);

                int sent = sender.Send(message);

                //int sent sender.SendTo()

                int received = sender.Receive(dataBuffer);

                Console.WriteLine("From client we connected to: " + Encoding.ASCII.GetString(dataBuffer, 0, received));

                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }

            // add file to our own filehost directory

            if (FileDirectory.ContainsKey(fileName) == true)
            {
                if (FileDirectory[fileName].Contains(hostData) == false)
                {
                    FileDirectory[fileName].Add(hostData);
                }

                else
                {

                }

            }

            else
            {
                FileDirectory[fileName] = new List<HostData>();

                FileDirectory[fileName].Add(hostData);
            }

            
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
            // set ip for joining cloud
            Console.WriteLine("Give IP:");
            string ipString = Console.ReadLine();

            joinIP = ipString;
        }

        public void SetJoinPort()
        {
            // set port for joining cloud
            Console.WriteLine("Give port:");
            string portString = Console.ReadLine();
            int port = Convert.ToInt32(portString);

            joinerListenPort = port;
        }

        public void SetListenPort()
        {
            
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
            Console.WriteLine("starting to listen...");

            //Thread requestListening = new Thread(SendFileThread);
            //requestListening.Start();

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
                    Console.WriteLine("LISTENER RECEIVE THREAD:: waiting...");

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

                    Console.WriteLine("LISTENER RECEIVE THREAD:: received: " + clientData);

                    // handle messages

                    // strip <EOF>

                    clientData = clientData.Substring(0, clientData.Length - 5);

                    Console.WriteLine("LISTENER RECEIVE THREAD:: " + clientData);

                    // split on hypen

                    string[] clientSplit = clientData.Split('-');

                    foreach (string section in clientSplit)
                    {
                        Console.WriteLine("LISTENER RECEIVE THREAD:: " + section);
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
                                client.ListenPort = Convert.ToInt32(clientSplit[2]);
                                client.SendPort = Convert.ToInt32(clientSplit[3]);
                                client.Name = clientSplit[4];


                                if (Hosts.Contains(client) == false)
                                {
                                    // add host to list

                                    Hosts.Add(client);

                                    // let client know addition was successful

                                    byte[] successMsg = Encoding.ASCII.GetBytes("Addition of host successful");


                                    handler.Send(successMsg);
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
                                client.ListenPort = Convert.ToInt32(clientSplit[2]);
                                client.SendPort = Convert.ToInt32(clientSplit[3]);
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
                                client.ListenPort = Convert.ToInt32(clientSplit[2]);
                                client.SendPort = Convert.ToInt32(clientSplit[3]);
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
                                client.ListenPort = Convert.ToInt32(clientSplit[2]);
                                client.SendPort = Convert.ToInt32(clientSplit[3]);
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

                                HostData requestClient = new HostData();

                                requestClient.IP = clientSplit[1];
                                requestClient.ListenPort = Convert.ToInt32(clientSplit[2]);
                                requestClient.Name = clientSplit[3];

                                Console.WriteLine("Send file to client");

                                // get ipv4 address for requesting client

                                // instead of telling a client to send file to another client, just straight up send the file to the client 

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

                                /*

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

                                            IPEndPoint fromClientEndPoint = new IPEndPoint(fromClientIP, fromClient.SendPort);

                                            Socket fromClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                                            fromClientSocket.Connect(fromClientEndPoint);

                                            // build request to fromClient to send file to Client

                                            Console.WriteLine("Connected to: ", fromClientSocket.RemoteEndPoint.ToString());

                                            byte[] fileRequestMessage = Encoding.ASCII.GetBytes("requestFile" + "-" + client.IP + "-" + client.ListenPort.ToString() + "-" + client.Name + "-" + file + "<EOF>");

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
                                 * */

                                break;
                            }

                        #endregion

                        #region join cloud
                        case "joinCloud":
                            {
                                // create hostdata from client data string
                                
                                HostData client = new HostData();

                                client.IP = clientSplit[1];
                                client.ListenPort = Convert.ToInt32(clientSplit[2]);
                                client.SendPort = Convert.ToInt32(clientSplit[3]);
                                client.Name = clientSplit[4];

                                if (Hosts.Contains(client) == false)
                                {
                                    // add host to list

                                    Console.WriteLine("LISTENER RECEIVE THREAD:: adding host to list: " + client.Name);

                                    Hosts.Add(client);

                                    // let client know addition was successful

                                    byte[] successMsg = Encoding.ASCII.GetBytes("Addition of host successful");
                                    //ArraySegment<byte> successMsg = new ArraySegment<byte>(Encoding.ASCII.GetBytes("Addition of host successful"));

                                    // also give client known hostfile structure

                                    string hostFile = SerializeFileDirectory();

                                    byte[] hostFileMsg = Encoding.ASCII.GetBytes(hostFile);

                                    //ArraySegment<byte> hostFileMsg = new ArraySegment<byte>(Encoding.ASCII.GetBytes(hostFile));

                                   // List<ArraySegment<byte>> messages = new List<ArraySegment<byte>>();

                                    //Console.WriteLine("LISTENER RECEIVE THREAD:: message0: " + "Addition of host successful");
                                    //Console.WriteLine("LISTENER RECEIVE THREAD:: message1: " + hostFile);

                                    //messages.Add(successMsg);
                                    //messages.Add(hostFileMsg);

                                    //Console.WriteLine("LISTENER RECEIVE THREAD:: message0: " + Encoding.ASCII.GetString(messages[0].ToArray<byte>(), 0, messages[0].Count));
                                    //Console.WriteLine("LISTENER RECEIVE THREAD:: message1: " + Encoding.ASCII.GetString(messages[1].ToArray<byte>(), 0, messages[1].Count));

                                    Console.WriteLine("LISTENER RECEIVE THREAD:: message0: " + Encoding.ASCII.GetString(successMsg, 0, successMsg.Length));
                                    Console.WriteLine("LISTENER RECEIVE THREAD:: message1: " + Encoding.ASCII.GetString(hostFileMsg, 0, hostFileMsg.Length));

                                    //handler.Send(successMsg);
                                    //handler.Send(messages);

                                    //handler.Send(successMsg);
                                    handler.Send(hostFileMsg);

                                    // send messages to all known clients besides this one that we have a new client

                                    foreach(HostData host in Hosts)
                                    {
                                        if(host != client)
                                        {
                                            Console.WriteLine("LISTENER RECEIVE THREAD:: sending addhost of " + client.Name + " to " + host.Name);
                                            byte[] message = Encoding.ASCII.GetBytes("addHost" + "-"
                                               + client.IP + "-"
                                               + client.ListenPort.ToString() + "-"
                                               + client.SendPort.ToString() + "-"
                                               + client.Name + "<EOF>");

                                            IPHostEntry hostInfo = Dns.GetHostEntry(host.IP);

                                            // get ipv4 address

                                            IPAddress hostIp = Array.Find(
                                                hostInfo.AddressList,
                                                a => a.AddressFamily == AddressFamily.InterNetwork);

                                            Console.WriteLine(ip.ToString());

                                            IPEndPoint hostEndPoint = new IPEndPoint(ip, host.ListenPort);

                                            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                                            sender.Connect(hostEndPoint);

                                            dataBuffer = new Byte[1024];

                                            int sent = sender.Send(message);

                                            int received = sender.Receive(dataBuffer);

                                            Console.WriteLine("LISTENER RECEIVE THREAD:: From server: " + Encoding.ASCII.GetString(dataBuffer, 0, received));

                                            sender.Shutdown(SocketShutdown.Both);

                                            sender.Close();
                                        }
                                        
                                    }
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
                            case "e":
                                {
                                    foreach(HostData host in Hosts)
                                    {
                                        Console.WriteLine(host.Name);
                                    }
                                }

                                break;

                            case "f":
                                {
                                    Console.WriteLine(SerializeFileDirectory());
                                }

                                break;
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

        public string SerializeFileDirectory()
        {
            string retString = "";

            if(Hosts.Count > 0 )
            {
                foreach(HostData host in Hosts)
                {
                    retString += host.IP.ToString() + "_" + host.Name + "_" + host.SendPort.ToString() + "_" + host.ListenPort.ToString() + ":";
                }

                retString += "%";

                foreach (string key in FileDirectory.Keys)
                {
                    retString += key + "-";
                    foreach (HostData host in FileDirectory[key])
                    {
                        retString += host.IP.ToString() + "_" + host.Name + "_" + host.SendPort.ToString() + "_" + host.ListenPort.ToString() + ":";
                    }

                    retString += "~";
                }
            }

            else
            {
                retString += "FirstJoin";
            }

            

            return retString;
        }

        public void DeserializeFileDirectoryString(string fileDirString)
        {
            Console.WriteLine(fileDirString);

            string[] hostSplit = fileDirString.Split('%');

            string[] firstHosts = hostSplit[0].Split(':');

            foreach(string host in firstHosts)
            {
                if(host.Length > 0)
                {
                    Console.WriteLine("Host: " + host);
                    Console.WriteLine("Hostlength: " + host.Length);
                    string[] hostData = host.Split('_');

                    HostData newHost = new HostData();

                    newHost.IP = hostData[0];
                    newHost.Name = hostData[1];
                    newHost.SendPort = Convert.ToInt32(hostData[2]);
                    newHost.ListenPort = Convert.ToInt32(hostData[3]);

                    if (Hosts.Contains(newHost) == false)
                    {
                        Console.WriteLine("Adding new host to hosts file: " + newHost.Name);
                        Hosts.Add(newHost);
                    }
                }
                
            }



            string[] files = hostSplit[1].Split('~');

            foreach(string file in files)
            {
                if(file.Length > 0)
                {
                    string[] fileData = file.Split('-');

                    string fileName = fileData[0];

                    string[] hosts = fileData[1].Split(':');

                    foreach (string host in hosts)
                    {
                        string[] hostData = host.Split('_');

                        HostData newHost = new HostData();

                        newHost.IP = hostData[0];
                        newHost.Name = hostData[1];
                        newHost.SendPort = Convert.ToInt32(hostData[2]);
                        newHost.ListenPort = Convert.ToInt32(hostData[3]);

                        /*
                        if(FileDirectory.ContainsKey(fileName) == false)
                        {
                            // filedirectory didn't contain filename, make new list of hosts for this file
                            FileDirectory[fileName] = new List<HostData>();

                            // add host to this file

                            FileDirectory[fileName].Add(newHost);
                        }

                        else
                        {
                            // filedirectory contains file, check if we already know host has this file
                            if(FileDirectory[fileName].Contains(newHost))
                            {
                                // nothing
                            }

                            else
                            {
                                FileDirectory[fileName].Add(newHost);
                            }

                        }
                         */
                    }
                }
                


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
