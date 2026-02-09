using System;
using System.IO;
using NAudio.Wave;
using NVorbis;

namespace Utils.IO;

public record PcmData(byte[] Data, int SampleRate, int Channels, int BitDepth, float DurationSeconds);

public static class AudioConverter
{
    public static PcmData ConvertToPcm(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLower();
        var file = FileLoader.LoadByteFile(filePath);
        if (file == null)
        {
            throw new ArgumentException("Audio file not found");
        }

        return ext switch
        {
            ".wav" => DecodeWav(file),
            ".ogg" => DecodeOgg(file),
            ".mp3" => DecodeMp3(file),
            _ => throw new NotSupportedException($"Unsupported format: {ext}"),
        };
    }

    private static PcmData DecodeWav(byte[] wavBytes)
    {
        using var ms = new MemoryStream(wavBytes);
        using var reader = new WaveFileReader(ms);
        var sampleRate = reader.WaveFormat.SampleRate;
        var channels = reader.WaveFormat.Channels;
        var bitDepth = reader.WaveFormat.BitsPerSample;

        var rawBytes = new byte[reader.Length];
        // ReSharper disable once MustUseReturnValue
        reader.Read(rawBytes, 0, rawBytes.Length);

        var duration = (float)reader.TotalTime.TotalSeconds;
        return new PcmData(rawBytes, sampleRate, channels, bitDepth, duration);
    }

    private static PcmData DecodeMp3(byte[] mp3Bytes)
    {
        using var ms = new MemoryStream(mp3Bytes);
        using var reader = new Mp3FileReader(ms);
        var sampleRate = reader.Mp3WaveFormat.SampleRate;
        var channels = reader.Mp3WaveFormat.Channels;
        var bitDepth = reader.Mp3WaveFormat.BitsPerSample;

        using var pcmStream = WaveFormatConversionStream.CreatePcmStream(reader);
        using var memStream = new MemoryStream();
        pcmStream.CopyTo(memStream);

        var rawBytes = memStream.ToArray();
        var duration = (float)reader.TotalTime.TotalSeconds;
        return new PcmData(rawBytes, sampleRate, channels, bitDepth, duration);
    }

    private static PcmData DecodeOgg(byte[] oggBytes)
    {
        using var ms = new MemoryStream(oggBytes);
        using var vorbis = new VorbisReader(ms, false);

        var sampleRate = vorbis.SampleRate;
        var channels = vorbis.Channels;
        const int bitDepth = 16;

        var samples = new float[vorbis.TotalSamples * channels];
        vorbis.ReadSamples(samples, 0, samples.Length);

        var rawBytes = new byte[samples.Length * 2];
        for (var i = 0; i < samples.Length; i++)
        {
            var s = Math.Min(Math.Max(samples[i], -1f), 1f);
            var val = (short)(s * short.MaxValue);
            rawBytes[i * 2] = (byte)(val & 0xFF);
            rawBytes[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
        }

        var duration = (float)vorbis.TotalTime.TotalSeconds;
        return new PcmData(rawBytes, sampleRate, channels, bitDepth, duration);
    }
}
