using FluentAssertions;
using Hangfire.WorkflowCore.Abstractions;
using NSubstitute;
using WorkflowCore.Models;

namespace Hangfire.WorkflowCore.Tests.Abstractions;

public class IWorkflowStorageBridgeTests
{
    [Fact]
    public async Task Should_Store_JobToWorkflow_Mapping()
    {
        // Arrange
        var storage = Substitute.For<IWorkflowStorageBridge>();
        var jobId = "job-123";
        var workflowInstanceId = "workflow-456";
        
        storage.StoreJobWorkflowMappingAsync(jobId, workflowInstanceId)
            .Returns(Task.CompletedTask);
        
        // Act
        await storage.StoreJobWorkflowMappingAsync(jobId, workflowInstanceId);
        
        // Assert
        await storage.Received(1).StoreJobWorkflowMappingAsync(jobId, workflowInstanceId);
    }
    
    [Fact]
    public async Task Should_Get_WorkflowInstanceId_By_JobId()
    {
        // Arrange
        var storage = Substitute.For<IWorkflowStorageBridge>();
        var jobId = "job-123";
        var expectedWorkflowId = "workflow-456";
        
        storage.GetWorkflowInstanceIdAsync(jobId)
            .Returns(Task.FromResult<string?>(expectedWorkflowId));
        
        // Act
        var result = await storage.GetWorkflowInstanceIdAsync(jobId);
        
        // Assert
        result.Should().Be(expectedWorkflowId);
    }
    
    [Fact]
    public async Task Should_Store_Workflow_Result()
    {
        // Arrange
        var storage = Substitute.For<IWorkflowStorageBridge>();
        var workflowInstanceId = "workflow-456";
        var result = new WorkflowExecutionResult
        {
            WorkflowInstanceId = workflowInstanceId,
            Status = WorkflowStatus.Complete,
            Data = new { success = true }
        };
        
        storage.StoreWorkflowResultAsync(workflowInstanceId, result)
            .Returns(Task.CompletedTask);
        
        // Act
        await storage.StoreWorkflowResultAsync(workflowInstanceId, result);
        
        // Assert
        await storage.Received(1).StoreWorkflowResultAsync(workflowInstanceId, result);
    }
    
    [Fact]
    public async Task Should_Get_Workflow_Result()
    {
        // Arrange
        var storage = Substitute.For<IWorkflowStorageBridge>();
        var workflowInstanceId = "workflow-456";
        var expectedResult = new WorkflowExecutionResult
        {
            WorkflowInstanceId = workflowInstanceId,
            Status = WorkflowStatus.Complete
        };
        
        storage.GetWorkflowResultAsync(workflowInstanceId)
            .Returns(Task.FromResult<WorkflowExecutionResult?>(expectedResult));
        
        // Act
        var result = await storage.GetWorkflowResultAsync(workflowInstanceId);
        
        // Assert
        result.Should().NotBeNull();
        result!.WorkflowInstanceId.Should().Be(workflowInstanceId);
        result.Status.Should().Be(WorkflowStatus.Complete);
    }
    
    [Fact]
    public async Task Should_Delete_Mapping_And_Result()
    {
        // Arrange
        var storage = Substitute.For<IWorkflowStorageBridge>();
        var jobId = "job-123";
        
        storage.DeleteMappingAsync(jobId)
            .Returns(Task.FromResult(true));
        
        // Act
        var result = await storage.DeleteMappingAsync(jobId);
        
        // Assert
        result.Should().BeTrue();
    }
}