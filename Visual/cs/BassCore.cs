using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Schema;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Tags;
using Un4seen.Bass.Misc;

namespace Visual.cs
{

    /// <summary>
    /// Storing player states, allows easy separation from automatic BASS.Net and manual user behaviour.
    /// </summary>
    public enum PlayerState
    {
        Disabled,
        Stopped,
        Paused,
        PlaylistEnd,
        Playing

    }
    public class BassCore
    {

        public int HZ = 44100;
        public PlayerState activeState; // Current state of the player (stream)
        private bool InitDefaultDevice;
        private BASS_DX8_PARAMEQ _parEq = new BASS_DX8_PARAMEQ();
        private int[] _fx = new int[] { 0, 1, 2, 3, 4};
        private bool _eqEnabled = false;

        public int Stream;

        public int Volume = 100;
        public bool PlaylistEnd;

        private float[] fftData = new float[2048];

        private readonly List<int> ExtensionHandlers = new List<int>();

        public event PropertyChangedEventHandler StaticPropertyChanged;

        //Just testing
        public delegate void PlaybackEventHandler(object sender, string file);

        public event PlaybackEventHandler PlayEvent;
        public byte[] SpectrumData = new byte[200];
        private void OnStaticPropertyChanged(string propertyName)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        public float[] FFTData
        {
            get { return fftData; }
            set { fftData = value; OnStaticPropertyChanged("FFTData"); Console.WriteLine(fftData + "kekw"); }
        }

        public BassCore()
        {
            activeState = PlayerState.Disabled; 
        }

        /// <summary>
        /// Initialization of bass library
        /// </summary>
        /// <param name="hz"></param>
        /// <returns></returns>
        public bool InitBass(int hz)
        {
            if (!InitDefaultDevice)
            {
                InitDefaultDevice = Bass.BASS_Init(-1, hz, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
                //If default device initialized
                if (InitDefaultDevice)
                {
                    //Adding WAV Support
                    ExtensionHandlers.Add(Bass.BASS_PluginLoad(DataVars.AppPath + @"extensions\basswv.dll"));

                    int errs = 0;
                    for (int i = 0; i < ExtensionHandlers.Count; i++)
                        if (ExtensionHandlers[i] == 0)
                            errs++;
                    if (errs != 0)
                        MessageBox.Show("Total of " + errs + " plugins failed to load", "Error", MessageBoxButton.OK);
                    errs = 0;
                    activeState = PlayerState.Stopped;
                }
            }
                

            return InitDefaultDevice;
        }

       
        /// <summary>
        /// Playing the audio (Check if initialized & paused)
        /// </summary>
        /// <param name="file"></param>
        /// <param name="volume"></param>
        public void Play(string file, int volume)
        {
            PlayEvent?.Invoke(null, file);
            
            if (Bass.BASS_ChannelIsActive(Stream) != BASSActive.BASS_ACTIVE_PAUSED) { 
                Stop(); //Stopping existing stream
                if (InitBass(HZ))
                {
                    Stream = Bass.BASS_StreamCreateFile(file, 0, 0, BASSFlag.BASS_DEFAULT); //Loading in audio file into stream
                    if (Stream != 0)
                    {
                        
                        Volume = volume;
                        Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume / 100F); //Initializing channel volume attribute
                        Bass.BASS_ChannelPlay(Stream, false); //Start playing the stream
                    }
                }
            }
            else
            {
                Bass.BASS_ChannelPlay(Stream, false);
            }
            EqEnable(_eqEnabled); // Enabling EQ based on _eqEnabled state
            activeState = PlayerState.Playing;
        }

        /// <summary>
        /// Chandata (Fast Fourier Transform) refactored to a specific range of values for spectrum display
        /// Peak value limited to 255
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public byte[] GetChanData(int stream)
        {
            Bass.BASS_ChannelGetData(stream, FFTData, (int)(BASSData.BASS_DATA_FFT1024 | BASSData.BASS_DATA_FFT_COMPLEX));
            //FFTData.ToList().ForEach(i => Console.Write(i.ToString()));

            //for (int a = 0; a < 1024; a++)
            //    Console.WriteLine("{0}: ({1}, {2})", a, FFTData[a * 2], FFTData[a * 2 + 1]);
            int range = 200;
            int x, y;
            int b0 = 0;
            for(x=0; x< range; x++)
            {
                double peak = 0;
                int b1 = (int)Math.Pow(2, x * 10.0 / (range - 1));

                if (b1 > 1023) b1 = 1023;
                if (b1 <= b0) b1 = b0 + 1;

                for(; b0 < b1; b0++)
                {
                    if (peak < FFTData[1 + b0]) peak = FFTData[1 + b0];
                }

                y = (int)(Math.Sqrt(peak) * 3 * 255 - 4);

                if (y > 255) y = 255;
                if (y < 0) y = 0;

                SpectrumData[x] = (byte)y;               
            }
            return SpectrumData;
        }

        /// <summary>
        /// Stopping and freeing the stream.
        /// </summary>
        public void Stop()
        {
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            activeState = PlayerState.Stopped;
        }

        /// <summary>
        /// Pausing the stream (check the state first)
        /// </summary>
        public void Pause()
        {
            if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING)
                Bass.BASS_ChannelPause(Stream); activeState = PlayerState.Paused;
        }
        /// <summary>
        /// Returning channel time
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public int GetStreamTime(int stream)
        {
            long TimeBytes = Bass.BASS_ChannelGetLength(stream);
            double Time = Bass.BASS_ChannelBytes2Seconds(stream, TimeBytes);
            return (int)Time;
        }

        /// <summary>
        /// Getting position of stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public int GetStreamPos(int stream)
        {
            long pos = Bass.BASS_ChannelGetPosition(stream);
            int posSec = (int)Bass.BASS_ChannelBytes2Seconds(stream, pos);
            return posSec;
        }
        /// <summary>
        /// Specifying position of a stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="pos"></param>
        public void SetScrollPos(int stream, double pos)
        {
            Bass.BASS_ChannelSetPosition(stream, pos);
        }
        /// <summary>
        /// Streamposition variable
        /// </summary>
        public double StreamPos
        {
            get { return GetStreamPos(Stream); }
            set { SetScrollPos(Stream, value); }
        }
        /// <summary>
        /// Setting Volume
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="volume"></param>
        public void SetStreamVolume(int stream, int volume)
        {
            Volume = volume;
            Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, Volume / 100F);
        }

        /// <summary>
        /// Switching track automatically after end, using BASS_ACTIVE_STOPPED state against activeState enum to 
        /// confirm the change is not manual
        /// </summary>
        /// <returns></returns>
        public bool NextTrack()
        {
            //If stream is stopped automatically (Playing state)
            if ((Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_STOPPED) && (activeState == PlayerState.Playing))
            {
                if (DataVars.FileList.Count > DataVars.CurrentTrack + 1)
                {
                    Play(DataVars.FileList[++DataVars.CurrentTrack], Volume);
                    return true;
                }
                else
                    activeState = PlayerState.PlaylistEnd;
            }
            return false;
        }

        /// <summary>
        /// Enables EQ based on data in _fx array
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public bool EqEnable(bool enabled)
        {
            _eqEnabled = enabled;
            if (_eqEnabled)
            {
                for (int i = 0; i < _fx.Length; i++)
                {
                    _fx[i] = Bass.BASS_ChannelSetFX(this.Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
                }
                return true;
            }
            else
            {
                for(int i = 0; i < _fx.Length; i++)
                {
                    Bass.BASS_ChannelRemoveFX(this.Stream, _fx[i]);
                }
                return false;
            }
        }

        /// <summary>
        /// Equalizer parameter
        /// </summary>
        /// <param name="fxchan">_fx Array channel</param>
        /// <param name="center"> Center band</param>
        /// <param name="gain"> Gain for the band</param>
        /// <returns></returns>
        public bool SetEqParams(int fxchan, double center, double gain)
        {
            _parEq.fBandwidth = 18.0f;
            _parEq.fCenter = (float)center;
            _parEq.fGain = (float)gain;
            return Bass.BASS_FXSetParameters(_fx[fxchan], _parEq);
        }

    }
}
