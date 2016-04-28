using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PServer
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


    public class Server
    {
        public Dictionary<string, List<HostData>> FileDirectory = new Dictionary<string, List<HostData>>();

        public List<HostData> Hosts = new List<HostData>();

        public int serverListenPort = 8888;

        public Server()
        {
            StartServer();
        }

        public void StartServer()
        {
            IPHostEntry serverInfo = Dns.GetHostEntry(Dns.GetHostName());

            // get ipv4 address for self (server)

            IPAddress ip = Array.Find(
                serverInfo.AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);

            IPEndPoint endPoint = new IPEndPoint(ip, serverListenPort);

            Socket serverListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // listening loop

            Console.WriteLine("Server started");

            string clientData = null;

            byte[] dataBuffer = new Byte[1024];

            try
            {
                serverListener.Bind(endPoint);
                serverListener.Listen(10);

                // listen for connections forever
                while(true)
                {
                    Console.WriteLine("waiting...");

                    Socket handler = serverListener.Accept();

                    clientData = null;

                    // while there's data to accept
                    while(true)
                    {
                        dataBuffer = new Byte[1024];
                        int received = handler.Receive(dataBuffer);
                        
                        // decode data sent

                        clientData += Encoding.ASCII.GetString(dataBuffer, 0, received);

                        if(clientData.IndexOf("<EOF>") > -1)
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

                    foreach(string section in clientSplit)
                    {
                        Console.WriteLine(section);
                    }

                    // switch on header

                    switch(clientSplit[0])
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

                            if(Hosts.Contains(client) == true)
                            {
                                string file = clientSplit[5];

                                // add file to file listing

                                if(FileDirectory.ContainsKey(file) == true)
                                {
                                    if(FileDirectory[file].Contains(client) == false)
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
                                    if(FileDirectory[file].Count > 0)
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
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("shutting down server...");
            Console.Read();

        }

        public static void Main(string[] args)
        {
            Server server = new Server();

            if (args.Length > 0)
            {
                server.serverListenPort = Convert.ToInt32(args[0]);
                
            }
        }
    }
}


