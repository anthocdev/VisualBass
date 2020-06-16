using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass.AddOn.Tags;

namespace Visual.cs
{
    /// <summary>
    /// Model for storing track information
    /// </summary>
    public class MetaModel
    {
        public int Freq;
        public int BitRate;
        public string File;
        public string Channels;
        public string Artist;
        public string Title;
        public string Album;
        public string Year;

        /// <summary>
        /// Dict for returning channel type by id
        /// </summary>
        Dictionary<int, string> ChannelsDict = new Dictionary<int, string>()
        {
            {0, "Null" },
            {1, "Mono" },
            {2, "Stereo" }
        };

        public MetaModel(string file)
        {
            TAG_INFO metaInfo = new TAG_INFO();
            File = file;
            metaInfo = BassTags.BASS_TAG_GetFromFile(file);
            BitRate = metaInfo.bitrate;
            Freq = metaInfo.channelinfo.freq;
            Channels = ChannelsDict[metaInfo.channelinfo.chans];
            Artist = metaInfo.artist;
            Album = metaInfo.album;

            if (metaInfo.title == "")
                Title = DataVars.GetFileName(file);
            else
                Title = metaInfo.title;
            Year = metaInfo.year;
        }

        public override string ToString()
        {
            return this.Title;
        }
    }
}
