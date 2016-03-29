using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P2PClient
{
    public struct HostData
    {
        string ip;
        string name;
    }

    class Client
    {
        public static void Listen()
        {
            IPHostEntry serverInfo = Dns.Resolve(Dns.GetHostName());
            // TODO: resolve deprecation above

            IPAddress ip = serverInfo.AddressList[0];

            IPEndPoint endPoint = new IPEndPoint(ip, 8889);

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

                    byte[] msg = Encoding.ASCII.GetBytes(clientData);

                    handler.Send(msg);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }


            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("shutting down listener...");
            Console.Read();
        }

        public static void StartClient()
        {
            byte[] dataBuffer = new Byte[1024];

            while (true)
            {
                try
                {
                    IPHostEntry hostInfo = Dns.Resolve(Dns.GetHostName());
                    // TODO: Resolve deprecation

                    IPAddress ip = hostInfo.AddressList[0];

                    IPEndPoint endPoint = new IPEndPoint(ip, 8888);

                    Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


                    try
                    {


                        //while (true)
                        //{
                        // connect to server
                        sender.Connect(endPoint);

                        Console.WriteLine("Connected to: ", sender.RemoteEndPoint.ToString());
                        byte[] message = Encoding.ASCII.GetBytes("Testing send<EOF>");

                        dataBuffer = new Byte[1024];

                        // send stuff
                        int sent = sender.Send(message);

                        // receive stuff
                        int received = sender.Receive(dataBuffer);

                        Console.WriteLine("Echoed: " + Encoding.ASCII.GetString(dataBuffer, 0, received));

                        sender.Shutdown(SocketShutdown.Both);
                        sender.Close();


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
            Thread requestListening = new Thread(Listen);
            requestListening.Start();
            StartClient();
        }
    }
}
