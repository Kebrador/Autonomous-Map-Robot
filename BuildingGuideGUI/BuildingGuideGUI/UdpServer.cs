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
using System.Collections.Generic;
using System.Windows.Threading;

namespace MPConBot
{
    class UdpServer
    {
        static UdpClient MainServerSocket;
        static IPEndPoint MainClient;
        static LoCoMoCo MyBot;
        static Thread t1;
        static Thread t2;
        static List<UdpClient> sockets;
        static Capture[] captures;
        static Thread idle;
        static System.Timers.Timer tim;

        public static int idle_count = 0;

        static bool autonomous = true;
        static bool isidle = false; 

        static public void UDP_main()
        {
            MyBot = new LoCoMoCo("COM4"); // com port number
            var MainToken = new CancellationTokenSource(); //create token for the cancel

            MainServerSocket = new UdpClient(15000); // declare a client
            byte[] MainDataReceived = new byte[1024]; // prepare container for received data
            string MainStringData = "";
            sockets = new List<UdpClient>();
            captures = new Capture[] { new Capture(0), new Capture(1) };
            restart_cameras();

            if (tim == null)
            {
                tim = new System.Timers.Timer();
                tim.Elapsed += tick;
                tim.Interval = 1000;
                tim.Start();
            }

            while (true) // this while for keeping the main server "listening"
            {
                try
                {
                    MainClient = new IPEndPoint(IPAddress.Any,15000); // prepare a client
                    MainDataReceived = MainServerSocket.Receive(ref MainClient); // receive packet
                    MainStringData = Encoding.ASCII.GetString(MainDataReceived, 0, MainDataReceived.Length); // get string from packet

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
                    else if (MainStringData.Equals("Autonomous"))
                        autonomous_mode_start();
                    else if (MainStringData.Equals("Manual"))
                        manual_mode_start();
                    else if (MainStringData.Equals("Restart"))
                        restart_cameras();
                }
                catch (Exception) { }
            }
        }

        static void tick(Object sender, EventArgs e)
        {
            if (!autonomous)
                return;

            idle_count++;

            if (idle_count < 60)
            {
                if (idle != null)
                    idle.Abort();
                idle = null;
                isidle = false;
            }
            else
                isidle = true;

            if (isidle == true && idle == null)
            {
                idle = new Thread(idle_thread);
                idle.IsBackground = true;
                idle.Start();
            }
        }

        static void idle_thread()
        {
            Random rand = new Random();
            int rcount = 0;
            int probmult = 10;

            while (true)
            {
                Thread.Sleep(1000);

                if (isidle && autonomous)
                {
                    rcount++;
                    if (rand.Next(0,1001) > 1000 - rcount * probmult)
                    {
                        move_random(rand);
                    }
                }
                else
                {
                    rcount = 0;
                }

            }

        }

        static void move_random(Random rand)
        {
            if (rand.Next(0,2) == 1)
            {
                move("1");
                Thread.Sleep(5000);
                move("0");
            }
            else
            {
                move("2");
                Thread.Sleep(5000);
                move("0");
            }
        }

        static void restart_cameras()
        {
            if (t1 != null && t1.IsAlive)
                t1.Abort();
            if (t2 != null && t2.IsAlive)
                t2.Abort();

            foreach (UdpClient sock in sockets)
                sock.Close();

            Thread.Sleep(100);

            t1 = new Thread(Camera1);
            t2 = new Thread(Camera2);
            t1.IsBackground = true;
            t2.IsBackground = true;
            t1.Start();
            t2.Start();
        }

        static int lastdir = 0;
        static public void move(string dir)
        {
            if (!autonomous)
                return;

            int rad = 400;

            switch (dir)
            {
                case "0"://return
                    {
                        if (lastdir == 1)
                            MyBot.turnleft();
                        else if (lastdir == 2)
                            MyBot.turnright();

                        lastdir = 0;
                        Thread.Sleep(rad);
                        MyBot.stop();
                        realign();
                    }
                    break;
                case "1"://right
                    lastdir = 1;
                    MyBot.turnright();
                    Thread.Sleep(rad);
                    MyBot.stop();
                    break;
                case "2"://left
                    lastdir = 2;
                    MyBot.turnleft();
                    Thread.Sleep(rad);
                    MyBot.stop();
                    break;
            }
        }

        static void realign()
        {
            if (!autonomous)
                return;
        }

        static void autonomous_mode_start()
        {
            MyBot.stop();
            byte[] data = Encoding.ASCII.GetBytes("Confirm");
            MainServerSocket.Send(data, data.Length, MainClient);
            autonomous = true;
            idle_count = 0;
        }

        static void manual_mode_start()
        {
            MyBot.stop();
            byte[] data = Encoding.ASCII.GetBytes("Confirm");
            MainServerSocket.Send(data, data.Length, MainClient);
            autonomous = false;
            idle_count = 0;
        }

        static void Camera1()
        {
            camthread(0);
        }

        static void Camera2()
        {
            camthread(1);
        }

        static void camthread(int camera_number)
        {
            Capture cap = captures[camera_number];
            
            Image<Bgr, Byte> image;

            UdpClient MainServerSocket = new UdpClient(15001 + camera_number); // declare a client
            sockets.Add(MainServerSocket);
            string MainStringData = "";

            while (true)
            {
                try
                {
                    IPEndPoint MainClient = new IPEndPoint(IPAddress.Any, 15001 + camera_number); // prepare a client
                    byte[] MainDataReceived = new byte[1024]; // prepare container for received data
                    MainDataReceived = MainServerSocket.Receive(ref MainClient); // receive packet
                    MainStringData = Encoding.ASCII.GetString(MainDataReceived, 0, MainDataReceived.Length); // get string from packet

                    if (MainStringData.Equals("Picture"))
                    {
                        image = cap.QueryFrame();
                        //image = image.Resize(0.25, Emgu.CV.CvEnum.INTER.CV_INTER_NN);
                        var MemorySt = new MemoryStream();
                        image.Bitmap.Save(MemorySt, System.Drawing.Imaging.ImageFormat.Jpeg); // take a frame and save it in the memory stream
                        byte[] dataToSend;
                        dataToSend = MemorySt.ToArray();
                        MainServerSocket.Send(dataToSend, dataToSend.Length, MainClient); // Send a packet
                    }
                }
                catch(Exception) { }
            }
        }
    }
}






