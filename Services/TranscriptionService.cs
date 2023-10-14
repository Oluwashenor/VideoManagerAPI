using Hangfire;
using System.Diagnostics;
using VideoManagerAPI.Data;
using VideoManagerAPI.Models;
using VideoManagerAPI.Repository;
using Whisper.net;
using Whisper.net.Ggml;

namespace VideoManagerAPI.Services
{
	public interface ITranscriptionService
	{
		Task<APIResponse<List<Transcript>>> ProcessTranscript(string wavFile);
		Task<APIResponse<bool>> TranscribeAndSave(string video);
		Task<APIResponse<List<Transcript>>> TranscribeVideo(string video);
	}
	public class TranscriptionService : ITranscriptionService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMediaService _mediaService;
        private readonly IResponseService _responseService;

		public TranscriptionService(AppDbContext appDbContext, IMediaService mediaService, IResponseService responseService)
		{
			_appDbContext = appDbContext;
			_mediaService = mediaService;
            _responseService = responseService;
		}

		public async Task<APIResponse<bool>> TranscribeAndSave(string video)
        {
            var transcription = await TranscribeVideo(video);
            if (!transcription.Status) return _responseService.ErrorResponse<bool>(transcription.Message);
            var transcripts = transcription.Data;
            var videoId = Path.GetFileNameWithoutExtension(video);
            if (transcripts.Any())
            {
                foreach (var transcript in transcripts)
                {
                    transcript.VideoId = videoId;
                }
                try
                {
                    await _appDbContext.AddRangeAsync(transcripts);
                    var saved = await _appDbContext.SaveChangesAsync() > 0;
                    return saved ? _responseService.SuccessResponse(true) : _responseService.ErrorResponse<bool>("Unable to save your transcripts");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                    return _responseService.ErrorResponse<bool>("Unable to save your transcripts");
                }
            }
            return _responseService.ErrorResponse<bool>("Unable to transcribe your video");
        }


        public async Task<APIResponse<List<Transcript>>> TranscribeVideo(string video)
        {
            var audioName = Path.GetFileNameWithoutExtension(video);
            var filePath = Path.Combine("uploads", $"{video}");
            var convertToWMA = _mediaService.ExtractAudioFromVideo(filePath, $"{audioName}.wma");
            if (!convertToWMA.Status) return _responseService.ErrorResponse<List<Transcript>>(convertToWMA.Message);
            var convertToWav = await _mediaService.ConvertMp3ToWave(convertToWMA.Data, $"{audioName}.wav");
			if (!convertToWav.Status) return _responseService.ErrorResponse<List<Transcript>>(convertToWav.Message);
			var transcriptProcessor = await ProcessTranscript(convertToWav?.Data);
            return transcriptProcessor;

		}

        public async Task<APIResponse<List<Transcript>>> ProcessTranscript(string wavFile)
        {
            wavFile = Path.Combine("uploads", wavFile);
            List<Transcript> transcripts = new List<Transcript>();
            var ggmlType = GgmlType.Base;
            var modelFileName = "ggml-base.bin";
            if (!File.Exists(modelFileName))
            {
                await DownloadModel(modelFileName, ggmlType);
            }
            using var whisperFactory = WhisperFactory.FromPath("ggml-base.bin");
            using var processor = whisperFactory.CreateBuilder()
                .WithLanguage("auto")
                .Build();
            if (!File.Exists(wavFile))
            {
                Console.WriteLine("File not found");
                return _responseService.ErrorResponse<List<Transcript>>("Wav file not found");
            }
            using var fileStream = File.OpenRead(wavFile);
            await foreach (var result in processor.ProcessAsync(fileStream))
            {
                transcripts.Add(new Transcript()
                {
                    Text = result.Text,
                    Start = result.Start,
                    End = result.End,
                });
                Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
            }
            try
            {
				return _responseService.SuccessResponse(transcripts);
			}
            catch(Exception ex)
            {
                return default;
            }
          
        }

        private static async Task DownloadModel(string fileName, GgmlType ggmlType)
        {
            Console.WriteLine($"Downloading Model {fileName}");
            using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
            using var fileWriter = File.OpenWrite(fileName);
            await modelStream.CopyToAsync(fileWriter);
        }
    }
}
