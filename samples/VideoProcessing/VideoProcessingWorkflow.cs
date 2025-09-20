using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace VideoProcessing;

/// <summary>
/// A comprehensive video processing workflow that demonstrates the integration
/// between Hangfire and WorkflowCore for long-running tasks
/// </summary>
public class VideoProcessingWorkflow : IWorkflow<VideoData>
{
    public string Id => "VideoProcessingWorkflow";
    public int Version => 1;

    public void Build(IWorkflowBuilder<VideoData> builder)
    {
        builder
            .StartWith<AnalyzeVideoStep>()
            .Then<ConvertVideoStep>()
            .Then<UploadVideoStep>()
            .Then<CleanupStep>();
    }
}

/// <summary>
/// Analyzes the input video file to determine properties and requirements
/// </summary>
public class AnalyzeVideoStep : StepBody
{
    public override ExecutionResult Run(IStepExecutionContext context)
    {
        var data = context.Workflow.Data as VideoData;
        if (data == null)
            return ExecutionResult.Next();

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Analyzing video: {data.VideoId}");
        data.Status = VideoStatus.Analyzing;

        // Simulate video analysis
        Thread.Sleep(2000);

        // Simulate extracting video properties
        data.FileSizeBytes = new Random().Next(100_000_000, 1_000_000_000); // 100MB - 1GB
        data.DurationSeconds = new Random().Next(30, 3600); // 30s - 1hour

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Analysis complete - Size: {data.FileSizeBytes / 1024 / 1024}MB, Duration: {data.DurationSeconds}s");
        
        return ExecutionResult.Next();
    }
}

/// <summary>
/// Converts the video to the target quality
/// </summary>
public class ConvertVideoStep : StepBody
{
    public override ExecutionResult Run(IStepExecutionContext context)
    {
        var data = context.Workflow.Data as VideoData;
        if (data == null)
            return ExecutionResult.Next();

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Converting video {data.VideoId} to {data.Quality} quality");
        data.Status = VideoStatus.Converting;

        // Simulate conversion time based on file size and target quality
        var conversionTimeMs = (int)(data.FileSizeBytes / 10_000_000) * (int)data.Quality / 100;
        Thread.Sleep(Math.Min(conversionTimeMs, 5000)); // Cap at 5 seconds for demo

        data.OutputPath = $"/output/{data.VideoId}_{data.Quality}.mp4";
        
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Conversion complete: {data.OutputPath}");
        
        return ExecutionResult.Next();
    }
}

/// <summary>
/// Uploads the processed video to storage
/// </summary>
public class UploadVideoStep : StepBody
{
    public override ExecutionResult Run(IStepExecutionContext context)
    {
        var data = context.Workflow.Data as VideoData;
        if (data == null)
            return ExecutionResult.Next();

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Uploading video: {data.VideoId}");
        data.Status = VideoStatus.Uploading;

        // Simulate upload based on file size
        var uploadTimeMs = data.FileSizeBytes / 50_000_000; // Simulate 50MB/s upload speed
        Thread.Sleep(Math.Min((int)uploadTimeMs, 3000)); // Cap at 3 seconds for demo

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Upload complete for video: {data.VideoId}");
        
        return ExecutionResult.Next();
    }
}

/// <summary>
/// Cleans up temporary files and marks the workflow as complete
/// </summary>
public class CleanupStep : StepBody
{
    public override ExecutionResult Run(IStepExecutionContext context)
    {
        var data = context.Workflow.Data as VideoData;
        if (data == null)
            return ExecutionResult.Next();

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Cleaning up temporary files for video: {data.VideoId}");

        // Simulate cleanup
        Thread.Sleep(500);

        data.Status = VideoStatus.Completed;
        data.CompletedAt = DateTime.UtcNow;

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✅ Video processing completed: {data.VideoId}");
        
        return ExecutionResult.Next();
    }
}