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

        private static string folderToStoreMessages = "C:\\Temp\\HL7XMLMessages\\";
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
                Console.WriteLine("[LOG] Client disconnected.");
                context.Client.Close();
                context.Stream.Dispose();
                context.Message.Dispose();
                context = null;
            }
        }

        // conversion to xml and html
        static void ConvertMessage(MemoryStream message)
        {
            Console.WriteLine("Antes de enviar el mensaje: " + Encoding.ASCII.GetString(message.ToArray()));
            string xmlContent = HL7Converter.ConvertToXml(Encoding.ASCII.GetString(message.ToArray()));
            Console.WriteLine("XML Message: ");
            Console.WriteLine(xmlContent);
            SendMessage(xmlContent);
            /*string xmlContent = HL7Converter.ConvertToXml(Encoding.ASCII.GetString(message.ToArray()));
            string dateTimeFile = DateTime.Now.ToString("yyyyMMddhhmmss");
            if (!Directory.Exists(folderToStoreMessages))
            {
                Directory.CreateDirectory(folderToStoreMessages);
            }

            String xmlFile = folderToStoreMessages + "IncomingMessage" + dateTimeFile + ".xml";
            // This text is added only once to the file.


            if (File.Exists(xmlFile))
            {
                Console.WriteLine("[LOG] File already exists. Overwritting previous file");
            }

            // Create a file to write to.
            File.WriteAllText(xmlFile, xmlContent);


            string htmlContent = HL7Converter.ConvertToHtml(xmlContent);

            String htmlFile = folderToStoreMessages + "IncomingMessage" + dateTimeFile + ".html";
            // This text is added only once to the file.

            if (File.Exists(htmlFile))
            {
                Console.WriteLine("[LOG] File already exists. Overwritting previous file");
            }

            // Create a file to write to.
            File.WriteAllText(htmlFile, htmlContent);

            Console.WriteLine("[LOG] Message stored under: " + xmlFile);
            Console.WriteLine("[LOG] Message stored under: " + htmlFile);

            if (openInBrowser)
            {
                // open created html in the default browser
                Console.WriteLine("[LOG] Automatically open html file in browser");
                System.Diagnostics.Process.Start(htmlFile);
            }*/
        }

        // client connection accepted callback
        static void OnClientAccepted(IAsyncResult ar)
        {
            TcpListener listener = ar.AsyncState as TcpListener;
            if (listener == null)
                return;

            try
            {
                Console.WriteLine("[LOG] Client connected");
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
            Console.WriteLine("Sending message: ");
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

                    Console.WriteLine("Socket connected to {0}",
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
                    Console.WriteLine("Releasing socket");
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                    Console.WriteLine("END OF TRANSMISION");

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

        //private static void SendHeartBeat(object state)
        //{
        //    Console.WriteLine("Sending HeartBeat");
        //    SendMessage("Ping");
        //}


        //private static void StartHeartBeatMonitor()
        //{
        //    Timer timerMonitor = new Timer(new TimerCallback(SendHeartBeat),null,0,10000);
        //}

        // main funcion
        static void Main(string[] args)
        {
            int portNumber = defaultPortNumber;
            try
            {
                Console.Write("Please Specify the port on which the server should start up: ");
                string portToStartup = Console.ReadLine();
                portNumber = Convert.ToInt32(portToStartup);
            }
            catch (System.Exception)
            {
                Console.WriteLine("[LOG] Portnumber entered not a valid number, using default port 19000 now...");
            }

            Console.Write("Please specify the directory to store the converted (XML-)Messages (or hit enter for the default): ");
            string folderEntered = Console.ReadLine();
            if (!String.IsNullOrEmpty(folderEntered))
            {
                folderToStoreMessages = folderEntered;
            }
            else
            {
                Console.WriteLine("[LOG] Using default folder: " + folderToStoreMessages);
            }

            Console.Write("Include all values of HL7 messages in HTML display(Y/N): ");
            string displayAllInHTML = Console.ReadLine();

            if (displayAllInHTML.ToLower().Equals("y") || displayAllInHTML.ToLower().Equals("yes"))
            {
                HL7Converter.showAllInHtml = true;
            }
            else if (displayAllInHTML.ToLower().Equals("n") || displayAllInHTML.ToLower().Equals("no"))
            {
                HL7Converter.showAllInHtml = false;
            }
            else
            {
                HL7Converter.showAllInHtml = false;
            }

            Console.Write("Open retrieved result directly in browser (Y/N): ");
            string displayDirectly = Console.ReadLine();

            if (displayDirectly.ToLower().Equals("y") || displayDirectly.ToLower().Equals("yes"))
            {
                openInBrowser = true;
            }
            else if (displayDirectly.ToLower().Equals("n") || displayDirectly.ToLower().Equals("no"))
            {
                openInBrowser = false;
            }
            else
            {
                openInBrowser = false;
            }

            Console.WriteLine("[LOG] Open directly in browser on retrieval: " + openInBrowser);

            TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Any, portNumber));
            listener.Start();

            listener.BeginAcceptTcpClient(OnClientAccepted, listener);
            //StartHeartBeatMonitor();
            Console.WriteLine("[LOG] Press enter to exit...");
            Console.ReadLine();
            Console.WriteLine("[LOG] Shutting down.....");
            listener.Stop();
        }
    }
}
