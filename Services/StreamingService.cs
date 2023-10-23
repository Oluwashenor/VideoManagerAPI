using Hangfire;
using Microsoft.EntityFrameworkCore;
using VideoManagerAPI.Data;
using VideoManagerAPI.Models;
using VideoManagerAPI.Models.DTO;
using VideoManagerAPI.Repository;

namespace VideoManagerAPI.Services
{
    public class StreamingService : IStreamingService
    {
        private readonly IResponseService _responseService;
        private readonly ITranscriptionService _transcriptionService;
        private static List<ChunkUploadStringDTO> chunksStrings = new();
        private static List<ChunkUploadByteDTO> chunksBytes = new();
        private readonly AppDbContext _appDbContext;

        public StreamingService(IResponseService responseService, ITranscriptionService transcriptionService, AppDbContext appDbContext)
        {
            _responseService = responseService;
            _transcriptionService = transcriptionService;
            _appDbContext = appDbContext;
        }

        public async Task<APIResponse<string>> StartStream()
        {
            Video video = new();
            await _appDbContext.AddAsync(video);
            if(await _appDbContext.SaveChangesAsync() > 0)
                return _responseService.SuccessResponse(video.Id);
            return _responseService.ErrorResponse<string>("something went wrong");
        }

        public async Task<APIResponse<VideoResponse>> StopStream(string id)
        {
            var processor = new APIResponse<string>();
            var response = new VideoResponse() { 
                Id = id
            };
            List<ChunkUploadByteDTO> streamBytesChunks = chunksBytes.Where(x=>x.Id == id).ToList();
            List<ChunkUploadStringDTO> streamStringChunks = chunksStrings.Where(x=>x.Id == id).ToList();
            if (streamBytesChunks.Any())
            {
                chunksBytes = chunksBytes.Except(streamBytesChunks).ToList();
                processor = await ProcessByteStreams(streamBytesChunks);
            }
            else if (streamStringChunks.Any())
            {
                chunksStrings = chunksStrings.Except(streamStringChunks).ToList();
                processor = await ProcessStreams(streamStringChunks);
            }
            else
            {
                return _responseService.ErrorResponse<VideoResponse>("Invalid Id Passed");
            }
            if (processor.Status)
            {
                try
                {
                    var bJClient = new BackgroundJobClient();
                    bJClient.Enqueue(() => _transcriptionService.TranscribeAndSave(processor.Data));
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
               
                response.Url = processor.Data;
                response.Transcripts = default;
                return _responseService.SuccessResponse(response);
            }
            return _responseService.ErrorResponse<VideoResponse>("Unable to Process your Request"); ;
        }

        private async Task<APIResponse<string>> ProcessStreams(List<ChunkUploadStringDTO> streamChunks)
        {
            var videoName = streamChunks.First().Id;
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
            var filePath = Path.Combine("uploads", $"{videoName}.mp4");
            await File.WriteAllBytesAsync(filePath, videoBytesList.ToArray());
            return _responseService.SuccessResponse($"{videoName}.mp4");
        }

        private async Task<APIResponse<string>> ProcessByteStreams(List<ChunkUploadByteDTO> streamChunks)
        {
            var videoName = streamChunks.First().Id;
            byte[] videoBytes = new byte[0];
            foreach (var chunk in streamChunks)
            {
               videoBytes = videoBytes.Concat(chunk.Chunk).ToArray();
            }
            if (!Directory.Exists("uploads"))
            {
                Directory.CreateDirectory("uploads");
            }
            var filePath = Path.Combine("uploads", $"{videoName}.mp4");
            await File.WriteAllBytesAsync(filePath, videoBytes);
            return _responseService.SuccessResponse($"{videoName}.mp4");
        }
        
        public async Task<APIResponse<string>> UploadStreamBytes(ChunkUploadByteDTO model)
        {
            var videoExist = await _appDbContext.Videos.FirstOrDefaultAsync(x=>x.Id == model.Id);
            if (videoExist == default)
                return _responseService.ErrorResponse<string>("Invalid Video Id Sent, Please verify and try again");
            chunksBytes?.Add(model);
            return _responseService.SuccessResponse("Streamed chunk received and saved successfully");
        }

        public async Task<APIResponse<string>> UploadStream(ChunkUploadStringDTO model)
        {
            var videoExist = await _appDbContext.Videos.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (videoExist == default)
                return _responseService.ErrorResponse<string>("Invalid video id sent, Please verify and try again");
            chunksStrings?.Add(model);
            return _responseService.SuccessResponse("Streamed chunk received and saved successfully");
        }

        public async Task<APIResponse<VideoResponse>> GetStream(string id)
        {
            var response = new VideoResponse();
            Video? videoExist = new Video();
            videoExist = await _appDbContext.Videos.Where(x => x.Id == id).Include(x=>x.Transcripts).FirstOrDefaultAsync();
            if (videoExist == default)
                return _responseService.ErrorResponse<VideoResponse>("Video not Found");
            string absoluteFilePath = Path.Combine(Directory.GetCurrentDirectory(), @"uploads\", $"{id}.mp4");
            var path = Path.Combine("uploads", $"{id}.mp4");
            if (!File.Exists(path))
            {
                response.Id = id;
                response.Transcripts = default;
                response.Url = "";
                return _responseService.ErrorResponse<VideoResponse>("Video Not Ready");
            };
            response.Id = id;
            response.Transcripts = videoExist.Transcripts;
            response.Url = absoluteFilePath;
            return _responseService.SuccessResponse(response);
        }
    }

    public interface IStreamingService
    {
        Task<APIResponse<VideoResponse>> GetStream(string id);
        Task<APIResponse<string>> StartStream();
        Task<APIResponse<VideoResponse>> StopStream(string id);
        Task<APIResponse<string>> UploadStream(ChunkUploadStringDTO model);
        Task<APIResponse<string>> UploadStreamBytes(ChunkUploadByteDTO model);
    }
}
