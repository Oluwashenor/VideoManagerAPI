using Microsoft.Extensions.Options;
using VideoManagerAPI.Models;
using VideoManagerAPI.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var configuration = builder.Configuration;
builder.Services.Configure<WasabiCredentials>(configuration.GetSection("WasabiKeys"));
builder.Services.AddScoped<FileUploader>();
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("/uploadVideo", async (FileUploader fileUploader) =>
{
    return Results.Ok(await fileUploader.UploadVideo());
}).WithTags("Uploads");

app.Run();

