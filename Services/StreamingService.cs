using VideoManagerAPI.Models;
using VideoManagerAPI.Models.DTO;
using VideoManagerAPI.Repository;

namespace VideoManagerAPI.Services
{
    public class StreamingService : IStreamingService
    {
        private readonly IResponseService _responseService;
        private static List<ChunkUploadDTO>? chunks = new();

        public StreamingService(IResponseService responseService)
        {
            _responseService = responseService;
        }

        public APIResponse<string> StartStream()
        {
            var id = Guid.NewGuid().ToString();
            return _responseService.SuccessResponse(id);
        }

        public APIResponse<List<ChunkUploadDTO>> StopStream(string id)
        {
            var streamChunks = chunks?.Where(x=>x.Id == id).ToList();
            if (!streamChunks.Any()) return _responseService.ErrorResponse<List<ChunkUploadDTO>>("Invalid Id");
            ProcessStreams(streamChunks);
            return _responseService.SuccessResponse(streamChunks);
        }

        private bool ProcessStreams(List<ChunkUploadDTO> streamChunks)
        {
            List<byte> videoBytesList = new List<byte>();
            foreach (var chunk in streamChunks)
            {
                var formattedString = chunk.ChunkString.Replace("data:video/x-matroska;codecs=avc1;base64,", "");
                byte[] chunkBytes = Convert.FromBase64String(formattedString);
                videoBytesList.AddRange(chunkBytes);
            }

            File.WriteAllBytes("myvideo.mkv", videoBytesList.ToArray());
            return true;
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
        APIResponse<List<ChunkUploadDTO>> StopStream(string id);
        APIResponse<string> UploadStream(ChunkUploadDTO model);
    }
}
