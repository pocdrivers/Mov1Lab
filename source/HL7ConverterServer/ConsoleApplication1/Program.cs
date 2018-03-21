using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace HL7ToXMLServer
{
    class ClientContext
    {
        public TcpClient Client;
        public Stream Stream;
        public byte[] Buffer = new byte[4];
        public MemoryStream Message = new MemoryStream();
    }

    class Program
    {
        private static int defaultPortNumber = 19000;
        private static bool openInBrowser = false;

        // message completely retrieved callback
        static void OnMessageReceived(ClientContext context)
        {
            // only process it, if there is content received.
            if (context.Message.Length > 0)
            {
                ConvertMessage(context.Message);
            }
        }

        // retrieve data from tcp stream
        static void OnClientRead(IAsyncResult ar)
        {
            ClientContext context = ar.AsyncState as ClientContext;
            bool clientClosed = false;
            if (context == null)
                return;

            // Detect if client disconnected
            if( context.Client.Client.Poll( 0, SelectMode.SelectRead ) )
            {
              byte[] buff = new byte[1];
              if( context.Client.Client.Receive( buff, SocketFlags.Peek ) == 0 )
              {
                // Client disconnected
                clientClosed = true;
              }
            }

            if (!clientClosed)
            {
                try
                {
                    int read = context.Stream.EndRead(ar);
                    context.Message.Write(context.Buffer, 0, read);

                    int length = BitConverter.ToInt32(context.Buffer, 0);
                    byte[] buffer = new byte[1024];
                    read = context.Stream.Read(buffer, 0, Math.Min(buffer.Length, length));
                    context.Message.Write(buffer, 0, read);
                    length -= read;

                    OnMessageReceived(context);
                }
                catch (System.Exception)
                {
                    context.Client.Close();
                    context.Stream.Dispose();
                    context.Message.Dispose();
                    context = null;
                }
                finally
                {
                    if (context != null)
                        context.Stream.BeginRead(context.Buffer, 0, context.Buffer.Length, OnClientRead, context);
                }
            }
            else
            {
                Console.WriteLine("[LOG] Client disconnected.\n");
                context.Client.Close();
                context.Stream.Dispose();
                context.Message.Dispose();
                context = null;
            }
        }

        // conversion to xml and html
        static void ConvertMessage(MemoryStream message)
        {
            string xmlContent = HL7Converter.ConvertToTemplate(Encoding.ASCII.GetString(message.ToArray()));
            SendMessage(xmlContent);
           
        }

        // client connection accepted callback
        static void OnClientAccepted(IAsyncResult ar)
        {
            TcpListener listener = ar.AsyncState as TcpListener;
            if (listener == null)
                return;

            try
            {
                Console.WriteLine("[LOG] Client connected\n");
                ClientContext context = new ClientContext();
                context.Client = listener.EndAcceptTcpClient(ar);
                context.Stream = context.Client.GetStream();
                context.Stream.BeginRead(context.Buffer, 0, context.Buffer.Length, OnClientRead, context);
            }
            catch (SystemException ex)
            {
                return;
            }
            finally
            {
                listener.BeginAcceptTcpClient(OnClientAccepted, listener);
            }
        }

        static void SendMessage(string msg)
        {
            Console.WriteLine("Sending message: \n");
            Console.WriteLine(msg);
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                // This example uses port 11000 on the local computer.  
                //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

                IPAddress ipAddress = IPAddress.Parse(ConfigurationManager.AppSettings["MiddleManIp"]);
                int portNumber = int.Parse(ConfigurationManager.AppSettings["MiddleManPort"]);
                IPEndPoint remoteEp = new IPEndPoint(ipAddress, portNumber);

                // Create a TCP/IP  socket.  
                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    sender.Connect(remoteEp);

                    Console.WriteLine("Socket connected to {0}\n",
                        sender.RemoteEndPoint.ToString());

                    // Encode the data string into a byte array.  
                    byte[] msgToSend = Encoding.ASCII.GetBytes(msg);

                    // Send the data through the socket.  
                    int bytesSent = sender.Send(msgToSend);

                    // Receive the response from the remote device.  
                    /*int bytesRec = sender.Receive(bytes);
                    Console.WriteLine("Echoed test = {0}",
                        Encoding.ASCII.GetString(bytes, 0, bytesRec));*/

                    // Release the socket.  
                    Console.WriteLine("Releasing socket\n");
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                    Console.WriteLine("END OF TRANSMISION\n");

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
        }

        private static void StartHeartBeatMonitor()
        {
            Timer timerMonitor = new Timer(new TimerCallback(SendHeartBeat), null, 0, 10000);
        }

        private static void SendHeartBeat(object state)
        {
            Console.WriteLine("Sending HeartBeat\n");
            SendMessage("Ping");
        }

        // main funcion
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            string title = @"
 |  | |  __  /__|                     |           
 __ | |     /(     _ \   \\ \ / -_)  _|_|  -_)  _|
_| _|____|_/\___|\___/_| _|\_/\___|_|\__|\___|_|  ";

            Console.Title = "HL7Converter";
            Console.WriteLine(title+"\n");

            int portNumber = defaultPortNumber;
            try
            {
                Console.Write("Port on which the server should start up: 19000\n");

            }
            catch (System.Exception)
            {
                Console.WriteLine("[LOG] Portnumber entered not a valid number, using default port 19000 now...\n");
            }

            TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Any, portNumber));
            listener.Start();

            listener.BeginAcceptTcpClient(OnClientAccepted, listener);
            StartHeartBeatMonitor();
            //Console.WriteLine("[LOG] Press enter to exit...");
            Console.ReadLine();
            //Console.WriteLine("[LOG] Shutting down.....");
            listener.Stop();
        }
    }
}
