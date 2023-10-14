using NAudio.Wave;
using System.Diagnostics;
using VideoManagerAPI.Models;
using VideoManagerAPI.Repository;

namespace VideoManagerAPI.Services
{
    public class MediaService : IMediaService
    {
        private readonly IResponseService _responseService;
        public MediaService(IResponseService responseService) {
            _responseService = responseService;
        }
        public async Task<APIResponse<string>> ConvertMp3ToWave(string mp3, string wav)
        {
            if(mp3 == null) 
                return _responseService.ErrorResponse<string>("Audio file cannot be null");
            var filePath = Path.Combine("uploads", wav);
            using (var reader = new AudioFileReader(mp3))
            {
                using var resampler = new MediaFoundationResampler(reader, new WaveFormat(16000, reader.WaveFormat.Channels));
                await Task.Run(() =>
                {
                    WaveFileWriter.CreateWaveFile(filePath, resampler);
                });
            }
            return _responseService.SuccessResponse(wav);
        }

        public string ConvertVideoToAudio(string video, string audio)
        {
            var filePath = Path.Combine("uploads", audio);
            using (var reader = new MediaFoundationReader(video))
            {
                MediaFoundationEncoder.EncodeToWma(reader, filePath);
            }
            return audio;
        }

        public APIResponse<string> ExtractAudioFromVideo(string videoFilePath, string audio)
         {
            try
            {
                var filePath = Path.Combine("uploads", audio);
                if(File.Exists(videoFilePath)) {
                    using (var reader = new MediaFoundationReader(videoFilePath))
                    {
                        var pcmStream = WaveFormatConversionStream.CreatePcmStream(reader); // 16 kHz, 16-bit, mono
                        WaveFileWriter.CreateWaveFile(filePath, pcmStream);
                    }
                    return _responseService.SuccessResponse(filePath);
                }
                Console.WriteLine("File not Found");
                return _responseService.ErrorResponse<string>("File Not Found");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
				return _responseService.ErrorResponse<string>("Something went wrong while trying to extract audio from video");
			}
            
         }

        public async Task<APIResponse<string>> ConvertFormVideoToAudio(IFormFile video)
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
            var convertToWav = await ConvertMp3ToWave(audiomp3, audioWav);
            return convertToWav.Status ? _responseService.SuccessResponse(convertToWav.Data) : _responseService.ErrorResponse<string>(convertToWav.Message);
        }
    }

	public interface IMediaService
	{
		Task<APIResponse<string>> ConvertFormVideoToAudio(IFormFile video);
		Task<APIResponse<string>> ConvertMp3ToWave(string mp3, string wav);
		APIResponse<string> ExtractAudioFromVideo(string videoFilePath, string audio);
	}
}
