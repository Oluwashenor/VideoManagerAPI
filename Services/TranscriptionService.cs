using NAudio.Wave;
using VideoManagerAPI.Models;
using Whisper.net;
using Whisper.net.Ggml;

namespace VideoManagerAPI.Services
{
    public interface ITranscriptionService
    {
        Task<List<Transcript>> ProcessTranscript(string file);
    }
    public class TranscriptionService : ITranscriptionService
    {

        public async Task<List<Transcript>> ProcessTranscript(string file)
        {
            List<Transcript> transcripts = new List<Transcript>();
            var ggmlType = GgmlType.Base;
            var modelFileName = "ggml-base.bin";
            var wavFileName = file;
            var newWavFileName = $"new-{file}";

            // This section detects whether the "ggml-base.bin" file exists in our project disk. If it doesn't, it downloads it from the internet
            if (!File.Exists(modelFileName))
            {
                await DownloadModel(modelFileName, ggmlType);
            }

            // This section creates the whisperFactory object which is used to create the processor object.
            using var whisperFactory = WhisperFactory.FromPath("ggml-base.bin");

            // This section creates the processor object which is used to process the audio file, it uses language `auto` to detect the language of the audio file.
            using var processor = whisperFactory.CreateBuilder()
                .WithLanguage("auto")
                .Build();
            if (!File.Exists(wavFileName))
            {
                Console.WriteLine("File not found");
            }

           // var formattedAudio = MediaService.ConvertAudio(wavFileName, newWavFileName);

            using var fileStream = File.OpenRead(wavFileName);

            // This section processes the audio file and prints the results (start time, end time and text) to the console.
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
