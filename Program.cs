using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using VideoManagerAPI.Models;
using VideoManagerAPI.Repository;

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
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
        return Results.Ok(await fileUploader.UploadVideo(file));
    }
    catch(Exception ex)
    {
        Console.WriteLine(ex.ToString());
        return Results.Problem($"Upload Failed : {ex.Message.ToString()}");
    }
   
}).WithTags("Uploads")
.Produces(200).Produces(400).Produces(500).Produces<APIResponse<string>>();

app.Run();