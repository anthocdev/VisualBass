using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using OxyPlot;
using Visual.cs;
using Visual.cs.Components;
using Visual.cs.Models;

namespace Visual
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        BassCore BassInstance = new BassCore();
        public PlaylistModel MMList { get; set; }
        private MetaModel _playingItem;


        public string OxyTitle { get; private set; }
        public ObservableCollection<DataPoint> Points { get; private set; }
        public MainWindow()
        {
            this.MMList = new PlaylistModel();
            initOxy();
            InitializeComponent();      
            DataVars.Core = this;
            this.DataContext = this;
            BassInstance.InitBass(BassInstance.HZ);
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            CompositionTarget.Rendering += graphTick; // Attach to comptarget renderer for smoother result and no mouse lag
            /* Execute when song is played (auto & manual)*/
            BassInstance.PlayEvent += (s, file) =>
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
            lblStream.Content = TimeSpan.FromSeconds(BassInstance.GetStreamPos(BassInstance.Stream)).ToString();
            if (!sldStream.IsMouseOver) sldStream.Value = BassInstance.GetStreamPos(BassInstance.Stream);
                
            BassInstance.GetChanData(BassInstance.Stream);  //Currently testing - Constant collection of FFT data          

            //Check if track should be switched (not efficient/ to be improved)
            if (BassInstance.NextTrack())
            {
                lstPlaylist.SelectedIndex = DataVars.CurrentTrack;
                lblStream.Content = TimeSpan.FromSeconds(BassInstance.GetStreamPos(BassInstance.Stream)).ToString();
                sldStream.Maximum = BassInstance.GetStreamTime(BassInstance.Stream);
                sldStream.Value = BassInstance.GetStreamPos(BassInstance.Stream);
            }

            if (BassInstance.activeState == PlayerState.PlaylistEnd)
            {
                btnStop_Click(this, new RoutedEventArgs());
                lstPlaylist.SelectedIndex = DataVars.CurrentTrack = 0; //Setting playlist index and initializing currenttrack value
            }


        }

        
        

        private void graphTick(object sender, EventArgs e)
        {
            byte[] chanData = BassInstance.GetChanData(BassInstance.Stream);
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
                }
            }
        }

        private void initDetails()
        {
            lstDetails.Items.Clear();
            _playingItem = MMList[lstPlaylist.SelectedIndex];
            lstDetails.Items.Add("Title: " + _playingItem.Title);
            lstDetails.Items.Add("Artist: " + _playingItem.Artist);
            lstDetails.Items.Add("Album: " + _playingItem.Album);
            lstDetails.Items.Add("Year: " + _playingItem.Year);
            lstDetails.Items.Add("BitRate: " + _playingItem.BitRate);
            lstDetails.Items.Add("Channel: " + _playingItem.Channels);
            lstDetails.Items.Add("Freq: " + _playingItem.Freq);
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
                DataVars.CurrentTrack = lstPlaylist.SelectedIndex;
                dispatcherTimer.IsEnabled = true;
                BassInstance.SetStreamVolume(BassInstance.Stream, (int)sldVolume.Value);
                BassInstance.Play(MMList[lstPlaylist.SelectedIndex].File, BassInstance.Volume); //Using bass library to play with initial volume val
                lblStream.Content = TimeSpan.FromSeconds(BassInstance.GetStreamPos(BassInstance.Stream)).ToString();
                sldStream.Maximum = BassInstance.GetStreamTime(BassInstance.Stream);
                sldStream.Value = BassInstance.GetStreamPos(BassInstance.Stream);
                UpdateEqParams(sender, null);
            }

        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            BassInstance.Stop();
            dispatcherTimer.IsEnabled = false;
            sldStream.Value = 0;
            lblStream.Content = "00:00:00";
        }

        private void sldStream_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            BassInstance.SetScrollPos(BassInstance.Stream, sldStream.Value);
        }


        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            BassInstance.Pause();
        }

        private void MenuItem_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (lstPlaylist.SelectedIndex == -1) return; //Don't do anything

            if (_playingItem == MMList[lstPlaylist.SelectedIndex]) BassInstance.Stop();
            MMList.RemoveAt(lstPlaylist.SelectedIndex);
        }

        private void sldVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(lblVolumeVal != null)
            {
                BassInstance.SetStreamVolume(BassInstance.Stream, (int)e.NewValue);
                lblVolumeVal.Content = (int)e.NewValue + "%";
            }

        }

        private void chkEqEnabled_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (chkEqEnabled.IsChecked == true)
            {
                this.BassInstance.EqEnable(true);
                UpdateEqParams(sender, null);
            }
            else { this.BassInstance.EqEnable(false); }
            
            Console.WriteLine("Checked?" + chkEqEnabled.IsChecked.ToString());
        }

        private void sldEq1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.BassInstance.SetEqParams(0, 100, sldEq1.Value);
        }

        private void sldEq2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.BassInstance.SetEqParams(1, 500, sldEq2.Value);
        }

        private void sldEq3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.BassInstance.SetEqParams(2, 1000, sldEq3.Value);
        }

        private void sldEq4_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.BassInstance.SetEqParams(3, 4000, sldEq4.Value);
        }

        private void sldEq5_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.BassInstance.SetEqParams(4, 8000, sldEq5.Value);
        }

        private void UpdateEqParams(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sldEq1_ValueChanged(sender, e);
            sldEq2_ValueChanged(sender, e);
            sldEq3_ValueChanged(sender, e);
            sldEq4_ValueChanged(sender, e);
            sldEq5_ValueChanged(sender, e);
        }

        /// <summary>
        /// Application exit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miExit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void miAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog aboutDialog = new AboutDialog();
            aboutDialog.Owner = this;
            aboutDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            aboutDialog.ShowDialog();
        }
    }

}
