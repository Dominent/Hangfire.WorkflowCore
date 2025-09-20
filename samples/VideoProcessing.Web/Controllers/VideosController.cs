using Hangfire.WorkflowCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using VideoProcessing.Core.Models;
using VideoProcessing.Core.Workflows;

namespace VideoProcessing.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideosController : ControllerBase
{
    private readonly ILogger<VideosController> _logger;

    public VideosController(ILogger<VideosController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Process a video immediately using basic WorkflowJob
    /// </summary>
    [HttpPost("process")]
    public ActionResult<VideoProcessResponse> ProcessVideo([FromBody] VideoProcessRequest request)
    {
        _logger.LogInformation("Processing video: {VideoId}", request.VideoId);

        var videoData = new VideoData
        {
            VideoId = request.VideoId,
            FilePath = request.FilePath,
            Quality = request.Quality
        };

        var jobId = BackgroundJobWorkflow.Instance.Enqueue<VideoProcessingWorkflow, VideoData>(videoData);

        return Ok(new VideoProcessResponse
        {
            JobId = jobId,
            VideoId = request.VideoId,
            Status = "Enqueued",
            Message = "Video processing job created successfully"
        });
    }

    /// <summary>
    /// Process a video with HttpContext awareness using HttpContextWorkflowJob
    /// </summary>
    [HttpPost("process-with-context")]
    public ActionResult<VideoProcessResponse> ProcessVideoWithContext([FromBody] VideoProcessRequest request)
    {
        _logger.LogInformation("Processing video with HttpContext: {VideoId}", request.VideoId);

        var videoData = new VideoData
        {
            VideoId = request.VideoId,
            FilePath = request.FilePath,
            Quality = request.Quality
        };

        // This will use HttpContextWorkflowJob which captures current HttpContext
        var jobId = BackgroundJobWorkflow.Instance.EnqueueWithHttpContext<HttpContextVideoProcessingWorkflow, VideoData>(videoData);

        return Ok(new VideoProcessResponse
        {
            JobId = jobId,
            VideoId = request.VideoId,
            Status = "Enqueued with HttpContext",
            Message = "Video processing job created with HttpContext integration"
        });
    }

    /// <summary>
    /// Schedule a video for processing later
    /// </summary>
    [HttpPost("schedule")]
    public ActionResult<VideoProcessResponse> ScheduleVideo([FromBody] VideoScheduleRequest request)
    {
        _logger.LogInformation("Scheduling video: {VideoId} for {DelayMinutes} minutes", 
            request.VideoId, request.DelayMinutes);

        var videoData = new VideoData
        {
            VideoId = request.VideoId,
            FilePath = request.FilePath,
            Quality = request.Quality
        };

        var delay = TimeSpan.FromMinutes(request.DelayMinutes);
        var jobId = BackgroundJobWorkflow.Instance.ScheduleWorkflow<VideoProcessingWorkflow, VideoData>(videoData, delay);

        return Ok(new VideoProcessResponse
        {
            JobId = jobId,
            VideoId = request.VideoId,
            Status = "Scheduled",
            Message = $"Video processing scheduled for {delay.TotalMinutes} minutes from now"
        });
    }

    /// <summary>
    /// Get a list of supported video qualities
    /// </summary>
    [HttpGet("qualities")]
    public ActionResult<IEnumerable<object>> GetVideoQualities()
    {
        var qualities = Enum.GetValues<VideoQuality>()
            .Select(q => new { Name = q.ToString(), Resolution = (int)q })
            .ToList();

        return Ok(qualities);
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public ActionResult<object> Health()
    {
        return Ok(new 
        { 
            Status = "Healthy", 
            Timestamp = DateTime.UtcNow,
            Service = "Video Processing API"
        });
    }

}

public class VideoProcessRequest
{
    public string VideoId { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public VideoQuality Quality { get; set; } = VideoQuality.HD;
}

public class VideoScheduleRequest : VideoProcessRequest
{
    public int DelayMinutes { get; set; } = 5;
}

public class VideoProcessResponse
{
    public string JobId { get; set; } = string.Empty;
    public string VideoId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}