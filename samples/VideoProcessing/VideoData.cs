namespace VideoProcessing;

public class VideoData
{
    public string VideoId { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public VideoQuality Quality { get; set; } = VideoQuality.HD;
    public VideoStatus Status { get; set; } = VideoStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public long FileSizeBytes { get; set; }
    public int DurationSeconds { get; set; }
}

public enum VideoQuality
{
    SD = 480,
    HD = 720,
    FullHD = 1080,
    UltraHD = 2160
}

public enum VideoStatus
{
    Pending,
    Analyzing,
    Converting,
    Uploading,
    Completed,
    Failed
}