using System.IO;
using NAudio.Wave;

namespace BOYAREngine.Audio
{
    public class Mp3
    {
        private static WaveOutEvent _outputDevice;
        private static Mp3FileReader _audioFileReader;
        private static MemoryStream _ms;

        public static void Play(byte[] bytes, float volume = 1f)
        {
            if (_outputDevice == null) _outputDevice = new WaveOutEvent();

            _audioFileReader = null;
            
            _outputDevice.PlaybackStopped += OutputDevice_PlaybackStopped;
            _outputDevice.Volume = volume;

            _ms = new MemoryStream(bytes);

            _audioFileReader = new Mp3FileReader(_ms);

            _outputDevice.Init(_audioFileReader);
            _outputDevice.Play();
        }

        public static void Stop()
        {
            _outputDevice.Stop();
        }

        private static void OutputDevice_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            _outputDevice.Dispose();
            _outputDevice = null;
            _audioFileReader.Dispose();
            _audioFileReader = null;
            _ms.Dispose();
        }
    }
}

