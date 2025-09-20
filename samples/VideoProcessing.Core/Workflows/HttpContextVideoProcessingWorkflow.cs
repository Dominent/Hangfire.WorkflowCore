using Hangfire.WorkflowCore.Abstractions;
using VideoProcessing.Core.Models;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace VideoProcessing.Core.Workflows;

/// <summary>
/// HttpContext-aware video processing workflow that demonstrates HttpContext integration
/// </summary>
public class HttpContextVideoProcessingWorkflow : IWorkflow<WorkflowDataWithContext<VideoData>>
{
    public string Id => "HttpContextVideoProcessingWorkflow";
    public int Version => 1;

    public void Build(IWorkflowBuilder<WorkflowDataWithContext<VideoData>> builder)
    {
        builder
            .StartWith<HttpContextAnalyzeVideoStep>()
            .Then<HttpContextConvertVideoStep>()
            .Then<HttpContextUploadVideoStep>()
            .Then<HttpContextCleanupStep>();
    }
}

/// <summary>
/// HttpContext-aware analyze step that logs request information
/// </summary>
public class HttpContextAnalyzeVideoStep : StepBody
{
    public override ExecutionResult Run(IStepExecutionContext context)
    {
        var dataWithContext = context.Workflow.Data as WorkflowDataWithContext<VideoData>;
        if (dataWithContext?.Data == null)
            return ExecutionResult.Next();

        var data = dataWithContext.Data;
        var httpContext = dataWithContext.HttpContext;

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🌐 Analyzing video with HttpContext: {data.VideoId}");
        
        if (httpContext != null)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 📍 Request Path: {httpContext.RequestPath}");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🔑 User ID: {httpContext.UserId ?? "Anonymous"}");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🕐 Request Time: {httpContext.CreatedAt}");
        }

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
/// HttpContext-aware convert step
/// </summary>
public class HttpContextConvertVideoStep : StepBody
{
    public override ExecutionResult Run(IStepExecutionContext context)
    {
        var dataWithContext = context.Workflow.Data as WorkflowDataWithContext<VideoData>;
        if (dataWithContext?.Data == null)
            return ExecutionResult.Next();

        var data = dataWithContext.Data;
        var httpContext = dataWithContext.HttpContext;

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🎬 Converting video {data.VideoId} to {data.Quality} quality");
        
        if (httpContext?.UserId != null)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 👤 Processing for user: {httpContext.UserId}");
        }

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
/// HttpContext-aware upload step
/// </summary>
public class HttpContextUploadVideoStep : StepBody
{
    public override ExecutionResult Run(IStepExecutionContext context)
    {
        var dataWithContext = context.Workflow.Data as WorkflowDataWithContext<VideoData>;
        if (dataWithContext?.Data == null)
            return ExecutionResult.Next();

        var data = dataWithContext.Data;

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ☁️ Uploading video: {data.VideoId}");
        data.Status = VideoStatus.Uploading;

        // Simulate upload based on file size
        var uploadTimeMs = data.FileSizeBytes / 50_000_000; // Simulate 50MB/s upload speed
        Thread.Sleep(Math.Min((int)uploadTimeMs, 3000)); // Cap at 3 seconds for demo

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Upload complete for video: {data.VideoId}");
        
        return ExecutionResult.Next();
    }
}

/// <summary>
/// HttpContext-aware cleanup step
/// </summary>
public class HttpContextCleanupStep : StepBody
{
    public override ExecutionResult Run(IStepExecutionContext context)
    {
        var dataWithContext = context.Workflow.Data as WorkflowDataWithContext<VideoData>;
        if (dataWithContext?.Data == null)
            return ExecutionResult.Next();

        var data = dataWithContext.Data;

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🧹 Cleaning up temporary files for video: {data.VideoId}");

        // Simulate cleanup
        Thread.Sleep(500);

        data.Status = VideoStatus.Completed;
        data.CompletedAt = DateTime.UtcNow;

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✅ Video processing completed with HttpContext: {data.VideoId}");
        
        return ExecutionResult.Next();
    }
}