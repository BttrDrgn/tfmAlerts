using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;

namespace tfmAlert
{
    public class Audio
    {
        public static Dictionary<string, string> AudioCache = new Dictionary<string, string>();

        public static void Cache(string key, string path)
        {
            if (AudioCache.ContainsKey(key)) return;
            AudioCache.Add(key, path);
        }

        public static void Play(string key)
        {
            if (!AudioCache.ContainsKey(key)) return;

            IWavePlayer player = new WaveOutEvent();
            var reader = new Mp3FileReader(AudioCache[key]);
            player.Init(reader);
            player.Play();
        }
    }
}
