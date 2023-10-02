using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using VideoManagerAPI.Data;
using VideoManagerAPI.Migrations;
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
        private readonly AppDbContext _appDbContext;

        public StreamingService(IResponseService responseService, ITranscriptionService transcriptionService, AppDbContext appDbContext)
        {
            _responseService = responseService;
            _transcriptionService = transcriptionService;
            _appDbContext = appDbContext;
        }

        public async Task<APIResponse<string>> StartStream()
        {
            var video = new Video
            {

                Created = DateTime.Now,
                Id = Guid.NewGuid().ToString()
            };
            await _appDbContext.AddAsync(video);
            if(await _appDbContext.SaveChangesAsync() > 0)
                return _responseService.SuccessResponse(video.Id);
            return _responseService.ErrorResponse<string>("something went wrong");
        }

        public async Task<APIResponse<VideoResponse>> StopStream(string id)
        {
            var response = new VideoResponse() { 
                Id = id
            };
            var streamChunks = chunks?.Where(x=>x.Id == id).ToList();
            chunks = chunks.Except(streamChunks).ToList();
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
                var transcripts = await _transcriptionService.TranscribeVideo(processor.Data);
                string videoId = Path.GetFileNameWithoutExtension(processor.Data);
                if (transcripts.Count > 0)
                {
                    foreach (var transcript in transcripts)
                    {
                        transcript.VideoId = videoId;
                    }
                    try
                    {
                        await _appDbContext.AddRangeAsync(transcripts);
                        var saved = await _appDbContext.SaveChangesAsync() > 0;   
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message.ToString());
                        return _responseService.ErrorResponse<VideoResponse>("Something went wrong");
                    }
                }
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


        public async Task<APIResponse<string>> UploadStreamBytes(ChunkUploadDTO model)
        {
            var videoExist = await _appDbContext.videos.FirstOrDefaultAsync(x=>x.Id == model.Id);
            if (videoExist == default)
                return _responseService.ErrorResponse<string>("Invalid Video Sent");
            chunks?.Add(model);
            return _responseService.SuccessResponse("Successful operation");
        }

        public async Task<APIResponse<string>> UploadStream(ChunkUploadDTO model)
        {
            var videoExist = await _appDbContext.videos.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (videoExist == default)
                return _responseService.ErrorResponse<string>("Invalid Video Sent");
            chunks?.Add(model);
            return _responseService.SuccessResponse("Successful operation");
        }

        public async Task<APIResponse<VideoResponse>> GetStream(string id)
        {
            var videoExist = await _appDbContext.videos.Where(x => x.Id == id).Include(x=>x.Transcripts).FirstOrDefaultAsync();
            if (videoExist == default)
                return _responseService.ErrorResponse<VideoResponse>("Invalid Video Sent");
            string absoluteFilePath = Path.Combine(Directory.GetCurrentDirectory(), @"uploads\", $"{id}.mp4");

            var path = Path.Combine("uploads", $"{id}.mp4");
           // var url = "https://localhost:7056/" + absoluteFilePath;
            var response = new VideoResponse()
            {
                Id = id,
                Transcripts = videoExist.Transcripts,
                Url = absoluteFilePath,
            };
            return _responseService.SuccessResponse(response);
        }

    }

    public interface IStreamingService
    {
        Task<APIResponse<VideoResponse>> GetStream(string id);
        Task<APIResponse<string>> StartStream();
        Task<APIResponse<VideoResponse>> StopStream(string id);
        Task<APIResponse<string>> UploadStream(ChunkUploadDTO model);
        Task<APIResponse<string>> UploadStreamBytes(ChunkUploadDTO model);
    }
}
