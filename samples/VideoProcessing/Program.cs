using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.WorkflowCore;
using Hangfire.WorkflowCore.Abstractions;
using Hangfire.WorkflowCore.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VideoProcessing;
using WorkflowCore.Interface;

Console.WriteLine("🎬 Video Processing Demo - Hangfire.WorkflowCore Integration");
Console.WriteLine("=============================================================\n");

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

var jobId1 = BackgroundJobWorkflow.Enqueue<VideoProcessingWorkflow, VideoData>(video1);
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
    var jobId = BackgroundJobWorkflow.ScheduleWorkflow<VideoProcessingWorkflow, VideoData>(video, delay);
    Console.WriteLine($"📅 Scheduled {video.VideoId} for {video.Quality} quality (delay: {delay.TotalSeconds}s) - Job: {jobId}");
}

Console.WriteLine();

// Demo 3: Create a recurring video processing job
Console.WriteLine("Demo 3: Setting up recurring video processing");
Console.WriteLine("---------------------------------------------");

var recurringVideo = new VideoData
{
    VideoId = "daily-summary",
    FilePath = "/input/daily-template.mp4",
    Quality = VideoQuality.HD
};

RecurringJobWorkflow.AddOrUpdateWorkflow<VideoProcessingWorkflow, VideoData>(
    "daily-video-processing",
    recurringVideo,
    "0 9 * * *"); // Daily at 9 AM

Console.WriteLine("⏰ Set up recurring video processing job (daily at 9 AM)");
Console.WriteLine("   Job ID: daily-video-processing\n");

// Demo 4: Create a continuation workflow
Console.WriteLine("Demo 4: Creating workflow continuation");
Console.WriteLine("--------------------------------------");

var parentVideo = new VideoData
{
    VideoId = "main-video",
    FilePath = "/input/main-content.mp4",
    Quality = VideoQuality.FullHD
};

var parentJobId = BackgroundJobWorkflow.Enqueue<VideoProcessingWorkflow, VideoData>(parentVideo);
Console.WriteLine($"🎬 Started main video processing: {parentJobId}");

var thumbnailVideo = new VideoData
{
    VideoId = "main-video-thumbnail",
    FilePath = "/input/main-content.mp4",
    Quality = VideoQuality.SD
};

var continuationJobId = BackgroundJobWorkflow.ContinueWorkflowWith<VideoProcessingWorkflow, VideoData>(
    parentJobId, thumbnailVideo);

Console.WriteLine($"🖼️  Scheduled thumbnail generation after main video: {continuationJobId}\n");

// Let the workflows run for a bit
Console.WriteLine("⏳ Letting workflows execute... (watching for 15 seconds)");
await Task.Delay(15000);

Console.WriteLine("\n🎯 Demo completed! Key features demonstrated:");
Console.WriteLine("   ✅ Immediate workflow execution");
Console.WriteLine("   ✅ Scheduled workflow execution with delays");
Console.WriteLine("   ✅ Recurring workflow scheduling");
Console.WriteLine("   ✅ Workflow continuation (dependent jobs)");
Console.WriteLine("   ✅ Integration between Hangfire job scheduling and WorkflowCore orchestration");

// Clean up
Console.WriteLine("\n🧹 Cleaning up recurring jobs...");
RecurringJobWorkflow.RemoveWorkflow("daily-video-processing");

await workflowHost.StopAsync(CancellationToken.None);
await host.StopAsync(CancellationToken.None);

Console.WriteLine("👋 Demo finished!");
Environment.Exit(0);

// Mock implementations for demo purposes
public class MockWorkflowStorageBridge : IWorkflowStorageBridge
{
    private readonly Dictionary<string, string> _jobToWorkflow = new();
    private readonly Dictionary<string, string> _workflowToJob = new();
    private readonly Dictionary<string, WorkflowExecutionResult> _results = new();

    public Task StoreJobWorkflowMappingAsync(string jobId, string workflowInstanceId)
    {
        _jobToWorkflow[jobId] = workflowInstanceId;
        _workflowToJob[workflowInstanceId] = jobId;
        Console.WriteLine($"📝 Stored mapping: Job {jobId} -> Workflow {workflowInstanceId}");
        return Task.CompletedTask;
    }

    public Task<string?> GetWorkflowInstanceIdAsync(string jobId)
    {
        _jobToWorkflow.TryGetValue(jobId, out var workflowId);
        return Task.FromResult<string?>(workflowId);
    }

    public Task<string?> GetJobIdAsync(string workflowInstanceId)
    {
        _workflowToJob.TryGetValue(workflowInstanceId, out var jobId);
        return Task.FromResult<string?>(jobId);
    }

    public Task StoreWorkflowResultAsync(string workflowInstanceId, WorkflowExecutionResult result)
    {
        _results[workflowInstanceId] = result;
        Console.WriteLine($"💾 Stored result for workflow {workflowInstanceId}: {result.Status}");
        return Task.CompletedTask;
    }

    public Task<WorkflowExecutionResult?> GetWorkflowResultAsync(string workflowInstanceId)
    {
        _results.TryGetValue(workflowInstanceId, out var result);
        return Task.FromResult(result);
    }

    public Task<bool> DeleteMappingAsync(string jobId)
    {
        var removed = _jobToWorkflow.Remove(jobId, out var workflowId);
        if (workflowId != null)
        {
            _workflowToJob.Remove(workflowId);
            _results.Remove(workflowId);
        }
        return Task.FromResult(removed);
    }

    public Task<IDictionary<string, string>> GetAllMappingsAsync()
    {
        return Task.FromResult<IDictionary<string, string>>(_jobToWorkflow.ToDictionary(kv => kv.Key, kv => kv.Value));
    }

    public Task<int> CleanupOldEntriesAsync(DateTime olderThan)
    {
        // For mock implementation, just return 0
        return Task.FromResult(0);
    }
}

public class MockWorkflowInstanceProvider : IWorkflowInstanceProvider
{
    public async Task<WorkflowCore.Models.WorkflowInstance?> GetWorkflowInstanceAsync(string workflowInstanceId)
    {
        // Simulate getting workflow instance
        await Task.Delay(100);
        
        return new WorkflowCore.Models.WorkflowInstance
        {
            Id = workflowInstanceId,
            Status = WorkflowCore.Models.WorkflowStatus.Complete,
            CompleteTime = DateTime.UtcNow,
            Data = new VideoData { VideoId = "mock-video" }
        };
    }
}