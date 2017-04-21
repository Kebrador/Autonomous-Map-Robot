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
using System.Speech.Synthesis;

namespace DirectControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        LoCoMoCo MyBot = new LoCoMoCo("COM4");

        private void B_Reverse_Click(object sender, RoutedEventArgs e)
        {
            MyBot.backward();
        }

        private void B_Right_Click(object sender, RoutedEventArgs e)
        {
            MyBot.turnright();
        }

        private void B_Stop_Click(object sender, RoutedEventArgs e)
        {
            MyBot.stop();
        }

        private void B_Left_Click(object sender, RoutedEventArgs e)
        {
            MyBot.turnleft();
        }

        private void B_Forward_Click(object sender, RoutedEventArgs e)
        {
            MyBot.forward();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            SpeechSynthesizer synth = new SpeechSynthesizer();
            List<string> names = new List<string>();
            foreach (InstalledVoice v in synth.GetInstalledVoices())
            {
                names.Add(v.VoiceInfo.Name.ToString());
            }
            synth.SelectVoice(names[1]);
            synth.Speak(textBox.Text.ToString());
        }
    }
}
