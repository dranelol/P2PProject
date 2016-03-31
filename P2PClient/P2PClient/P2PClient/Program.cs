﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P2PClient
{
    public class HostData
    {
        public string IP;
        public string Name;
        public int Port;

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

        public Client()
        {
            hostData.Name = "default";
            Thread client = new Thread(StartClient);
            client.Start();
        }

        public void Listen()
        {
            IPHostEntry serverInfo = Dns.Resolve(Dns.GetHostName());
            // TODO: resolve deprecation above

            IPAddress ip = serverInfo.AddressList[0];

            IPEndPoint endPoint = new IPEndPoint(ip, hostData.Port);

            Socket clientListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // listening loop

            Console.WriteLine("Listener started");

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

                    Console.WriteLine("received: " + clientData);

                    // server sends request to this client to send another client a file

                    // requestFileHeader - clientWantsFileIP - clientWantsFilePort - clientWantsFileName - wantedFile

                    // split clientData to get request information

                    

                    // strip <EOF>

                    clientData = clientData.Substring(0, clientData.Length - 5);

                    Console.WriteLine(clientData);

                    // split on hypen

                    string[] clientSplit = clientData.Split('-');

                    HostData requestClient = new HostData();

                    string header = clientSplit[0];

                    if(header == "requestFile")
                    {
                        requestClient.IP = clientSplit[1];
                        requestClient.Port = Convert.ToInt32(clientSplit[2]);
                        requestClient.Name = clientSplit[3];




                    }

                    else
                    {
                        byte[] msg = Encoding.ASCII.GetBytes("Invalid request header!");
                        handler.Send(msg);
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                    }

                    
                    
                }


            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("shutting down listener...");
            Console.Read();
        }

        public void StartClient()
        {
            byte[] dataBuffer = new Byte[1024];

            while (true)
            {
                try
                {
                    IPHostEntry hostInfo = Dns.Resolve(Dns.GetHostName());
                    // TODO: Resolve deprecation

                    IPAddress ip = hostInfo.AddressList[0];

                    hostData.IP = ip.ToString();

                    IPEndPoint endPoint = new IPEndPoint(ip, 8888);

                    Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


                    try
                    {


                        //while (true)
                        //{
                        // connect to server
                        

                        //byte[] message = Encoding.ASCII.GetBytes("Testing send<EOF>");

                        //dataBuffer = new Byte[1024];

                        // send stuff
                        //int sent = sender.Send(message);

                        // receive stuff
                        //int received = sender.Receive(dataBuffer);

                        //Console.WriteLine("Echoed: " + Encoding.ASCII.GetString(dataBuffer, 0, received));


                        Console.WriteLine("choose thing:");
                        Console.WriteLine("----------------------------------------------");
                        
                        Console.WriteLine("1: add host (self) to server");
                        Console.WriteLine("2: remove host (self) to server");
                        Console.WriteLine("3: add file to server (hosted by self)");
                        Console.WriteLine("4: remove file from server (hosted by self)");
                        Console.WriteLine("5: request file from server (hosted by other)");
                        Console.WriteLine("6: set unique host name (self)");
                        Console.WriteLine("7: set port for incoming connections (self)");
                        Console.WriteLine("8: set download folder location (self)");
                        Console.WriteLine("9: start listening for requests (self)");
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

                                byte[] message = Encoding.ASCII.GetBytes("addHost" + "-" + hostData.IP + "-" + hostData.Port.ToString() + "-" + hostData.Name + "<EOF>");

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

                                byte[] message = Encoding.ASCII.GetBytes("removeHost" + "-" + hostData.IP + "-" + hostData.Port.ToString() + "-" + hostData.Name + "<EOF>");

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

                                byte[] message = Encoding.ASCII.GetBytes("addFile" + "-" + hostData.IP + "-" + hostData.Port.ToString() + "-" + hostData.Name + "-" + fileName + "<EOF>");

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

                                byte[] message = Encoding.ASCII.GetBytes("removeFile" + "-" + hostData.IP + "-" + hostData.Port.ToString() + "-" + hostData.Name + "-" + fileName + "<EOF>");

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
                                // set port 
                                Console.WriteLine("Give port:");
                                string portString = Console.ReadLine();
                                int port = Convert.ToInt32(portString);

                                hostData.Port = port;
                            }

                                break;

                            case "8":
                            {
                                // set download folder
                                Console.WriteLine("Give download folder location with double backslashes:");
                                string folder = Console.ReadLine();

                                filesDirectory = folder;


                            }

                                break;

                            case "9":
                            {
                                Console.WriteLine("starting to listen...");


                                Thread requestListening = new Thread(Listen);
                                requestListening.Start();
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
            Client client = new Client();


            
            
            
            
            //StartClient();
        }
    }
}