using Microsoft.AspNetCore.Mvc;
using VideoManagerAPI.Models;
using VideoManagerAPI.Models.DTO;
using VideoManagerAPI.Repository;
using VideoManagerAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024;
});

var configuration = builder.Configuration;
builder.Services.Configure<WasabiCredentials>(configuration.GetSection("WasabiKeys"));
builder.Services.AddScoped<FileUploader>();
builder.Services.AddScoped<IResponseService, ResponseService>();
builder.Services.AddScoped<ITranscriptionService, TranscriptionService>();
builder.Services.AddScoped<IStreamingService, StreamingService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
});
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();


app.MapPost("/api/uploadVideo", async (HttpContext context,FileUploader fileUploader) =>
{
    try
    {
        var form = await context.Request.ReadFormAsync();
        if (!form.Files.Any())
        {
            return Results.BadRequest("Please attach file for uploading");
        }
        var file = form.Files[0];
        return Results.Ok(await fileUploader.Processor(file));
    }
    catch(Exception ex)
    {
        Console.WriteLine(ex.ToString());
        return Results.Problem($"Upload Failed : {ex.Message.ToString()}");
    }
   
}).WithTags("Uploads")
.Produces(200).Produces(400).Produces(500).Produces<APIResponse<string>>();

app.MapGet("/api/ProcessAudio", async (ITranscriptionService transcriptionService) =>
{
    await transcriptionService.ProcessTranscript(@"Grumpy Monkey Says No- Bedtime Story.mp3");
    return Results.Ok();
}).WithTags("Processings");

app.MapGet("/api/ProcessVideo", async (ITranscriptionService transcriptionService) =>
{
    //await transcriptionService.TranscribeVideo("sample.mp4");
    //MediaService.ConvertVideoToAudio("Grumpy Monkey Says No- Bedtime Story.mp4", "grump.wma");
    return Results.Ok();
}).WithTags("Processings");

app.MapGet("/api/startStream", async (IStreamingService streamingService) =>
{
    var id = streamingService.StartStream();
    return Results.Ok(id);
}).WithTags("Streaming")
.Produces(200).Produces(500).Produces<APIResponse<string>>();

app.MapGet("/api/stopStream/{id}", async (IStreamingService streamingService, string id) =>
{
    return Results.Ok(streamingService.StopStream(id));
}).WithTags("Streaming")
.Produces(200).Produces(500).Produces<APIResponse<List<ChunkUploadDTO>>>();

app.MapPost("/api/uploadStream", async (ChunkUploadDTO model, IStreamingService streamingService) =>
{
    var uploadStream = streamingService.UploadStream(model);
    return Results.Ok(model.Id);
}).WithTags("Streaming")
.Produces(200).Produces(500).Produces<APIResponse<string>>();

app.MapPost("/api/uploadStreamInBytes/{id}", async (HttpContext context, IStreamingService streamingService, string id) =>
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
    var uploadStream = streamingService.UploadStreamBytes(model);
    return Results.Ok();
}).WithTags("Streaming")
.Produces(200).Produces(500).Produces<APIResponse<string>>();

app.Run();