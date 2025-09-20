using Hangfire.WorkflowCore.Abstractions;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hangfire.WorkflowCore.Services;

/// <summary>
/// Production WorkflowCore integration that gets actual workflow instance data
/// from WorkflowCore's persistence layer
/// </summary>
public class WorkflowCoreInstanceProvider : IWorkflowInstanceProvider
{
    private readonly IWorkflowHost _workflowHost;
    private readonly IPersistenceProvider _persistenceProvider;

    public WorkflowCoreInstanceProvider(IWorkflowHost workflowHost, IPersistenceProvider persistenceProvider)
    {
        _workflowHost = workflowHost;
        _persistenceProvider = persistenceProvider;
    }

    public async Task<WorkflowInstance?> GetWorkflowInstanceAsync(string workflowInstanceId)
    {
        try
        {
            // Get the actual workflow instance from WorkflowCore's persistence
            var workflowInstance = await _persistenceProvider.GetWorkflowInstance(workflowInstanceId);
            return workflowInstance;
        }
        catch (Exception)
        {
            // If the workflow instance is not found, return null
            return null;
        }
    }
}