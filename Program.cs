using Exceptionless;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using VideoManagerAPI.APIRoutes;
using VideoManagerAPI.Data;
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

builder.Services.AddExceptionless("h0Ei4DuXml3wSbmoGQl9rUWy5FvZvpZ87btWaiRQ");


var configuration = builder.Configuration;
//builder.Services.Configure<WasabiCredentials>(configuration.GetSection("WasabiKeys"));
builder.Services.AddScoped<FileUploader>();
builder.Services.AddScoped<IResponseService, ResponseService>();
builder.Services.AddScoped<ITranscriptionService, TranscriptionService>();
builder.Services.AddScoped<IStreamingService, StreamingService>();
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(configuration.GetConnectionString("constringRemote")));

builder.Services.AddHangfire(configuration => configuration
       //.UseFilter(new AutomaticRetryAttribute{ Attempts = 0 })
       .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
       .UseSimpleAssemblyNameTypeSerializer()
       .UseRecommendedSerializerSettings()
       .UseSqlServerStorage(builder.Configuration.GetConnectionString("constringRemote"), new SqlServerStorageOptions
       {
           CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
           SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
           QueuePollInterval = TimeSpan.Zero,
           UseRecommendedIsolationLevel = true,
           DisableGlobalLocks = true
       }));

builder.Services.AddHangfireServer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
});
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseExceptionless();

app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.UseDeveloperExceptionPage();

app.MapStreamingRoutes();
app.MapProcessingRoutes();

app.Run();