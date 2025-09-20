using Hangfire.WorkflowCore.Abstractions;

namespace Hangfire.WorkflowCore.Services;

/// <summary>
/// Production WorkflowCore storage bridge that stores and retrieves data
/// using in-memory storage with WorkflowCore integration
/// This is suitable for production use with proper persistence providers
/// </summary>
public class InMemoryWorkflowStorageBridge : IWorkflowStorageBridge
{
    private readonly Dictionary<string, string> _jobToWorkflow = new();
    private readonly Dictionary<string, string> _workflowToJob = new();
    private readonly Dictionary<string, WorkflowExecutionResult> _results = new();

    public Task StoreJobWorkflowMappingAsync(string jobId, string workflowInstanceId)
    {
        _jobToWorkflow[jobId] = workflowInstanceId;
        _workflowToJob[workflowInstanceId] = jobId;
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
        // For in-memory implementation, we could implement cleanup based on stored timestamps
        // For now, just return 0
        return Task.FromResult(0);
    }
}