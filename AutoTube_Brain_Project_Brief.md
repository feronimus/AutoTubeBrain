# AutoTube Brain Project Brief

## Goal
Automated long form YouTube content pipeline that scripts, narrates, illustrates, composes and uploads. Fully headless after setup. Budget safe during dev.

## Architecture
Worker service in .NET 8. EF Core PostgreSQL. MassTransit with RabbitMQ. Quartz for kick jobs. MinIO for storage. ElevenLabs for TTS behind ITts. Image generation behind IImageGen. Docker Compose runtime.

## States and Messages
States: Planned, Scripted, TtsReady, VisualsReady, Composed, Uploaded.
Messages: EpisodePlanRequested, EpisodeTtsRequested, EpisodeImagesRequested.
Future: Compose, Upload, Shorts.

## Dev Commands
```
docker compose up -d
dotnet ef migrations add <Name> -p Infrastructure -s Infrastructure
dotnet ef database update -p Infrastructure -s Infrastructure
docker compose logs -f worker
```

## Configuration
ConnectionStrings__Main, Rabbit__Host, MINIO__Endpoint and keys,
TTS__Provider and ElevenLabs keys, DryRun flags and MaxScenes limits.

## Collaboration Rules
Return complete files. Avoid secrets. Keep costs low. Update docs/LLM_COLLABORATION_GUIDE.md when contracts or states change.

## Kickoff Prompt
You are the AutoTube Brain copilot. Work only on this repository. Read docs/LLM_COLLABORATION_GUIDE.md and the solution layout. Propose the next three high value steps to reach end to end video generation. Return only complete files for any changes.
