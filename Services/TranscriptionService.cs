using System.Diagnostics;
using VideoManagerAPI.Models;
using Whisper.net;
using Whisper.net.Ggml;

namespace VideoManagerAPI.Services
{
    public interface ITranscriptionService
    {
        Task<List<Transcript>> ProcessTranscript(string file);
        Task<List<Transcript>> TranscribeVideo(string VideoPath);
    }
    public class TranscriptionService : ITranscriptionService
    {

        public async Task<List<Transcript>> TranscribeVideo(string video)
        {
            var audioName = Path.GetFileNameWithoutExtension(video);
            var filePath = Path.Combine("uploads", $"{video}");
            var wmafile = MediaService.ExtractAudioFromVideo(filePath, $"{audioName}.wma");
            //var wmafile = MediaService.ConvertVideoToAudio(filePath, $"{audioName}.wma");
            var wavFile = await MediaService.ConvertMp3ToWave(wmafile, $"{audioName}.wav");
            return await ProcessTranscript(wavFile);
        }

        public async Task<List<Transcript>> ProcessTranscript(string wavFile)
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
                return default;
            }
            using var fileStream = File.OpenRead(wavFile);
            await foreach (var result in processor.ProcessAsync(fileStream))
            {
                transcripts.Add(new Transcript
                {
                    Text = result.Text,
                    Start = result.Start,
                    End = result.End
                });
                Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
            }
            return transcripts;
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
