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
    public static class BassCore
    {

        public static int HZ = 44100;

        private static bool InitDefaultDevice;

        public static int Stream;

        public static int Volume = 100;

        private static bool isPlaying = false;
        public static bool PlaylistEnd;

        private static float[] fftData = new float[2048];

        private static readonly List<int> ExtensionHandlers = new List<int>();

        public static event PropertyChangedEventHandler StaticPropertyChanged;

        //Just testing
        public delegate void PlaybackEventHandler(object sender, string file);

        public static event PlaybackEventHandler PlayEvent;
        public static byte[] SpectrumData = new byte[200];
        private static void OnStaticPropertyChanged(string propertyName)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        public static float[] FFTData
        {
            get { return fftData; }
            set { fftData = value; OnStaticPropertyChanged("FFTData"); Console.WriteLine(fftData + "kekw"); }
        }



        /// <summary>
        /// Initialization of bass library
        /// </summary>
        /// <param name="hz"></param>
        /// <returns></returns>
        public static bool InitBass(int hz)
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
                }
            }
                

            return InitDefaultDevice;
        }

       
        /// <summary>
        /// Playing the audio (Check if initialized & paused)
        /// </summary>
        /// <param name="file"></param>
        /// <param name="volume"></param>
        public static void Play(string file, int volume)
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
            
            isPlaying = true;
        }

        public static byte[] GetChanData(int stream)
        {
            Bass.BASS_ChannelGetData(stream, FFTData, (int)(BASSData.BASS_DATA_FFT1024 | BASSData.BASS_DATA_FFT_COMPLEX));
            //FFTData.ToList().ForEach(i => Console.Write(i.ToString()));

            //for (int a = 0; a < 1024; a++)
            //    Console.WriteLine("{0}: ({1}, {2})", a, FFTData[a * 2], FFTData[a * 2 + 1]);

            int x, y;
            int b0 = 0;
            for(x=0; x< 200; x++)
            {
                double peak = 0;
                int b1 = (int)Math.Pow(2, x * 10.0 / (200 - 1));

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
        public static void Stop()
        {
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            isPlaying = false;
        }

        /// <summary>
        /// Pausing the stream (check the state first)
        /// </summary>
        public static void Pause()
        {
            if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING)
                Bass.BASS_ChannelPause(Stream);
        }
        /// <summary>
        /// Returning channel time
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static int GetStreamTime(int stream)
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
        public static int GetStreamPos(int stream)
        {
            long pos = Bass.BASS_ChannelGetPosition(stream);
            int posSec = (int)Bass.BASS_ChannelBytes2Seconds(stream, pos);
            return posSec;
        }

        public static void SetScrollPos(int stream, double pos)
        {
            Bass.BASS_ChannelSetPosition(stream, pos);
        }

        public static double StreamPos
        {
            get { return GetStreamPos(Stream); }
            set { SetScrollPos(Stream, value); }
        }
        /// <summary>
        /// Setting Volume
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="volume"></param>
        public static void SetStreamVolume(int stream, int volume)
        {
            Volume = volume;
            Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, Volume / 100F);
        }

        public static bool NextTrack()
        {
            //If stream is stopped automatically (isPlay = true)
            if ((Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_STOPPED) && (isPlaying))
            {
                if(DataVars.FileList.Count > DataVars.CurrentTrack + 1)
                {
                    Play(DataVars.FileList[++DataVars.CurrentTrack], Volume);
                    PlaylistEnd = false;
                    return true;
                }
                else
                    PlaylistEnd = true;
            }
            return false;

        }
    }
}
