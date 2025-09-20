using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.WorkflowCore;
using Hangfire.WorkflowCore.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VideoProcessing.Core.Models;
using VideoProcessing.Core.Services;
using VideoProcessing.Core.Workflows;
using WorkflowCore.Interface;

Console.WriteLine("🎬 Video Processing Console Demo - Hangfire.WorkflowCore Integration");
Console.WriteLine("===================================================================\n");

// Configure services
var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices((context, services) =>
{
    // Add logging
    services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

    // Add Hangfire.WorkflowCore with all necessary configurations
    services.AddHangfireWorkflowCore(
        // Configure Hangfire (storage, dashboard, etc.)
        hangfireConfig => hangfireConfig.UseMemoryStorage(),

        // Configure WorkflowCore integration components
        workflowOptions =>
        {
            workflowOptions.UseStorageBridge<MockWorkflowStorageBridge>();
            workflowOptions.UseInstanceProvider<MockWorkflowInstanceProvider>();
        });
});

var host = builder.Build();

// Initialize Hangfire global configuration
GlobalConfiguration.Configuration.UseMemoryStorage();

// Start the host
await host.StartAsync();

var workflowHost = host.Services.GetRequiredService<IWorkflowHost>();
await workflowHost.StartAsync(CancellationToken.None);

// Register the workflow
workflowHost.RegisterWorkflow<VideoProcessingWorkflow, VideoData>();

Console.WriteLine("🚀 Services started. Demonstrating video processing workflows...\n");

// Demo 1: Process a single video immediately
Console.WriteLine("Demo 1: Processing a single video immediately");
Console.WriteLine("----------------------------------------------");

var video1 = new VideoData
{
    VideoId = "video-001",
    FilePath = "/input/vacation-2024.mp4",
    Quality = VideoQuality.HD
};

var jobId1 = BackgroundJobWorkflow.Instance.Enqueue<VideoProcessingWorkflow, VideoData>(video1);
Console.WriteLine($"✅ Enqueued video processing job: {jobId1}\n");

// Wait a bit to let the first job start
await Task.Delay(1000);

// Demo 2: Schedule multiple videos with different delays
Console.WriteLine("Demo 2: Scheduling multiple videos with delays");
Console.WriteLine("-----------------------------------------------");

var videos = new[]
{
    new VideoData { VideoId = "video-002", FilePath = "/input/presentation.mp4", Quality = VideoQuality.FullHD },
    new VideoData { VideoId = "video-003", FilePath = "/input/tutorial.mp4", Quality = VideoQuality.SD },
    new VideoData { VideoId = "video-004", FilePath = "/input/webinar.mp4", Quality = VideoQuality.UltraHD }
};

foreach (var (video, index) in videos.Select((v, i) => (v, i)))
{
    var delay = TimeSpan.FromSeconds(2 * (index + 1));
    var jobId = BackgroundJobWorkflow.Instance.ScheduleWorkflow<VideoProcessingWorkflow, VideoData>(video, delay);
    Console.WriteLine($"📅 Scheduled {video.VideoId} for {video.Quality} quality (delay: {delay.TotalSeconds}s) - Job: {jobId}");
}

Console.WriteLine();

// Demo 3: Create a continuation workflow
Console.WriteLine("Demo 3: Creating workflow continuation");
Console.WriteLine("--------------------------------------");

var parentVideo = new VideoData
{
    VideoId = "main-video",
    FilePath = "/input/main-content.mp4",
    Quality = VideoQuality.FullHD
};

var parentJobId = BackgroundJobWorkflow.Instance.Enqueue<VideoProcessingWorkflow, VideoData>(parentVideo);
Console.WriteLine($"🎬 Started main video processing: {parentJobId}");

var thumbnailVideo = new VideoData
{
    VideoId = "main-video-thumbnail",
    FilePath = "/input/main-content.mp4",
    Quality = VideoQuality.SD
};

var continuationJobId = BackgroundJobWorkflow.Instance.ContinueWorkflowWith<VideoProcessingWorkflow, VideoData>(
    parentJobId, thumbnailVideo);

Console.WriteLine($"🖼️  Scheduled thumbnail generation after main video: {continuationJobId}\n");

// Let the workflows run for a bit
Console.WriteLine("⏳ Letting workflows execute... (watching for 15 seconds)");
await Task.Delay(15000);

Console.WriteLine("\n🎯 Console Demo completed! Key features demonstrated:");
Console.WriteLine("   ✅ Immediate workflow execution");
Console.WriteLine("   ✅ Scheduled workflow execution with delays");
Console.WriteLine("   ✅ Workflow continuation (dependent jobs)");
Console.WriteLine("   ✅ Basic WorkflowJob integration (no HttpContext)");

// Clean up
await workflowHost.StopAsync(CancellationToken.None);
await host.StopAsync(CancellationToken.None);

Console.WriteLine("\n👋 Console demo finished!");
Environment.Exit(0);