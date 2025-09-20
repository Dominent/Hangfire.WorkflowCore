namespace Hangfire.WorkflowCore.Abstractions;

/// <summary>
/// Provides storage bridge between Hangfire and Workflow Core
/// </summary>
public interface IWorkflowStorageBridge
{
    /// <summary>
    /// Stores the mapping between a Hangfire job ID and a Workflow Core instance ID
    /// </summary>
    /// <param name="jobId">The Hangfire job ID</param>
    /// <param name="workflowInstanceId">The Workflow Core instance ID</param>
    Task StoreJobWorkflowMappingAsync(string jobId, string workflowInstanceId);
    
    /// <summary>
    /// Gets the workflow instance ID for a given Hangfire job ID
    /// </summary>
    /// <param name="jobId">The Hangfire job ID</param>
    /// <returns>The workflow instance ID or null if not found</returns>
    Task<string?> GetWorkflowInstanceIdAsync(string jobId);
    
    /// <summary>
    /// Gets the Hangfire job ID for a given workflow instance ID
    /// </summary>
    /// <param name="workflowInstanceId">The workflow instance ID</param>
    /// <returns>The job ID or null if not found</returns>
    Task<string?> GetJobIdAsync(string workflowInstanceId);
    
    /// <summary>
    /// Stores the result of a workflow execution
    /// </summary>
    /// <param name="workflowInstanceId">The workflow instance ID</param>
    /// <param name="result">The execution result</param>
    Task StoreWorkflowResultAsync(string workflowInstanceId, WorkflowExecutionResult result);
    
    /// <summary>
    /// Gets the result of a workflow execution
    /// </summary>
    /// <param name="workflowInstanceId">The workflow instance ID</param>
    /// <returns>The execution result or null if not found</returns>
    Task<WorkflowExecutionResult?> GetWorkflowResultAsync(string workflowInstanceId);
    
    /// <summary>
    /// Deletes the mapping and result for a job
    /// </summary>
    /// <param name="jobId">The job ID</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteMappingAsync(string jobId);
    
    /// <summary>
    /// Gets all active workflow mappings
    /// </summary>
    /// <returns>Dictionary of job IDs to workflow instance IDs</returns>
    Task<IDictionary<string, string>> GetAllMappingsAsync();
    
    /// <summary>
    /// Cleans up old mappings and results
    /// </summary>
    /// <param name="olderThan">Remove entries older than this date</param>
    /// <returns>Number of entries removed</returns>
    Task<int> CleanupOldEntriesAsync(DateTime olderThan);
}