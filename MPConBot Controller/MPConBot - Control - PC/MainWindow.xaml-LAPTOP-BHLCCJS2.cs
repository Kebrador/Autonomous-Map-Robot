using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Emgu.CV.VideoSurveillance;
using Emgu.CV.Structure;
using Emgu.CV;
using Emgu.Util;
using System.Net.Sockets;
using System.Diagnostics;
using System.Windows.Threading;

namespace MPConBot___Control___PC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string ip = "192.168.1.73";
        UdpClient sock_dir = null;
        IPEndPoint iep_dir = null;

        int failcount1 = 0;
        int failcount2 = 0;
        int framecount1 = 0;
        int framecount2 = 0;

        //System.Net.Sockets.UdpClient sock_send = null;
        //IPEndPoint iep_vid = null;

        public MainWindow()
        {
            InitializeComponent();
            sock_dir = new UdpClient(15000);
            iep_dir = new IPEndPoint(IPAddress.Parse(ip), 15000);
        }

        private void B_Forward_Click(object sender, RoutedEventArgs e)
        {
            control_l2bot("Forward");
        }

        private void B_Left_Click(object sender, RoutedEventArgs e)
        {
            control_l2bot("Left");
        }

        private void B_Stop_Click(object sender, RoutedEventArgs e)
        {
            control_l2bot("Stop");
        }

        private void B_Right_Click(object sender, RoutedEventArgs e)
        {
            control_l2bot("Right");
        }

        private void B_Reverse_Click(object sender, RoutedEventArgs e)
        {
            control_l2bot("Backward");
        }

        private void control_l2bot(string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            sock_dir.Send(data, data.Length, iep_dir);
        }

        private void vid_thread1()
        {
            while (true)
            {
                var task1 = Task.Run(() => vid_rec(15001, main_image));
                if (!task1.Wait(TimeSpan.FromMilliseconds(250)))
                {
                    failcount1++;
                }
                framecount1++;
            }
        }

        private void vid_thread2()
        {
            while (true)
            {
                var task2 = Task.Run(() => vid_rec(15002, lower_image));
                if (!task2.Wait(TimeSpan.FromMilliseconds(250)))
                {
                    failcount2++;
                }
                framecount2++;
            }
        }

        private void vid_rec(int port, Image GUI_img)
        {
            BitmapImage image = new BitmapImage();
            try
            {
                UdpClient sock = new UdpClient(port);
                IPEndPoint iep = new IPEndPoint(IPAddress.Parse(ip), port);
                byte[] data = Encoding.ASCII.GetBytes("Picture");
                sock.Send(data, data.Length, iep);
                
                byte[] buffer = new byte[65535];
                buffer = sock.Receive(ref iep);

                sock.Close();

                using (MemoryStream mem = new MemoryStream(buffer))
                {
                    mem.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = mem;
                    image.EndInit();
                }
                image.Freeze();

                //send image to gui
                this.Dispatcher.Invoke((Action)(() =>
                {
                    GUI_img.Source = image;
                }));
            }
            catch { return; };
        }

        Thread workingThread;
        Thread workingThread2;
        private void b_startvid_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;

            workingThread = new Thread(new ThreadStart(vid_thread1))  { IsBackground = true };
            workingThread.Start();
            workingThread2 = new Thread(new ThreadStart(vid_thread2)) { IsBackground = true };
            workingThread2.Start();

            DispatcherTimer tim = new DispatcherTimer();
            tim.Tick += tick;
            tim.Interval = new TimeSpan(0, 0, 1);
            tim.Start();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.W)
                control_l2bot("Forward");
            else if (e.Key == Key.Down || e.Key == Key.S)
                control_l2bot("Backward");
            else if (e.Key == Key.Left || e.Key == Key.A)
                control_l2bot("Left");
            else if (e.Key == Key.Right || e.Key == Key.D)
                control_l2bot("Right");
            else
                control_l2bot("Stop");
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            control_l2bot("Stop");
        }

        private void tick(Object sender, EventArgs e)
        {


            this.Dispatcher.Invoke((Action)(() =>
            {
                b_startvid.Content = (double)(framecount1 + framecount2)/2;
            }));

            framecount1 = 0;
            framecount2 = 0;
        }
    }
}
