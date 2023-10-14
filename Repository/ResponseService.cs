using VideoManagerAPI.Models;

namespace VideoManagerAPI.Repository
{
    public class ResponseService : IResponseService
    {
        public APIResponse<T> ErrorResponse<T>(string? message = null)
        {
            return new APIResponse<T>
            {
                Status = false,
                Message = message ?? "Something went wrong"
            };
        }

        public APIResponse<T> SuccessResponse<T>(T data, string? message = null)
        {
            return new APIResponse<T>
            {
                Status = true,
                Data = data,
                Message = message ?? "Successful Operation"
            };
        }
    }
}
