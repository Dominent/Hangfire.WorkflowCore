using FluentAssertions;
using Hangfire.Server;
using Hangfire.WorkflowCore.Abstractions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hangfire.WorkflowCore.Tests;

public class WorkflowJobPerformContextTests
{
    private readonly IWorkflowHost _workflowHost;
    private readonly IWorkflowStorageBridge _storageBridge;
    private readonly IWorkflowInstanceProvider _workflowInstanceProvider;
    private readonly ILogger<WorkflowJob<TestWorkflow, TestData>> _logger;
    private readonly PerformContext _performContext;

    public WorkflowJobPerformContextTests()
    {
        _workflowHost = Substitute.For<IWorkflowHost>();
        _storageBridge = Substitute.For<IWorkflowStorageBridge>();
        _workflowInstanceProvider = Substitute.For<IWorkflowInstanceProvider>();
        _logger = Substitute.For<ILogger<WorkflowJob<TestWorkflow, TestData>>>();
        _performContext = Substitute.For<PerformContext>();
    }

    [Fact]
    public void WorkflowJob_Should_Extract_JobId_From_PerformContext()
    {
        // Arrange
        var expectedJobId = "test-job-123";
        var data = new TestData { Name = "Test", Value = 42 };
        var jsonData = System.Text.Json.JsonSerializer.Serialize(data);
        var workflowInstanceId = "workflow-instance-456";
        
        // Mock PerformContext to return our test job ID
        _performContext.BackgroundJob.Id.Returns(expectedJobId);
        
        _workflowHost.StartWorkflow(typeof(TestWorkflow).Name, Arg.Any<TestData>())
            .Returns(Task.FromResult(workflowInstanceId));
        
        var workflowInstance = new WorkflowInstance
        {
            Id = workflowInstanceId,
            Status = WorkflowStatus.Complete,
            CompleteTime = DateTime.UtcNow,
            Data = data
        };
        
        _workflowInstanceProvider.GetWorkflowInstanceAsync(workflowInstanceId)
            .Returns(Task.FromResult<WorkflowInstance?>(workflowInstance));
        
        var workflowJob = new WorkflowJob<TestWorkflow, TestData>(
            _workflowHost, _storageBridge, _workflowInstanceProvider, _logger);
        
        // Act
        var result = workflowJob.ExecuteWithContextAsync(_performContext, jsonData);
        
        // Assert
        // The method should accept PerformContext and extract job ID from it
        // For now this will fail because we haven't implemented this method yet
        result.Should().NotBeNull();
        
        // Verify that the storage bridge was called with the correct job ID from context
        _storageBridge.Received(1).StoreJobWorkflowMappingAsync(expectedJobId, workflowInstanceId);
    }
}