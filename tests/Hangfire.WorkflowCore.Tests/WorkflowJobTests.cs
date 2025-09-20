using FluentAssertions;
using Hangfire.WorkflowCore.Abstractions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text.Json;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hangfire.WorkflowCore.Tests;

public class WorkflowJobTests
{
    private readonly IWorkflowHost _workflowHost;
    private readonly IWorkflowStorageBridge _storageBridge;
    private readonly IWorkflowInstanceProvider _workflowInstanceProvider;
    private readonly ILogger<WorkflowJob<TestWorkflow, TestData>> _logger;

    public WorkflowJobTests()
    {
        _workflowHost = Substitute.For<IWorkflowHost>();
        _storageBridge = Substitute.For<IWorkflowStorageBridge>();
        _workflowInstanceProvider = Substitute.For<IWorkflowInstanceProvider>();
        _logger = Substitute.For<ILogger<WorkflowJob<TestWorkflow, TestData>>>();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Start_Workflow_And_Return_Result()
    {
        // Arrange
        var jobId = "hangfire-job-123";
        var data = new TestData { Name = "Test", Value = 42 };
        var jsonData = JsonSerializer.Serialize(data);
        var workflowInstanceId = "workflow-instance-456";

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

        var workflowJob = new WorkflowJob<TestWorkflow, TestData>(_workflowHost, _storageBridge, _workflowInstanceProvider, _logger);

        // Act
        var result = await workflowJob.ExecuteAsync(jobId, jsonData);

        // Assert
        result.Should().NotBeNull();
        result.WorkflowInstanceId.Should().Be(workflowInstanceId);
        result.Status.Should().Be(WorkflowStatus.Complete);
        result.Data.Should().BeEquivalentTo(data);

        // Verify interactions
        await _workflowHost.Received(1).StartWorkflow(
            typeof(TestWorkflow).Name,
            Arg.Is<TestData>(d => d.Name == "Test" && d.Value == 42));
        await _storageBridge.Received(1).StoreJobWorkflowMappingAsync(jobId, workflowInstanceId);
        await _storageBridge.Received(1).StoreWorkflowResultAsync(workflowInstanceId, Arg.Any<WorkflowExecutionResult>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_Handle_Workflow_Failure()
    {
        // Arrange
        var jobId = "hangfire-job-123";
        var jsonData = """{"name": "Test"}""";
        var workflowInstanceId = "workflow-instance-456";

        _workflowHost.StartWorkflow(typeof(TestWorkflow).Name, Arg.Any<TestData>())
            .Returns(Task.FromResult(workflowInstanceId));

        var workflowInstance = new WorkflowInstance
        {
            Id = workflowInstanceId,
            Status = WorkflowStatus.Terminated,
            CompleteTime = DateTime.UtcNow
        };

        _workflowInstanceProvider.GetWorkflowInstanceAsync(workflowInstanceId)
            .Returns(Task.FromResult<WorkflowInstance?>(workflowInstance));

        var workflowJob = new WorkflowJob<TestWorkflow, TestData>(_workflowHost, _storageBridge, _workflowInstanceProvider, _logger);

        // Act
        var result = await workflowJob.ExecuteAsync(jobId, jsonData);

        // Assert
        result.Should().NotBeNull();
        result.WorkflowInstanceId.Should().Be(workflowInstanceId);
        result.Status.Should().Be(WorkflowStatus.Terminated);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Handle_Invalid_Json()
    {
        // Arrange
        var jobId = "hangfire-job-123";
        var invalidJson = "invalid json {";

        var workflowJob = new WorkflowJob<TestWorkflow, TestData>(_workflowHost, _storageBridge, _workflowInstanceProvider, _logger);

        // Act
        var result = await workflowJob.ExecuteAsync(jobId, invalidJson);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(WorkflowStatus.Terminated);
        result.ErrorMessage.Should().Contain("JSON");

        // Should not start workflow with invalid data
        await _workflowHost.DidNotReceive().StartWorkflow(typeof(TestWorkflow).Name, Arg.Any<TestData>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_Handle_Timeout()
    {
        // Arrange
        var jobId = "hangfire-job-123";
        var jsonData = """{"name": "Test"}""";
        var workflowInstanceId = "workflow-instance-456";
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(10)).Token;

        _workflowHost.StartWorkflow(typeof(TestWorkflow).Name, Arg.Any<TestData>())
            .Returns(Task.FromResult(workflowInstanceId));

        // Simulate long-running workflow that never completes
        var runningInstance = new WorkflowInstance
        {
            Id = workflowInstanceId,
            Status = WorkflowStatus.Runnable,
            CreateTime = DateTime.UtcNow
        };

        _workflowInstanceProvider.GetWorkflowInstanceAsync(workflowInstanceId)
            .Returns(Task.FromResult<WorkflowInstance?>(runningInstance));

        var workflowJob = new WorkflowJob<TestWorkflow, TestData>(_workflowHost, _storageBridge, _workflowInstanceProvider, _logger);

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => workflowJob.ExecuteAsync(jobId, jsonData, cancellationToken));
        Assert.True(exception is OperationCanceledException);
    }

    [Fact]
    public void Properties_Should_Be_Set_Correctly()
    {
        // Arrange
        var workflowJob = new WorkflowJob<TestWorkflow, TestData>(_workflowHost, _storageBridge, _workflowInstanceProvider, _logger);

        // Act & Assert
        workflowJob.WorkflowInstanceId.Should().BeNull(); // Not set until execution
        workflowJob.JobId.Should().BeNull(); // Not set until execution
    }
}

// Test workflow and data classes
public class TestWorkflow : IWorkflow<TestData>
{
    public string Id => "TestWorkflow";
    public int Version => 1;

    public void Build(IWorkflowBuilder<TestData> builder)
    {
        builder.StartWith(context => ExecutionResult.Next());
    }
}

public class TestData
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}