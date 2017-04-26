using System;
using System.Collections.Generic;
using System.Linq;
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
using System.ComponentModel;
using System.Net;
using System.Threading;
using MPConBot;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Speech.Synthesis;

namespace BuildingGuideGUI
{
    public partial class MainWindow : Window
    {
        System.Net.Sockets.UdpClient sock = null;
        IPEndPoint iep = null;
        Thread udp_thread;

        public MainWindow()
        {
            InitializeComponent();

            quickbuttons = new List<button_context>();
            for (int i = 0; i < 5; i++)
            {
                button_context bcon = new button_context();
                quickbuttons.Add(bcon);
            }
            floorimage(@"\University Floorplan\T_F3.jpg");

            buttonvis();
            populate_building();
            

            udp_thread = new Thread(new ThreadStart(UdpServer.UDP_main));//for darwin
            udp_thread.IsBackground = true;
            udp_thread.Start();
        }

        private void UDPspeak(string msg)//darwin
        {
            msg = "S" + msg;
            sock = new System.Net.Sockets.UdpClient();
            iep = new IPEndPoint(IPAddress.Parse("192.168.123.1"), 15007);

            byte[] data = Encoding.ASCII.GetBytes(msg);
            sock.Send(data, data.Length, iep);
            sock.Close();
        }

        private void UDPmove(string msg)//darwin
        {
            msg = "M" + msg;
            sock = new System.Net.Sockets.UdpClient();
            iep = new IPEndPoint(IPAddress.Parse("192.168.123.1"), 15007);

            byte[] data = Encoding.ASCII.GetBytes(msg);
            sock.Send(data, data.Length, iep);
            sock.Close();
        }

        private void buttonvis()
        {
            b332.Opacity = 0;
            b333.Opacity = 0;
            b334.Opacity = 0;
            b335.Opacity = 0;
            b336.Opacity = 0;
            b343.Opacity = 0;
            b343b.Opacity = 0;
            b349.Opacity = 0;
            b353.Opacity = 0;
        }

        private void floorimage(string path)
        {
            try
            { 
                map_image.Stretch = Stretch.Fill;
                map_image.Source = new BitmapImage(new Uri(@"C:\Users\"+ Environment.UserName + @"\OneDrive\Fall 2016 Projects\Darwin Projects\Autonomous-Map-Robot\BuildingGuideGUI\Asset" + path));//replace with local path to asset folder
            }
            catch (UriFormatException) { overlay_image.Source = new BitmapImage(); };
        }

        private void overlayimage(string path)
        {
            try
            {
                
                overlay_image.Stretch = Stretch.Fill;
                overlay_image.Source = new BitmapImage(new Uri(@"C:\Users\" + Environment.UserName + @"\OneDrive\Fall 2016 Projects\Darwin Projects\Autonomous-Map-Robot\BuildingGuideGUI\Asset" + path));//replace with local path to asset folder
            }
            catch (UriFormatException) { overlay_image.Source = new BitmapImage(); };
        }

        private void populate_building()
        {
            quickbuttons[0].text = "Cell Culture Lab";
            quickbuttons[0].subtitle = "The cell culture lab is is directly behind you, room 336";
            quickbuttons[0].path_overlay = @"\MapOverlay\336.png";
            quickbuttons[0].path_floor = @"\University Floorplan\T_F3.jpg";
            quickbuttons[0].move = 0;

            quickbuttons[1].text = "Cell Biology Lab";
            quickbuttons[1].subtitle = "The cell biology lab is directly behind you, room 334";
            quickbuttons[1].path_overlay = @"\MapOverlay\334.png";
            quickbuttons[1].path_floor = @"\University Floorplan\T_F3.jpg";
            quickbuttons[1].move = 0;

            quickbuttons[2].text = "Biomaterials/Histology Lab";
            quickbuttons[2].subtitle = "The biomaterials and histology lab is behind you and to the right, room 332";
            quickbuttons[2].path_overlay = @"\MapOverlay\332.png";
            quickbuttons[2].path_floor = @"\University Floorplan\T_F3.jpg";
            quickbuttons[2].move = 1;

            quickbuttons[3].text = "Microfabrication Lab";
            quickbuttons[3].subtitle = "Microfabrication lab: First turn to my right then head down the hallway past the elevator.  Enter the second door on the right, room 343.  Room 343b is the first door on the right wall of room 343";
            quickbuttons[3].path_overlay = @"\MapOverlay\343b.png";
            quickbuttons[3].path_floor = @"\University Floorplan\T_F3.jpg";
            quickbuttons[3].move = 1;

            quickbuttons[4].text = "BioMEMS Lab";
            quickbuttons[4].subtitle = "First turn to my right then proceed down the hallway past the elevator.  The BioMEMS lab is the second door on the right, room 343";
            quickbuttons[4].path_overlay = @"\MapOverlay\343.png";
            quickbuttons[4].path_floor = @"\University Floorplan\T_F3.jpg";
            quickbuttons[4].move = 1;

            dropdown_button.ContextMenu.Items.Clear();
            MenuItem m = new MenuItem();
            m.FontSize = 18;
            m.Click += function_select;
            m.Header = "J-336: Cell Culture Lab";
            dropdown_button.ContextMenu.Items.Add(m);
            m = new MenuItem();
            m.FontSize = 18;
            m.Click += function_select;
            m.Header = "J-334: Cell Biology Lab";
            dropdown_button.ContextMenu.Items.Add(m);
            m = new MenuItem();
            m.FontSize = 18;
            m.Click += function_select;
            m.Header = "J-332: Biomaterials/Histology Lab";
            dropdown_button.ContextMenu.Items.Add(m);
            m = new MenuItem();
            m.FontSize = 18;
            m.Click += function_select;
            m.Header = "J-343b: Microfabrication Lab";
            dropdown_button.ContextMenu.Items.Add(m);
            m = new MenuItem();
            m.FontSize = 18;
            m.Click += function_select;
            m.Header = "J-343: BioMEMS Lab";
            dropdown_button.ContextMenu.Items.Add(m);
            m = new MenuItem();
            m.FontSize = 18;
            m.Click += function_select;
            m.Header = "J-335: Bioinstrumentation Lab";
            dropdown_button.ContextMenu.Items.Add(m);
            m = new MenuItem();
            m.FontSize = 18;
            m.Click += function_select;
            m.Header = "J-333: Biosensors Lab";
            dropdown_button.ContextMenu.Items.Add(m);
            m = new MenuItem();
            m.FontSize = 18;
            m.Click += function_select;
            m.Header = "J-349: Marburger STEM Center";
            dropdown_button.ContextMenu.Items.Add(m);
            m = new MenuItem();
            m.FontSize = 18;
            m.Click += function_select;
            m.Header = "J-353 Biomedical Engineering Department";
            dropdown_button.ContextMenu.Items.Add(m);
            m = new MenuItem();
            m.FontSize = 18;
            m.Click += function_select;
            m.Header = "*2 Computer Science Robotics Lab";
            dropdown_button.ContextMenu.Items.Add(m);
            m = new MenuItem();
            m.FontSize = 18;
            m.Click += function_select;
            m.Header = "*2 Engineering Studios";
            dropdown_button.ContextMenu.Items.Add(m);
            m = new MenuItem();
            m.FontSize = 18;
            m.Click += function_select;
            m.Header = "*2 Robofest Office";
            dropdown_button.ContextMenu.Items.Add(m);
            m = new MenuItem();
            m.FontSize = 18;
            m.Click += function_select;
            m.Header = "*1 Experimental Biomechanics Lab";
            dropdown_button.ContextMenu.Items.Add(m);
            m = new MenuItem();
            m.FontSize = 18;
            m.Click += function_select;
            m.Header = "*1 Robotics Engineering Lab";
            dropdown_button.ContextMenu.Items.Add(m);
            m = new MenuItem();
            m.FontSize = 18;
            m.Click += function_select;
            m.Header = "*1 Embedded Software Engineering Lab";
            dropdown_button.ContextMenu.Items.Add(m);
        }

        private void populate_campus()
        {
            quickbuttons[0].text = "Student Services Center";
            quickbuttons[0].subtitle = "The student services center is the glass building to the right of this building's exit";
            quickbuttons[0].path_overlay = @"\MapOverlay\SSB.png";
            quickbuttons[0].path_floor = @"\University Floorplan\CampusMap.png";
            quickbuttons[0].move = 0;

            quickbuttons[1].text = "Buell Management Building";
            quickbuttons[1].subtitle = "The Buell management building will be on your right after exiting this building.  It is behind the glass building.";
            quickbuttons[1].path_overlay = @"\MapOverlay\Buell.png";
            quickbuttons[1].path_floor = @"\University Floorplan\CampusMap.png";
            quickbuttons[1].move = 0;

            quickbuttons[2].text = "Science Building";
            quickbuttons[2].subtitle = "The science building is to my left through the double doors";
            quickbuttons[2].path_overlay = @"\MapOverlay\Science.png";
            quickbuttons[2].path_floor = @"\University Floorplan\CampusMap.png";
            quickbuttons[2].move = 2;

            quickbuttons[3].text = "Technology and Learning Center";
            quickbuttons[3].subtitle = "The Technology and Learning Center is directly across the quad from this building.  You can enter the building under the arch.";
            quickbuttons[3].path_overlay = @"\MapOverlay\Tech.png";
            quickbuttons[3].path_floor = @"\University Floorplan\CampusMap.png";
            quickbuttons[3].move = 0;

            quickbuttons[4].text = "Engineering Building";
            quickbuttons[4].subtitle = "The Engineering building is the first building to the left of this building's exit.  Also, there is a bridge to the engineering on this building's second floor.";
            quickbuttons[4].path_overlay = @"\MapOverlay\Eng.png";
            quickbuttons[4].path_floor = @"\University Floorplan\CampusMap.png";
            quickbuttons[4].move = 1;

            dropdown_button.ContextMenu.Items.Clear();
            MenuItem m = new MenuItem();
            m.FontSize = 18;
            m.Click += function_select;
            m.Header = "Buell Management Building";
            dropdown_button.ContextMenu.Items.Add(m);

            m = new MenuItem();
            m.FontSize = 18;
            m.Click += function_select;
            m.Header = "A. Alfred Taubman Student Services Center";
            dropdown_button.ContextMenu.Items.Add(m);

            m = new MenuItem();
            m.FontSize = 18;
            m.Click += function_select;
            m.Header = "Engineering Building";
            dropdown_button.ContextMenu.Items.Add(m);

            m = new MenuItem();
            m.FontSize = 18;
            m.Click += function_select;
            m.Header = "Science Building";
            dropdown_button.ContextMenu.Items.Add(m);

            m = new MenuItem();
            m.FontSize = 18;
            m.Click += function_select;
            m.Header = "Architecture Building";
            dropdown_button.ContextMenu.Items.Add(m);

            m = new MenuItem();
            m.FontSize = 18;
            m.Click += function_select;
            m.Header = "Technology and Learning Center";
            dropdown_button.ContextMenu.Items.Add(m);

            m = new MenuItem();
            m.FontSize = 18;
            m.Click += function_select;
            m.Header = "Taubman Complex";
            dropdown_button.ContextMenu.Items.Add(m);
        }

        private List<button_context> quickbuttons;

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button) == button_0)
            {
                button_push(quickbuttons[0]);
            }
            else if ((sender as Button) == button_1)
            {
                button_push(quickbuttons[1]);
            }
            else if ((sender as Button) == button_2)
            {
                button_push(quickbuttons[2]);
            }
            else if ((sender as Button) == button_3)
            {
                button_push(quickbuttons[3]);
            }
            else if ((sender as Button) == button_4)
            {
                button_push(quickbuttons[4]);
            }
        }

        private void update_qbuttons()
        {
            button_0.Content = quickbuttons[0].text;
            button_1.Content = quickbuttons[1].text;
            button_2.Content = quickbuttons[2].text;
            button_3.Content = quickbuttons[3].text;
            button_4.Content = quickbuttons[4].text;
        }

        private void button_push(button_context bcon)
        {
            Tblock_bot.Text = bcon.subtitle;
            floorimage(bcon.path_floor);
            overlayimage(bcon.path_overlay);

            //UDPmove(bcon.move.ToString());
            //UDPspeak(Tblock_bot.Text);
            //UDPmove("0");
            UdpServer.move(bcon.move.ToString());
            speak(Tblock_bot.Text);
            UdpServer.move("0");
        }

        //Task task;
        public void speak(string line)
        {
            Dispatcher.Invoke(new Action(() => { }), System.Windows.Threading.DispatcherPriority.ContextIdle, null);

            //if (task != null)
            //    task.Wait();
            SpeechSynthesizer synth = new SpeechSynthesizer();
            //task = Task.Run(() => synth.Speak(line));
            synth.Speak(line);
            UdpServer.idle_count = 0;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            update_qbuttons();
        }

        private void dropdown_click(object sender, RoutedEventArgs e)
        {
            (sender as Button).ContextMenu.IsEnabled = true;
            (sender as Button).ContextMenu.PlacementTarget = (sender as Button);
            (sender as Button).ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            (sender as Button).ContextMenu.IsOpen = true;
        }

        private void function_select(object sender, RoutedEventArgs e)
        {
            MenuItem m = sender as MenuItem;

            switch (m.Header.ToString())
            {
                case "J-336: Cell Culture Lab":
                    overlayimage(@"\MapOverlay\336.png");
                    Tblock_bot.Text = "The cell culture lab is is directly behind you, room 336";
                    UDPmove("3");
                    UdpServer.move("3");
                    break;
                case "J-334: Cell Biology Lab":
                    overlayimage(@"\MapOverlay\334.png");
                    Tblock_bot.Text = "The cell biology lab is directly behind you, room 334";
                    UDPmove("3");
                    UdpServer.move("3");
                    break;
                case "J-332: Biomaterials/Histology Lab":
                    overlayimage(@"\MapOverlay\332.png");
                    Tblock_bot.Text = "The biomaterials and histology lab is behind you and to the right, room 332";
                    UDPmove("1");
                    UdpServer.move("1");
                    break;
                case "J-343b: Microfabrication Lab":
                    overlayimage(@"\MapOverlay\343b.png");
                    Tblock_bot.Text = "Microfabrication lab: First turn to my right then head down the hallway past the elevator.  Enter the second door on the right, room 343.  Room 343b is the first door on the right wall of room 343";
                    UDPmove("1");
                    UdpServer.move("1");
                    break;
                case "J-343: BioMEMS Lab":
                    overlayimage(@"\MapOverlay\343.png");
                    Tblock_bot.Text = "First turn to my right then proceed down the hallway past the elevator.  The BioMEMS lab is the second door on the right, room 343";
                    UDPmove("1");
                    UdpServer.move("1");
                    break;
                case "J-335: Bioinstrumentation Lab":
                    overlayimage(@"\MapOverlay\335.png");
                    Tblock_bot.Text = "First turn to my right then proceed down the hallway past the elevator.  The Bioinstrumentation lab is the sixth door on the right, room 335";
                    UDPmove("1");
                    UdpServer.move("1");
                    break;
                case "J-333: Biosensors Lab":
                    overlayimage(@"\MapOverlay\333.png");
                    Tblock_bot.Text = "First turn to my right then proceed down the hallway past the elevator.  The Biosensors lab is the seventh door on the right, room 335";
                    UDPmove("1");
                    UdpServer.move("1");
                    break;
                case "J-349: Marburger STEM Center":
                    overlayimage(@"\MapOverlay\349.png");
                    Tblock_bot.Text = "The Marburger STEM Center is the third door to my right";
                    UDPmove("1");
                    UdpServer.move("1");
                    break;
                case "J-353 Biomedical Engineering Department":
                    overlayimage(@"\MapOverlay\353.png");
                    Tblock_bot.Text = "The biomedical engineering department is the first door on my right, room 353";
                    UDPmove("1");
                    UdpServer.move("1");
                    break;
                case "*2 Computer Science Robotics Lab":
                    overlayimage(@"\MapOverlay\Stairs.png");
                    Tblock_bot.Text = "The Computer Science Robotics lab is on the second floor.  The elevator is to my right.  The stairs are directly across from the elevator.";
                    UDPmove("1");
                    UdpServer.move("1");
                    break;
                case "*2 Engineering Studios":
                    overlayimage(@"\MapOverlay\Stairs.png");
                    Tblock_bot.Text = "The Engineering Studios are on the second floor.  The elevator is to my right.  The stairs are directly across from the elevator.";
                    UDPmove("1");
                    UdpServer.move("1");
                    break;
                case "*2 Robofest Office":
                    overlayimage(@"\MapOverlay\Stairs.png");
                    Tblock_bot.Text = "The Robofest Office is on the second floor.  The elevator is to my right.  The stairs are directly across from the elevator.";
                    UDPmove("1");
                    UdpServer.move("1");
                    break;
                case "*1 Experimental Biomechanics Lab":
                    overlayimage(@"\MapOverlay\Stairs.png");
                    Tblock_bot.Text = "The Experimental Biomechanics lab is on the first floor.  The elevator is to my right.  The stairs are directly across from the elevator.";
                    UDPmove("1");
                    UdpServer.move("1");
                    break;
                case "*1 Robotics Engineering Lab":
                    overlayimage(@"\MapOverlay\Stairs.png");
                    Tblock_bot.Text = "The Robotics Engineering lab is on the first floor.  The elevator is to my right.  The stairs are directly across from the elevator.";
                    UDPmove("1");
                    UdpServer.move("1");
                    break;
                case "*1 Embedded Software Engineering Lab":
                    overlayimage(@"\MapOverlay\Stairs.png");
                    Tblock_bot.Text = "The Embedded Software Engineering lab is on the first floor.  The elevator is to my right.  The stairs are directly across from the elevator.";
                    UDPmove("1");
                    UdpServer.move("1");
                    break;
                case "Buell Management Building":
                    overlayimage(@"\MapOverlay\Buell.png");
                    Tblock_bot.Text = "The Buell management building will be on your right after exiting this building.  It is behind the glass building.";
                    UDPmove("0");
                    UdpServer.move("0");
                    break;
                case "Architecture Building":
                    overlayimage(@"\MapOverlay\Arch.png");
                    Tblock_bot.Text = "The architecture building is diagnally across the quad from this building.  It is a short red-brick building.";
                    UDPmove("0");
                    UdpServer.move("0");
                    break;
                case "A. Alfred Taubman Student Services Center":
                    overlayimage(@"\MapOverlay\SSB.png");
                    Tblock_bot.Text = "The student services center is the glass building to the right of this building's exit";
                    UDPmove("0");
                    UdpServer.move("0");
                    break;
                case "Engineering Building":
                    overlayimage(@"\MapOverlay\Eng.png");
                    Tblock_bot.Text = "The Engineering building is the first building to the left of this building's exit.  Also, there is a bridge to the engineering on this building's second floor.";
                    UDPmove("0");
                    UdpServer.move("0");
                    break;
                case "Science Building":
                    overlayimage(@"\MapOverlay\Science.png");
                    Tblock_bot.Text = "The science building is to my left through the double doors";
                    UDPmove("2");
                    UdpServer.move("0");
                    break;
                case "Technology and Learning Center":
                    overlayimage(@"\MapOverlay\Tech.png");
                    Tblock_bot.Text = "The Technology and Learning Center is directly across the quad from this building.  You can enter the building under the arch.";
                    UDPmove("0");
                    UdpServer.move("0");
                    break;
                case "Taubman Complex":
                    overlayimage(@"\MapOverlay\TComp.png");
                    Tblock_bot.Text = "We are currently in the Taubman Complex";
                    UDPmove("0");
                    UdpServer.move("0");
                    break;
            }

            UDPspeak(Tblock_bot.Text);
            speak(Tblock_bot.Text);
            UDPmove("0");
            UdpServer.move("0");
        }

        private void FloorDown_Click(object sender, RoutedEventArgs e)
        {
            floorimage(@"\University Floorplan\Floor1.jpg");
            //overlayimage("");
            overlay_image.Source = new BitmapImage();
        }

        private void FloorUp_Click(object sender, RoutedEventArgs e)
        {
            floorimage(@"\University Floorplan\Floor2.jpg");
            //overlayimage
            overlay_image.Source = new BitmapImage();
        }

        private void overlay_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;

            switch (b.Content.ToString())
            {
                case "336":
                    overlayimage(@"\MapOverlay\336.png");
                    Tblock_bot.Text = "The cell culture lab is is directly behind you, room 336";
                    UDPmove("3");
                    UdpServer.move("3");
                    break;
                case "334":
                    overlayimage(@"\MapOverlay\334.png");
                    Tblock_bot.Text = "The cell biology lab is directly behind you, room 334";
                    UDPmove("3");
                    UdpServer.move("3");
                    break;
                case "332":
                    overlayimage(@"\MapOverlay\332.png");
                    Tblock_bot.Text = "The biomaterials and histology lab is behind you and to the right, room 332";
                    UDPmove("1");
                    UdpServer.move("1");
                    break;
                case "343b":
                    overlayimage(@"\MapOverlay\343b.png");
                    Tblock_bot.Text = "Microfabrication lab: First turn to my right then head down the hallway past the elevator.  Enter the second door on the right, room 343.  Room 343b is the first door on the right wall of room 343";
                    UDPmove("1");
                    UdpServer.move("1");
                    break;
                case "343":
                    overlayimage(@"\MapOverlay\343.png");
                    Tblock_bot.Text = "First turn to my right then proceed down the hallway past the elevator.  The BioMEMS lab is the second door on the right, room 343";
                    UDPmove("1");
                    UdpServer.move("1");
                    break;
                case "335":
                    overlayimage(@"\MapOverlay\335.png");
                    Tblock_bot.Text = "First turn to my right then proceed down the hallway past the elevator.  The Bioinstrumentation lab is the sixth door on the right, room 335";
                    UDPmove("1");
                    UdpServer.move("1");
                    break;
                case "333":
                    overlayimage(@"\MapOverlay\333.png");
                    Tblock_bot.Text = "First turn to my right then proceed down the hallway past the elevator.  The Biosensors lab is the seventh door on the right, room 335";
                    UDPmove("1");
                    UdpServer.move("1");
                    break;
                case "349":
                    overlayimage(@"\MapOverlay\349.png");
                    Tblock_bot.Text = "The Marburger STEM Center is the third door to my right";
                    UDPmove("1");
                    UdpServer.move("1");
                    break;
                case "353":
                    overlayimage(@"\MapOverlay\353.png");
                    Tblock_bot.Text = "The biomedical engineering department is the first door on my right, room 353";
                    UDPmove("1");
                    UdpServer.move("1");
                    break;
            }
            UDPspeak(Tblock_bot.Text);
            speak(Tblock_bot.Text);
            UDPmove("0");
            UdpServer.move("0");
        }

        private void campus_button_Click(object sender, RoutedEventArgs e)
        {
            if (campus_button.Content.ToString() == "Campus Map")
            {
                campus_button.Content = "Taubman Complex \n  Third Floor Map";
                l_location.Content = "Campus";
                campusmap();
            }
            else
            {
                campus_button.Content = "Campus Map";

                l_location.Content = "Taubman Complex 3F";
                buildingmap();
            }
        }

        private void campusmap()
        {
            floorimage(@"\University Floorplan\CampusMap.png");
            populate_campus();
            update_qbuttons();
            disable_buttons();
            overlay_image.Source = new BitmapImage();
        }

        private void buildingmap()
        {
            floorimage(@"\University Floorplan\T_F3.jpg");
            populate_building();
            update_qbuttons();
            enable_buttons();
            overlay_image.Source = new BitmapImage();
        }

        private void disable_buttons()
        {
            foreach (Control c in Button_overlay.Children)
            {
                Button b = c as Button;
                if (b != null)
                {
                    b.IsEnabled = false;
                }
            }
        }

        private void enable_buttons()
        {
            foreach (Control c in Button_overlay.Children)
            {
                Button b = c as Button;
                if (b != null)
                {
                    b.IsEnabled = true;
                }
            }
        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            (sender as Button).Width = 1;
            (sender as Button).Height = 1;
        }
    }

    class button_context
    {
        public string text = "";
        public string path_floor = "";
        public string path_overlay = "";
        public string speak = "";
        public string subtitle = "";
        public int move;
    }
}
