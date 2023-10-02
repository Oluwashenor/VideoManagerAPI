using VideoManagerAPI.Models;
using VideoManagerAPI.Models.DTO;
using VideoManagerAPI.Repository;

namespace VideoManagerAPI.Services
{
    public class StreamingService : IStreamingService
    {
        private readonly IResponseService _responseService;
        private readonly ITranscriptionService _transcriptionService;
        private static List<ChunkUploadDTO>? chunks = new();

        public StreamingService(IResponseService responseService, ITranscriptionService transcriptionService)
        {
            _responseService = responseService;
            _transcriptionService = transcriptionService;
        }

        public APIResponse<string> StartStream()
        {
            var id = Guid.NewGuid().ToString();
            return _responseService.SuccessResponse(id);
        }

        public async Task<APIResponse<VideoResponse>> StopStream(string id)
        {
            var response = new VideoResponse() { 
                Id = id
            };
            var streamChunks = chunks?.Where(x=>x.Id == id).ToList();
            if (!streamChunks.Any()) return _responseService.ErrorResponse<VideoResponse>("Invalid Id");
            var processor = new APIResponse<string>();
            if(streamChunks.First().Chunk == default)
            {
              processor = ProcessStreams(streamChunks);
            }
            else
            {
                processor = ProcessByteStreams(streamChunks);
            }
                
            if (processor.Status)
            {
                var transcribe = await _transcriptionService.TranscribeVideo(processor.Data);
                response.Url = processor.Data;
                response.Transcripts = default;
                return _responseService.SuccessResponse(response);
            }
            return _responseService.ErrorResponse<VideoResponse>("Unable to Process your Request"); ;
        }

        private APIResponse<string> ProcessStreams(List<ChunkUploadDTO> streamChunks)
        {
            var videoname = streamChunks.First().Id;
            List<byte> videoBytesList = new List<byte>();
            foreach (var chunk in streamChunks)
            {
                var splitedChunk = chunk.ChunkString?.Split("base64,");
                var formattedString = splitedChunk[1];
                byte[] chunkBytes = Convert.FromBase64String(formattedString);
                videoBytesList.AddRange(chunkBytes);
            }
            if (!Directory.Exists("uploads"))
            {
                Directory.CreateDirectory("uploads");
            }
            var filePath = Path.Combine("uploads", $"{videoname}.mp4");
            File.WriteAllBytes(filePath, videoBytesList.ToArray());


            //using (FileStream fs = File.Create("mynewVideo.mp4"))
            //{
            //    fs.Write(videoBytesList.ToArray(), 0, videoBytesList.Count);
            //}

            //using (FileStream fs = new FileStream("mynewVideo.mp4", FileMode.Create))
            //{
            //    fs.Write(videoBytesList.ToArray(), 0, videoBytesList.ToArray().Length);
            //}

            //var generateAudio = MediaService.ConvertVideoToAudio("Grumpy Monkey Says No- Bedtime Story.mp4", "grump.wma");

            return _responseService.SuccessResponse($"{videoname}.mp4");
        }

        private APIResponse<string> ProcessByteStreams(List<ChunkUploadDTO> streamChunks)
        {
            var videoname = streamChunks.First().Id;
            byte[] videoBytes = new byte[0];
            foreach (var chunk in streamChunks)
            {
               videoBytes = videoBytes.Concat(chunk.Chunk).ToArray();
            }
            if (!Directory.Exists("uploads"))
            {
                Directory.CreateDirectory("uploads");
            }
            var filePath = Path.Combine("uploads", $"{videoname}.mp4");
            File.WriteAllBytes(filePath, videoBytes);

            return _responseService.SuccessResponse($"{videoname}.mp4");
        }


        public APIResponse<string> UploadStreamBytes(ChunkUploadDTO model)
        {
            chunks?.Add(model);
            return _responseService.SuccessResponse("Successful operation");
        }

        public APIResponse<string> UploadStream(ChunkUploadDTO model)
        {
           chunks?.Add(model);
            return _responseService.SuccessResponse("Successful operation");
        }

    }

    public interface IStreamingService
    {
        APIResponse<string> StartStream();
        Task<APIResponse<VideoResponse>> StopStream(string id);
        APIResponse<string> UploadStream(ChunkUploadDTO model);
        APIResponse<string> UploadStreamBytes(ChunkUploadDTO model);
    }
}
