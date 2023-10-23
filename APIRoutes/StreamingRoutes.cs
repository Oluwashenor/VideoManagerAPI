using Microsoft.AspNetCore.Routing;
using VideoManagerAPI.Models.DTO;
using VideoManagerAPI.Models;
using VideoManagerAPI.Services;

namespace VideoManagerAPI.APIRoutes
{
    public static class StreamingRoutes
    {
        public static IEndpointRouteBuilder MapStreamingRoutes(this IEndpointRouteBuilder builder)
        {
            builder.MapGet("/api/startStream", async (IStreamingService streamingService) =>
            {
                var id = await streamingService.StartStream();
                return Results.Ok(id);
            }).WithTags("Streaming").Produces(200).Produces(500).Produces<APIResponse<string>>();

            builder.MapGet("/api/stopStream/{id}", async (IStreamingService streamingService, string id) =>
            {
                var stopStream = await streamingService.StopStream(id);
                return Results.Ok(stopStream);
            }).WithTags("Streaming")
            .Produces(200).Produces(500).Produces<APIResponse<List<ChunkUploadDTO>>>();

            builder.MapGet("/api/getStream/{id}", async (IStreamingService streamingService, string id) =>
            {
                var stream = await streamingService.GetStream(id);
                return Results.Ok(stream);
            }).WithTags("Streaming")
            .Produces(200).Produces<APIResponse<VideoResponse>>().Produces(500);

            builder.MapPost("/api/uploadStreamInBytes/{id}", async (HttpContext context, IStreamingService streamingService, string id) =>
            {
                byte[] byteArray;
                using (MemoryStream ms = new MemoryStream())
                {
                    await context.Request.Body.CopyToAsync(ms);
                    byteArray = ms.ToArray();
                }
                var model = new ChunkUploadDTO()
                {
                    Chunk = byteArray,
                    Id = id
                };
                var uploadStream = await streamingService.UploadStreamBytes(model);
                return Results.Ok(uploadStream);
            }).WithTags("Streaming").Produces(200).Produces(500).Produces<APIResponse<string>>();

            builder.MapPost("/api/uploadStream", async (ChunkUploadDTO model, IStreamingService streamingService) =>
            {
                var uploadStream = await streamingService.UploadStream(model);
                return Results.Ok(model.Id);
            }).WithTags("Streaming")
           .Produces(200).Produces(500).Produces<APIResponse<string>>();

            return builder;
        }
    }
}
