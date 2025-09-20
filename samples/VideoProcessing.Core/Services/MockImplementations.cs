using Hangfire.WorkflowCore.Abstractions;
using VideoProcessing.Core.Models;
using WorkflowCore.Models;

namespace VideoProcessing.Core.Services;

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
    public async Task<WorkflowInstance?> GetWorkflowInstanceAsync(string workflowInstanceId)
    {
        // Simulate getting workflow instance
        await Task.Delay(100);
        
        return new WorkflowInstance
        {
            Id = workflowInstanceId,
            Status = WorkflowStatus.Complete,
            CompleteTime = DateTime.UtcNow,
            Data = new VideoData { VideoId = "mock-video" }
        };
    }
}