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
    public partial class MainWindow : Window
    {
        string ip = "192.168.1.73";
        UdpClient sock_dir = null;
        IPEndPoint iep_dir = null;
        UdpClient[] fail_sock;

        int failcount1 = 0;
        int failcount2 = 0;
        int framecount1 = 0;
        int framecount2 = 0;

        bool m_override = false;

        //System.Net.Sockets.UdpClient sock_send = null;
        //IPEndPoint iep_vid = null;

        public MainWindow()
        {
            InitializeComponent();
            sock_dir = new UdpClient(15000);
            iep_dir = new IPEndPoint(IPAddress.Parse(ip), 15000);
            fail_sock = new UdpClient[2];
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
                var task1 = Task.Run(() => vid_rec(0, main_image));
                if (!task1.Wait(TimeSpan.FromMilliseconds(250)))
                {
                    failcount1++;
                    if (fail_sock[0] != null)
                        fail_sock[0].Close();
                }
                framecount1++;
            }
        }

        private void vid_thread2()
        {
            while (true)
            {
                var task2 = Task.Run(() => vid_rec(1, lower_image));
                if (!task2.Wait(TimeSpan.FromMilliseconds(250)))
                {
                    failcount2++;
                    if (fail_sock[1] != null)
                        fail_sock[1].Close();
                }
                framecount2++;
            }
        }

        private void vid_rec(int cam_num, Image GUI_img)
        {
            BitmapImage image = new BitmapImage();
            try
            {
                UdpClient sock = new UdpClient(cam_num + 15001);
                fail_sock[cam_num] = sock;
                IPEndPoint iep = new IPEndPoint(IPAddress.Parse(ip), cam_num + 15001);
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

        Thread camreq1;
        Thread camreq2;
        DispatcherTimer tim;
        private void b_startvid_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Content.ToString() == "Re-Sync")
            {
                control_l2bot("Restart");
                Thread.Sleep(200);
            }

            (sender as Button).Content = "Re-Sync";
            if (camreq1 != null && camreq1.IsAlive)
            {
                camreq1.Abort();
                if (fail_sock[0] != null)
                    fail_sock[0].Close();
            }
            if (camreq2 != null && camreq2.IsAlive)
            {
                camreq2.Abort();
                if (fail_sock[1] != null)
                    fail_sock[1].Close();
            }

            camreq1 = new Thread(new ThreadStart(vid_thread1))  { IsBackground = true };
            camreq1.Start();
            camreq2 = new Thread(new ThreadStart(vid_thread2)) { IsBackground = true };
            camreq2.Start();

            if (tim == null)
            {
                tim = new DispatcherTimer();
                tim.Tick += tick;
                tim.Interval = new TimeSpan(0, 0, 1);
                tim.Start();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (m_override)
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
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            control_l2bot("Stop");
        }

        private void tick(Object sender, EventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                label_FPS.Content = "FPS: " + (double)(framecount1 + framecount2 - failcount1 - failcount2)/2;
            }));

            framecount1 = 0;
            framecount2 = 0;
            failcount1 = 0;
            failcount2 = 0;
        }

        private void b_focusswitch_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button) == null || (sender as Button).Content == null)
                return;
            else if ((sender as Button).Content.ToString() == ">")//switch right
            {
                (sender as Button).Content = "<";
                GridLength glL = new GridLength(2, GridUnitType.Star);
                GridLength glR = new GridLength(1, GridUnitType.Star);
                LeftCol.Width = glL;
                RightCol.Width = glR;
            }
            else if ((sender as Button).Content.ToString() == "<")//switch left
            {
                (sender as Button).Content = ">";
                GridLength glL = new GridLength(1, GridUnitType.Star);
                GridLength glR = new GridLength(2, GridUnitType.Star);
                LeftCol.Width = glL;
                RightCol.Width = glR;
            }
        }

        private void b_mode_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Content.ToString() == "Autonomous")
                autonomous_mode();
            else if ((sender as Button).Content.ToString() == "Manual")
                manual_mode();
        }

        private void manual_mode()
        {
            if (mode_change("Manual"))
            {
                m_override = true;
                b_mode.Content = "Autonomous";
                B_Forward.IsEnabled = true;
                B_Reverse.IsEnabled = true;
                B_Left.IsEnabled = true;
                B_Right.IsEnabled = true;
            }
            else
                MessageBox.Show("Communication timeout");
        }

        private void autonomous_mode()
        {
            if (mode_change("Autonomous"))
            {
                m_override = false;
                b_mode.Content = "Manual";
                B_Forward.IsEnabled = false;
                B_Reverse.IsEnabled = false;
                B_Left.IsEnabled = false;
                B_Right.IsEnabled = false;
            }
            else
                MessageBox.Show("Communication timeout");
        }

        private bool mode_change(string mode)
        {
            int count = 0;
            bool confirm = false;
            while (!confirm)
            {
                var task1 = Task.Run(() => confirm = mode_message(mode));
                if (!task1.Wait(TimeSpan.FromMilliseconds(50)))
                {
                    confirm = false;
                }
                count++;

                if (count > 10)
                {
                    return false;
                }
            }
            return true;
        }

        private bool mode_message(string mode)
        {
            byte[] data = Encoding.ASCII.GetBytes(mode);
            sock_dir.Send(data, data.Length, iep_dir);

            byte[] buffer = new byte[256];
            buffer = sock_dir.Receive(ref iep_dir);
            string ret = Encoding.ASCII.GetString(buffer, 0, buffer.Count());

            if (ret == "Confirm")
                return true;
            else
                return false;
        }
    }
}
