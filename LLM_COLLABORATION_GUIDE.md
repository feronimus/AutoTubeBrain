
# AutoTube Brain LLM Collaboration Guide

## Purpose
Single source of truth for any Large Language Model that assists on this repository.  
Keep this file updated whenever architecture, states, messages, services or external providers change.

## Scope
Focus only on this repository. Output C# 12 targeting .NET 8.  
No unrelated advice. No broad tutorials.

## Ground rules
1. When proposing code, return complete files with full content. No placeholders.  
2. Respect Domain Driven boundaries: Domain, Application, Infrastructure, Worker.  
3. Avoid secrets in code. Read configuration from environment or appsettings.  
4. Optimize for low cost during development. Prefer DryRun flags and MaxScenes limits.  
5. Prefer deterministic jobs and idempotent consumers.  
6. When changing behavior or contracts, update this guide in the same pull request.

## Current architecture
* Worker service orchestrated with MassTransit and RabbitMQ
* Quartz for periodic kick jobs
* EF Core PostgreSQL with Npgsql
* MinIO for object storage, bucket `media`
* ElevenLabs for Text to Speech via `ITts` provider abstraction
* Image generation behind `IImageGen`, dummy implementation available
* Docker Compose services: db, rabbitmq, minio, worker

## Solution map
* Domain: Entities and enums only
* Application: Contracts (messages, DTOs)
* Infrastructure: Persistence, providers, storage, text and image generation
* AutoTubeBrain: Worker host, jobs, consumers

## State machine
Episode status values in order
1. Planned
2. Scripted
3. TtsReady
4. VisualsReady
5. Composed
6. Uploaded
All transitions must be persisted and monotonic. Consumers publish the next message after successful persistence.

## Messages
* EpisodePlanRequested
* EpisodeTtsRequested
* EpisodeImagesRequested
Planned messages to add next
* EpisodeComposeRequested
* EpisodeUploadRequested
* EpisodeShortsRequested

## Entities
* Episode  Id, ChannelKey, Slug, Title, Description, SceneJson, Status, VideoPath, CreatedAt, UpdatedAt
* Asset  Id, EpisodeId, Kind Audio Image Video, Path, Mime, Bytes, DurationSec, CreatedAt
Planned  Channel, JobRun

## Configuration
Environment variables in worker
* ConnectionStrings__Main
* Rabbit__Host
* MINIO__Endpoint, MINIO__AccessKey, MINIO__SecretKey
* TTS__Provider  ElevenLabs by default
* ElevenLabs__ApiKey, ElevenLabs__VoiceId
* DryRun and TTS__MaxScenes
* DryRunImages and IMAGES__MaxScenes

## Development flow
1. `docker compose up -d` to run dependencies and worker
2. Use Quartz kick jobs to move episodes by status
3. Use pgAdmin against localhost:5432 to inspect tables
4. Use MinIO console on localhost:9001 to inspect objects

## Update protocol
Whenever a pull request modifies any of the following, update this guide
* Episode states or transitions
* Contracts in Application layer
* External provider usage or configuration
* Docker compose services or environment
Checklist for the PR description
* What changed
* Why it changed
* Dev commands to run
* Env additions or removals
* Sections of this guide updated

## Prompt seeds

### Kickoff
Use this when a new model joins the project.
```
You are the AutoTube Brain copilot. Work only on this repository. 
Read docs/LLM_COLLABORATION_GUIDE.md and the solution layout. 
Propose the next three high value steps to reach end to end video generation.
Return only complete files for any changes.
```

### Code change request
```
Goal: <one sentence>
Constraints: .NET 8, MassTransit, Quartz, EF Core, MinIO
Deliver: complete files, migrations, compose env additions, and exact run commands.
Update docs/LLM_COLLABORATION_GUIDE.md where relevant.
```

### Review
```
Review this pull request for correctness, idempotency, and cost safety. 
Check state transitions, retries, and configuration usage. 
List risks and propose fixes.
```

## Cost safety
* Keep DryRun true by default in development compose
* Limit scenes with TTS__MaxScenes and IMAGES__MaxScenes
* Add budget gates in Channel when implemented

## Glossary
* SceneJson  structured script with scenes, beats, narration and visual prompts
* Asset  any stored object in MinIO such as audio, images, videos
