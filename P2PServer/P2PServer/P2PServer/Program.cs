using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PServer
{
    public struct HostData
    {
        string ip;
        string name;
    }


    public class Server
    {
        public Dictionary<string, List<HostData>> FileDirectory = new Dictionary<string, List<HostData>>();

        public List<HostData> Hosts = new List<HostData>();

        public static void Start()
        {
            IPHostEntry serverInfo = Dns.Resolve(Dns.GetHostName());
            // TODO: resolve deprecation above

            IPAddress ip = serverInfo.AddressList[0];

            IPEndPoint endPoint = new IPEndPoint(ip, 8888);

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

            Console.WriteLine("shutting down server...");
            Console.Read();

        }

        public void AddFileFromHost(string file, HostData host)
        {

        }

        public void RemoveFileFromHost(string file, HostData host)
        {

        }

        public void AddHost(HostData host)
        {

        }

        public void RemoveHost(HostData host)
        {

        }


        public static void Main(string[] args)
        {
            Start();

        }
    }
}
