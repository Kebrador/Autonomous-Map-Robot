/*This application was Designed, Implemented and Tested by Khalil Khalaf Lawrence Technological Univeristy Spring 2016
This is the Server part of the Multi Purpose Connected Robot Senrior Project for the Masters of Computer Science in Intelligent Systems
For any additional Information or questions: KhalafKhalil@gmail.com*/

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading;
using System.Threading.Tasks;
using L2Bot_Controller;

namespace MPConBot
{
    class UdpServer
    {
        static public void UDP_main()
        {
            
            LoCoMoCo MyBot = new LoCoMoCo("COM4"); // com port number
            var MainToken = new CancellationTokenSource(); //create token for the cancel

            UdpClient MainServerSocket = new UdpClient(15000); // declare a client
            byte[] MainDataReceived = new byte[1024]; // prepare container for received data
            string MainStringData = "";


            Capture cap1 = new Capture(0); // declare object for camera
            Capture cap2 = new Capture(1);
            Image<Bgr, Byte> frame1; // declare image for capture
            Image<Bgr, Byte> frame2; // declare image for capture
            int TotalMessageCount = 0;

            while (true) // this while for keeping the main server "listening"
            {
                try {

                    frame1 = cap1.QueryFrame();
                    frame2 = cap2.QueryFrame();
                    Console.WriteLine("Waiting for a UDP client..."); // display stuff
                    IPEndPoint MainClient = new IPEndPoint(IPAddress.Any,0); // prepare a client
                    MainDataReceived = MainServerSocket.Receive(ref MainClient); // receive packet
                    MainStringData = Encoding.ASCII.GetString(MainDataReceived, 0, MainDataReceived.Length); // get string from packet
                    Console.WriteLine("Response from " + MainClient.Address); // display stuff
                    Console.WriteLine("Message " + TotalMessageCount++ + ": " + MainStringData + "\n"); // display client's string
                    
                    if (MainDataReceived.Equals("Picture1"))
                    {
                        SendPicture(15001, frame1);
                    }

                    if (MainStringData.Equals("Forward"))
                        MyBot.forward();
                    else if (MainStringData.Equals("Backward"))
                        MyBot.backward();
                    else if (MainStringData.Equals("Left"))
                        MyBot.turnleft();
                    else if (MainStringData.Equals("Right"))
                        MyBot.turnright();
                    else if (MainStringData.Equals("Stop"))
                        MyBot.stop();

                } catch (Exception e)
                { }
            }
        }

        static public void SendPicture(int port, Image<Bgr, Byte> frame)
        {
            try
            {
                UdpClient MainSocket = new UdpClient(port);
                IPEndPoint MainClient = new IPEndPoint(IPAddress.Any, 0);
                var MemorySt = new MemoryStream();
                frame = frame.Resize(0.25, Emgu.CV.CvEnum.INTER.CV_INTER_NN);
                frame.Bitmap.Save(MemorySt, System.Drawing.Imaging.ImageFormat.Jpeg); // take a frame and save it in the memory stream
                byte[] dataToSend;
                dataToSend = MemorySt.ToArray(); // convert memory to byte 
                MainSocket.Send(dataToSend, dataToSend.Length, MainClient); // Send a packet
                MainSocket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't Capture Frame.\n");
            }
        }

        static public void Camera1()
        {

        }

        static public void Camera2()
        {

        }
    }
}






