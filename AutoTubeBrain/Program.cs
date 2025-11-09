using Application;
using Application.Contracts;
using AutoTubeBrain;
using Domain;
using Infrastructure;
using Infrastructure.ImageGen;
using Infrastructure.Storage;
using Infrastructure.TextGen;
using Infrastructure.Tts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Minio;
using Quartz;


var builder = Host.CreateApplicationBuilder(args);

// Db
builder.Services.AddDbContext<YtbDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("Main")
             ?? Environment.GetEnvironmentVariable("ConnectionStrings__Main");
    opt.UseNpgsql(cs);
});

// MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumer<EpisodePlanRequestedConsumer>();
    x.AddConsumer<EpisodeTtsRequestedConsumer>();
    x.AddConsumer<EpisodeImagesRequestedConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        var host = builder.Configuration["Rabbit:Host"]
                   ?? Environment.GetEnvironmentVariable("Rabbit__Host")
                   ?? "amqp://guest:guest@localhost:5672";
        cfg.Host(new Uri(host));
        cfg.ConfigureEndpoints(ctx);
    });
});

// Quartz
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("plan-kick");
    q.AddJob<PlanKickJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(t => t
        .ForJob(jobKey)
        .WithIdentity("plan-kick-trigger")
        .StartNow()
        .WithSimpleSchedule(s => s.WithIntervalInSeconds(10).RepeatForever()));

    var ttsKey = new JobKey("tts-kick");
    q.AddJob<TtsKickJob>(opts => opts.WithIdentity(ttsKey));
    q.AddTrigger(t => t.ForJob(ttsKey).WithIdentity("tts-kick-trigger")
        .StartNow().WithSimpleSchedule(s => s.WithIntervalInSeconds(10).RepeatForever()));

    var imagesKey = new JobKey("images-kick");
    q.AddJob<ImagesKickJob>(opts => opts.WithIdentity(imagesKey));
    q.AddTrigger(t => t.ForJob(imagesKey).WithIdentity("images-kick-trigger")
        .StartNow().WithSimpleSchedule(s => s.WithIntervalInSeconds(10).RepeatForever()));
});
builder.Services.AddQuartzHostedService();

builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<ITextGen, DummyTextGen>();
builder.Services.AddScoped<IScriptFactory, ScriptFactory>();
builder.Services.AddHttpClient<ITts, OpenAiTts>();

// TTS provider switch
var ttsProvider = builder.Configuration["TTS:Provider"]
                  ?? Environment.GetEnvironmentVariable("TTS__Provider")
                  ?? "ElevenLabs";

if (string.Equals(ttsProvider, "ElevenLabs", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddHttpClient<ITts, ElevenLabsTts>();
else if (string.Equals(ttsProvider, "OpenAI", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddHttpClient<ITts, OpenAiTts>();
//else
//    builder.Services.AddSingleton<ITts, DummyTts>(); // optional simple stub if you want one

builder.Services.AddSingleton<IStorage, MinioStorage>();
builder.Services.AddSingleton<IImageGen, DummyImageGen>();
builder.Services.AddSingleton<IMinioClient>(sp =>
{
    var endpoint = builder.Configuration["MINIO:Endpoint"]
                   ?? Environment.GetEnvironmentVariable("MINIO__Endpoint")
                   ?? "http://localhost:9000";
    var access = builder.Configuration["MINIO:AccessKey"]
                 ?? Environment.GetEnvironmentVariable("MINIO__AccessKey")
                 ?? "minio";
    var secret = builder.Configuration["MINIO:SecretKey"]
                 ?? Environment.GetEnvironmentVariable("MINIO__SecretKey")
                 ?? "minio12345";
    return new Minio.MinioClient()
        .WithEndpoint(new Uri(endpoint).Host, new Uri(endpoint).Port)
        .WithCredentials(access, secret)
        .WithSSL(endpoint.StartsWith("https", StringComparison.OrdinalIgnoreCase))
        .Build();
});

var host = builder.Build();

// migrate and seed
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<YtbDbContext>();
    var log = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Bootstrap");

    log.LogInformation("Migrating DB");
    await db.Database.MigrateAsync();
}

await host.RunAsync();
