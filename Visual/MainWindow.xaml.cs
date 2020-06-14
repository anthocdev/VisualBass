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
using System.Windows.Threading;
using Un4seen.Bass;
using Visual.cs;

namespace Visual
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        
        public MainWindow()
        {
            InitializeComponent();
           
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e) {
            lblStream.Content = TimeSpan.FromSeconds(BassCore.GetStreamPos(BassCore.Stream)).ToString();
            sldStream.Value = BassCore.GetStreamPos(BassCore.Stream);
        }
        /// <summary>
        /// Opening file select dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".mp3";
            dlg.Filter = "MP3 Files (*.mp3)|*.mp3|WAV Files (*.wav)|*.wav|OGG Files (*.ogg)|*.ogg";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                DataVars.FileList.Add(dlg.FileName);
                lstPlaylist.Items.Add(DataVars.GetFileName(dlg.FileName));
            }
        }

        /// <summary>
        /// Start playback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if ((lstPlaylist.Items.Count != 0) && (lstPlaylist.SelectedIndex != -1))
            {
                string current = DataVars.FileList[lstPlaylist.SelectedIndex];
                dispatcherTimer.IsEnabled = true;
                BassCore.SetStreamVolume(BassCore.Stream, (int)sldVolume.Value);
                BassCore.Play(current, BassCore.Volume); //Using bass library to play with initial volume val
                lblStream.Content = TimeSpan.FromSeconds(BassCore.GetStreamPos(BassCore.Stream)).ToString();
                sldStream.Maximum = BassCore.GetStreamTime(BassCore.Stream);
                sldStream.Value = BassCore.GetStreamPos(BassCore.Stream);
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            BassCore.Stop();
            dispatcherTimer.IsEnabled = false;
            sldStream.Value = 0;
            lblStream.Content = "00:00:00";
        }

        private void sldStream_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            BassCore.SetScrollPos(BassCore.Stream, sldStream.Value);
        }

        private void sldVolume_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            BassCore.SetStreamVolume(BassCore.Stream, (int)sldVolume.Value);
            lblVolumeVal.Content = (int)sldVolume.Value;
        }
    }
}
