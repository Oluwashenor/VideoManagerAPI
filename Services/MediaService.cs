using NAudio.Wave;
using System.Diagnostics;

namespace VideoManagerAPI.Services
{
    public class MediaService
    {
        public static async Task<string> ConvertMp3ToWave(string mp3, string wav)
        {
            var filePath = Path.Combine("uploads", wav);
            using (var reader = new AudioFileReader(mp3))
            {
                using var resampler = new MediaFoundationResampler(reader, new WaveFormat(16000, reader.WaveFormat.Channels));
                await Task.Run(() =>
                {
                    WaveFileWriter.CreateWaveFile(filePath, resampler);
                });
            }
            return wav;
        }

        public static string ConvertVideoToAudio(string video, string audio)
        {
            var filePath = Path.Combine("uploads", audio);
            using (var reader = new MediaFoundationReader(video))
            {
                MediaFoundationEncoder.EncodeToWma(reader, filePath);
            }
            return audio;
        }

        public static string ExtractAudioFromVideo(string videoFilePath, string audio)
         {
            try
            {
                var filePath = Path.Combine("uploads", audio);
                using (var reader = new MediaFoundationReader(videoFilePath))
                {
                    var pcmStream = WaveFormatConversionStream.CreatePcmStream(reader); // 16 kHz, 16-bit, mono
                    WaveFileWriter.CreateWaveFile(filePath, pcmStream);
                }
                return filePath;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return default;
            }
            
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
