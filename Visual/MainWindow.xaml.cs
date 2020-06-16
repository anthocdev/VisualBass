using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
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
using OxyPlot;
using Visual.cs;

namespace Visual
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private List<MetaModel> MMList = new List<MetaModel>(); //List of current tracks
       
        public string OxyTitle { get; private set; }
        public ObservableCollection<DataPoint> Points { get; private set; }
        public MainWindow()
        {
            initOxy();
            InitializeComponent();      
            DataVars.Core = this;
            this.DataContext = this;
            BassCore.InitBass(BassCore.HZ);
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            CompositionTarget.Rendering += graphTick; // Attach to comptarget renderer for smoother result and no mouse lag
            /* Execute when song is played (auto & manual)*/
            BassCore.PlayEvent += (s, file) =>
            {
                initDetails(); //Song details of current playing track
            };
        }

        private void initOxy()
        {
            this.OxyTitle = "Visualizer v0.0001";
            this.Points = new ObservableCollection<DataPoint>();
        }


        private void dispatcherTimer_Tick(object sender, EventArgs e) {
            lblStream.Content = TimeSpan.FromSeconds(BassCore.GetStreamPos(BassCore.Stream)).ToString();
            sldStream.Value = BassCore.GetStreamPos(BassCore.Stream);
            BassCore.GetChanData(BassCore.Stream);  //Currently testing - Constant collection of FFT data          

            //Check if track should be switched (not efficient/ to be improved)
            if (BassCore.NextTrack())
            {
                lstPlaylist.SelectedIndex = DataVars.CurrentTrack;
                lblStream.Content = TimeSpan.FromSeconds(BassCore.GetStreamPos(BassCore.Stream)).ToString();
                sldStream.Maximum = BassCore.GetStreamTime(BassCore.Stream);
                sldStream.Value = BassCore.GetStreamPos(BassCore.Stream);
            }

            if (BassCore.PlaylistEnd)
            {
                btnStop_Click(this, new RoutedEventArgs());
                lstPlaylist.SelectedIndex = DataVars.CurrentTrack = 0; //Setting playlist index and initializing currenttrack value
                BassCore.PlaylistEnd = false;
            }


        }

        
        

        private void graphTick(object sender, EventArgs e)
        {
            byte[] chanData = BassCore.GetChanData(BassCore.Stream);
            Points.Clear();
            for (int i = 0; i < chanData.Length; i++)
            {
                Points.Add(new DataPoint(i, chanData[i]));
            }
        }
        /// <summary>
        /// Opening file select dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = true;
            dlg.DefaultExt = ".mp3";
            dlg.Filter = "MP3 Files (*.mp3)|*.mp3|WAV Files (*.wav)|*.wav|OGG Files (*.ogg)|*.ogg";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string[] temp = dlg.FileNames;
                for (int i = 0; i < temp.Length; i++)
                {
                    DataVars.FileList.Add(temp[i]);
                    MetaModel MM = new MetaModel(temp[i]);
                    MMList.Add(MM);
                    lstPlaylist.Items.Add(MM.Artist + " | " + MM.Title);
                }
            }
        }

        private void initDetails()
        {
            lstDetails.Items.Clear();
            MetaModel PlayingItem = MMList[lstPlaylist.SelectedIndex];
            lstDetails.Items.Add("Title: " + PlayingItem.Title);
            lstDetails.Items.Add("Artist: " + PlayingItem.Artist);
            lstDetails.Items.Add("Album: " + PlayingItem.Album);
            lstDetails.Items.Add("Year: " + PlayingItem.Year);
            lstDetails.Items.Add("BitRate: " + PlayingItem.BitRate);
            lstDetails.Items.Add("Channel: " + PlayingItem.Channels);
            lstDetails.Items.Add("Freq: " + PlayingItem.Freq);
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
                DataVars.CurrentTrack = lstPlaylist.SelectedIndex;
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

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            BassCore.Pause();
        }

    }

}
