using UnityEngine;
using System.IO;
using System;
using NAudio.Wave;

public static class NAudioPlayer
{
    public static AudioClip FromMp3Data(byte[] data)
    {
        var mp3Stream = new MemoryStream(data);
        var mp3Audio = new Mp3FileReader(mp3Stream);
        var waveStream = WaveFormatConversionStream.CreatePcmStream(mp3Audio);
        var wav = new WAV(AudioMemStream(waveStream).ToArray());

        AudioClip audioClip;
        if (wav.ChannelCount == 2)
        {
            audioClip = AudioClip.Create("Audio File Name", wav.SampleCount, 2, wav.Frequency, false);
            audioClip.SetData(wav.StereoChannel, 0);
        }
        else
        {
            audioClip = AudioClip.Create("Audio File Name", wav.SampleCount, 1, wav.Frequency, false);
            audioClip.SetData(wav.LeftChannel, 0);
        }

        return audioClip;
    }

    private static MemoryStream AudioMemStream(WaveStream waveStream)
    {
        var outputStream = new MemoryStream();
        using (var waveFileWriter = new WaveFileWriter(outputStream, waveStream.WaveFormat))
        {
            var bytes = new byte[waveStream.Length];
            waveStream.Position = 0;
            waveStream.Read(bytes, 0, Convert.ToInt32(waveStream.Length));
            waveFileWriter.Write(bytes, 0, bytes.Length);
            waveFileWriter.Flush();
        }
        return outputStream;
    }
}

public class WAV
{
    private static float BytesToFloat(byte firstByte, byte secondByte)
    {
        var s = (short)((secondByte << 8) | firstByte);
        return s / 32768.0F;
    }

    private static int BytesToInt(byte[] bytes, int offset = 0)
    {
        var value = 0;
        for (var i = 0; i < 4; i++)
        {
            value |= (bytes[offset + i]) << (i * 8);
        }
        return value;
    }

    public float[] LeftChannel { get; internal set; }
    public float[] RightChannel { get; internal set; }
    public float[] StereoChannel { get; internal set; }
    public int ChannelCount { get; internal set; }
    public int SampleCount { get; internal set; }
    public int Frequency { get; internal set; }

    public WAV(byte[] wav)
    {
        ChannelCount = wav[22];                     // Forget byte 23 as 99.999% of WAVs are 1 or 2 channels
        Frequency = BytesToInt(wav, 24);
        var pos = 12;                               // First Subchunk ID from 12 to 16

        while (!(wav[pos] == 100 && wav[pos + 1] == 97 && wav[pos + 2] == 116 && wav[pos + 3] == 97))
        {
            pos += 4;
            var chunkSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
            pos += 4 + chunkSize;
        }
        pos += 8;

        SampleCount = (wav.Length - pos) / 2;       // 2 bytes per sample (16 bit sound mono)
        if (ChannelCount == 2) SampleCount /= 2;    // 4 bytes per sample (16 bit stereo)

        LeftChannel = new float[SampleCount];
        RightChannel = ChannelCount == 2 ? new float[SampleCount] : null;

        var i = 0;
        while (pos < wav.Length)
        {
            LeftChannel[i] = BytesToFloat(wav[pos], wav[pos + 1]);
            pos += 2;
            if (ChannelCount == 2)
            {
                if (RightChannel != null) RightChannel[i] = BytesToFloat(wav[pos], wav[pos + 1]);
                pos += 2;
            }
            i++;
        }

        if (ChannelCount == 2)
        {
            StereoChannel = new float[SampleCount * 2];
            var channelPos = 0;
            short posChange = 0;

            for (var index = 0; index < (SampleCount * 2); index++)
            {

                if (index % 2 == 0)
                {
                    StereoChannel[index] = LeftChannel[channelPos];
                    posChange++;
                }
                else
                {
                    if (RightChannel != null) StereoChannel[index] = RightChannel[channelPos];
                    posChange++;
                }
                if (posChange % 2 == 0)
                {
                    if (channelPos < SampleCount)
                    {
                        channelPos++;
                        posChange = 0;
                    }
                }
            }
        }
        else
        {
            StereoChannel = null;
        }
    }

    public override string ToString()
    {
        return string.Format("[WAV: LeftChannel={0}, RightChannel={1}, ChannelCount={2}, SampleCount={3}, Frequency={4}]", LeftChannel, RightChannel, ChannelCount, SampleCount, Frequency);
    }
}