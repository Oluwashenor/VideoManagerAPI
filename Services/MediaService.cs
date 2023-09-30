using NAudio.Wave;

namespace VideoManagerAPI.Services
{
    public class MediaService
    {
        public static async Task<string> ConvertMp3ToWave(string mp3, string wav)
        {
            using (var reader = new AudioFileReader(mp3))
            {
                using var resampler = new MediaFoundationResampler(reader, new WaveFormat(16000, reader.WaveFormat.Channels));
                await Task.Run(() =>
                {
                    WaveFileWriter.CreateWaveFile(wav, resampler);
                });
            }
            return wav;
        }

        public static string ConvertVideoToAudio(string video, string audio)
        {
            using (var reader = new MediaFoundationReader(video))
            {
                MediaFoundationEncoder.EncodeToWma(reader, audio);
            }
            return audio;
        }

        public static async Task<string> ConvertFormVideoToAudio(IFormFile video)
        {
            string tempVideoFilePath = Path.Combine("", Path.GetTempFileName());
            string fileName = Path.GetFileNameWithoutExtension(video.FileName);
            string audiomp3 = $"{fileName}.mp3";
            string audioWav = $"{fileName}.wav";
            using (var fileStream = new FileStream(tempVideoFilePath, FileMode.Create))
            {
               await video.CopyToAsync(fileStream);
            }
            using (var videoStream = video.OpenReadStream())
            {
                using (var reader = new MediaFoundationReader(tempVideoFilePath))
                {
                    MediaFoundationEncoder.EncodeToMp3(reader, audiomp3);
                }
            }
            return await ConvertMp3ToWave(audiomp3, audioWav);
        }
    }
}
