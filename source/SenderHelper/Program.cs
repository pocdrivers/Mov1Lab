using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SenderHelper
{
    class Program
    {
        private static Socket socketSender;
        //private const string middleManIp = "192.168.126.1";
        //private static string msg = "MSH|2018-02-28|PAT020|CRP-AF|AF100 50596|50596||||1802282CF9E1B1855|P|2.3|||NE|SUPID|1||PAT020||lastname20^patient20^||19900706|F||||||||||PV1|1||SOFIAHIS||||||||||||||||ORC|NW|^HIS|||CM||||20180228140600OBR||^HIS||||||||||||||||||||20180228010101|||FOBX|1|ST|PATID|1|PAT020||||||FOBX|2|ST|VISIT|1|||||||FOBX|3|ST|LASTNAME|1|lastname20||||||FOBX|4|ST|FIRSTNAME|1|patient20||||||FOBX|5|ST|SEX|1|F||||||FOBX|6|ST|DATEOFBIRTH|1|19900706||||||FOBX|7|ST|ANALYZERNAME|1|AF100 50596||||||FOBX|8|ST|ANALYZEDATETIME|1|20180228010101||||||FOBX|9|ST|OPID|1|HUGO||||||FOBX|10|ST|CRP-AF|50596|59.0|mg/L|-||||F|||20180228010101OBX|11|01:01:01|NEWTEST|CRP-AF|AF100 50596||||||50596OBX|12|ST|HbA1cAF|50596|6.6|%|-||||F|||20180228010101OBX|13|01:01:01|NEWTEST|HbA1cAF|AF100 50596||||||50596";
        private const string middleManIp = "127.0.0.1";
        //private static string[] msg = { "MSH|14:55|28-02-2018|PAT020|CRP-AF|AF100 50596|50596|||", "MSH|12:24|08-08-2018|PAT008|CRP-AF|AF100 50596|50596|||", "MSH|17:13|19-09-2016|PAT020|GAS|B101|12345|||" };
        private static string[] msg = { "MSH|28-02-2018|14:55|PAT853|CRP-AF|AF100 50596|50596|||", "MSH|01-01-1991|12:24|PAT001|RAD-ABL90|AF100 50596|1111|||", "MSH|19-09-2016|17:13|PAT02|GAS|B101|12345|||" };
        private static Timer timer1;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            InitTimer();
            while (true)
            {
                try
                {
                    Console.WriteLine("Enviando mensaje");
                    //SendMessage(msg);
                    //Thread.Sleep(3000);
                    foreach (string sms in msg)
                    {
                        SendMessage(sms);
                        Thread.Sleep(3000);
                    }
                    Thread.Sleep(30000);
                }
                catch (Exception e)
                {

                    Console.WriteLine(e);
                    Console.ReadLine();
                }
            }
        }

        private static void InitTimer()
        {
            timer1 = new Timer(new TimerCallback(SendHeartBeat),null,0, 10000);
        }

        private static void SendHeartBeat(object state)
        {
            Console.WriteLine("Sending ping");

            SendMessage("Ping");
        }


        private static void SendMessage(string msg)
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                // This example uses port 11000 on the local computer.  
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = IPAddress.Parse(middleManIp);//ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 9000);

                // Create a TCP/IP  socket.  
                socketSender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    socketSender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}",
                        socketSender.RemoteEndPoint.ToString());

                    // Encode the data string into a byte array.  
                    byte[] msgToSend = Encoding.ASCII.GetBytes(msg);

                    // Send the data through the socket.  
                    int bytesSent = socketSender.Send(msgToSend);
                    Console.WriteLine("local end point {0}",
                        socketSender.LocalEndPoint.ToString());
                    // Receive the response from the remote device.  
                    //int bytesRec = socketSender.Receive(bytes);
                    //Console.WriteLine("Echoed test = {0}",
                    //    Encoding.ASCII.GetString(bytes, 0, bytesRec));

                    // Release the socket.  
                    //Console.WriteLine("BEFORE SHUTDOWN");
                    socketSender.Shutdown(SocketShutdown.Both);
                    //Console.WriteLine("AFTER SHUTDOWN");
                    socketSender.Close();
                    //Console.WriteLine("END OF TRANSMISION");

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
                Console.ReadLine();
            }
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            // Release the socket.  
            //Console.WriteLine("BEFORE SHUTDOWN");
            socketSender.Shutdown(SocketShutdown.Both);
            //Console.WriteLine("AFTER SHUTDOWN");
            socketSender.Close();
            //Console.WriteLine("END OF TRANSMISION");
        }

        private static void SendMessageHttp()
        {
            try
            {
                // Create a request using a URL that can receive a post.   
                WebRequest request = WebRequest.Create("http://localhost:11000/messages/showMessages");
                // Set the Method property of the request to POST.  
                request.Method = "POST";
                // Create POST data and convert it to a byte array.  
                string postData = "This is a test that posts this string to a Web server.";
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                // Set the ContentType property of the WebRequest.  
                request.ContentType = "application/x-www-form-urlencoded";
                // Set the ContentLength property of the WebRequest.  
                request.ContentLength = byteArray.Length;
                // Get the request stream.  
                Stream dataStream = request.GetRequestStream();
                // Write the data to the request stream.  
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Close the Stream object.  
                dataStream.Close();
                // Get the response.  
                WebResponse response = request.GetResponse();
                // Display the status.  
                Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                // Get the stream containing content returned by the server.  
                dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.  
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.  
                string responseFromServer = reader.ReadToEnd();
                // Display the content.  
                Console.WriteLine(responseFromServer);
                // Clean up the streams.  
                reader.Close();
                dataStream.Close();
                response.Close();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                Console.ReadLine();
            }
        }
    }
}
