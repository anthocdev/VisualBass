using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;
using Un4seen.Bass.Misc;

namespace Visual.cs
{
    public static class BassCore
    {

        private static int HZ = 44100;

        private static bool InitDefaultDevice;

        public static int Stream;

        public static int Volume = 100;

        private static float[] fftData;
        public static byte[] SpectrumData;

        private static int SampleCountValue = 180;
        public static int SampleCount
        {
            get => SampleCountValue;
            set
            {
                
                SampleCountValue = value;
                SpectrumData = new byte[value];
            }
        }// number of spectrum lines

        public static event PropertyChangedEventHandler StaticPropertyChanged;

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
        private static bool InitBass(int hz)
        {
            fftData = new float[1024];
            if (!InitDefaultDevice)
                InitDefaultDevice = Bass.BASS_Init(-1, hz, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            return InitDefaultDevice;
        }

       
        /// <summary>
        /// Playing the audio
        /// </summary>
        /// <param name="file"></param>
        /// <param name="volume"></param>
        public static void Play(string file, int volume)
        {
            Stop(); //Stopping existing stream
            if (InitBass(HZ))
            {

                Stream = Bass.BASS_StreamCreateFile(file, 0, 0, BASSFlag.BASS_DEFAULT); //Loading in audio file into stream
                if (Stream != 0)
                {
                    
                    Volume = volume;
                    Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume / 100); //Initializing channel volume attribute
                    Bass.BASS_ChannelPlay(Stream, false); //Start playing the stream
                }
            }
        }

        /// <summary>
        /// Stopping and freeing the stream.
        /// </summary>
        public static void Stop()
        {
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
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
            Bass.BASS_ChannelGetData(Stream, FFTData, (int)BASSData.BASS_DATA_FFT2048);
        }
    }
}
