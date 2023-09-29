using NAudio.Wave;

namespace VideoManagerAPI.Services
{
    public class MediaService
    {
        public static string ConvertAudio(string audio, string formattedAudio)
        {
            using (var reader = new AudioFileReader(audio))
            {
                var resampler = new MediaFoundationResampler(reader, new WaveFormat(16000, reader.WaveFormat.Channels));
                WaveFileWriter.CreateWaveFile(formattedAudio, resampler);
            }
            return formattedAudio;
        }

        public static string ConvertVideoToAudio(string video, string audio)
        {
            using (var reader = new MediaFoundationReader(video))
            {
                MediaFoundationEncoder.EncodeToWma(reader, audio);
            }
            return audio;
        }
    }
}
