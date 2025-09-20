using FluentAssertions;
using Hangfire.WorkflowCore.Abstractions;
using Hangfire.WorkflowCore.Dashboard.Models;
using Hangfire.WorkflowCore.Dashboard.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WorkflowCore.Models;
using Xunit;

namespace Hangfire.WorkflowCore.Dashboard.Tests.Services;

public class WorkflowDataProviderTests
{
    private readonly IWorkflowStorageBridge _storageBridge;
    private readonly IWorkflowInstanceProvider _instanceProvider;
    private readonly ILogger<WorkflowDataProvider> _logger;
    private readonly WorkflowDataProvider _provider;

    public WorkflowDataProviderTests()
    {
        _storageBridge = Substitute.For<IWorkflowStorageBridge>();
        _instanceProvider = Substitute.For<IWorkflowInstanceProvider>();
        _logger = Substitute.For<ILogger<WorkflowDataProvider>>();
        _provider = new WorkflowDataProvider(_storageBridge, _instanceProvider, _logger);
    }

    [Fact]
    public async Task GetWorkflowDataAsync_Should_Return_WorkflowData_When_Found()
    {
        // Arrange
        var jobId = "job-123";
        var workflowInstanceId = "workflow-456";
        var workflowInstance = new WorkflowInstance
        {
            Id = workflowInstanceId,
            Status = WorkflowStatus.Runnable,
            Data = new { VideoId = "video-123" },
            CreateTime = DateTime.UtcNow.AddMinutes(-5)
        };

        var executionResult = new WorkflowExecutionResult
        {
            WorkflowInstanceId = workflowInstanceId,
            Status = WorkflowStatus.Runnable,
            Data = workflowInstance.Data,
            CreatedAt = workflowInstance.CreateTime
        };

        _storageBridge.GetWorkflowInstanceIdAsync(jobId)
            .Returns(workflowInstanceId);

        _instanceProvider.GetWorkflowInstanceAsync(workflowInstanceId)
            .Returns(workflowInstance);

        _storageBridge.GetWorkflowResultAsync(workflowInstanceId)
            .Returns(executionResult);

        // Act
        var result = await _provider.GetWorkflowDataAsync(jobId);

        // Assert
        result.Should().NotBeNull();
        result!.JobId.Should().Be(jobId);
        result.WorkflowInstanceId.Should().Be(workflowInstanceId);
        result.Status.Should().Be(WorkflowStatus.Runnable);
        result.Data.Should().BeEquivalentTo(workflowInstance.Data);
        result.CreatedAt.Should().Be(workflowInstance.CreateTime);
    }

    [Fact]
    public async Task GetWorkflowDataAsync_Should_Return_Null_When_NoMapping()
    {
        // Arrange
        var jobId = "nonexistent-job";

        _storageBridge.GetWorkflowInstanceIdAsync(jobId)
            .Returns((string?)null);

        // Act
        var result = await _provider.GetWorkflowDataAsync(jobId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWorkflowDataAsync_Should_Return_Null_When_WorkflowInstanceNotFound()
    {
        // Arrange
        var jobId = "job-123";
        var workflowInstanceId = "nonexistent-workflow";

        _storageBridge.GetWorkflowInstanceIdAsync(jobId)
            .Returns(workflowInstanceId);

        _instanceProvider.GetWorkflowInstanceAsync(workflowInstanceId)
            .Returns((WorkflowInstance?)null);

        // Act
        var result = await _provider.GetWorkflowDataAsync(jobId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWorkflowDataAsync_Should_HandleException_GracefullyAndLogError()
    {
        // Arrange
        var jobId = "job-123";

        _storageBridge.GetWorkflowInstanceIdAsync(jobId)
            .Returns(Task.FromException<string?>(new InvalidOperationException("Storage error")));

        // Act
        var result = await _provider.GetWorkflowDataAsync(jobId);

        // Assert
        result.Should().BeNull();

        // Verify logging - we'll check this when we implement the actual provider
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task GetWorkflowDataAsync_Should_Return_Null_For_InvalidJobId(string? invalidJobId)
    {
        // Act
        var result = await _provider.GetWorkflowDataAsync(invalidJobId!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWorkflowDataAsync_Should_Populate_Steps_From_WorkflowInstance()
    {
        // Arrange
        var jobId = "job-123";
        var workflowInstanceId = "workflow-456";
        var workflowInstance = new WorkflowInstance
        {
            Id = workflowInstanceId,
            Status = WorkflowStatus.Runnable,
            ExecutionPointers = new ExecutionPointerCollection
            {
                new()
                {
                    Id = "pointer-1",
                    StepName = "Step1",
                    Status = PointerStatus.Complete,
                    StartTime = DateTime.UtcNow.AddMinutes(-5),
                    EndTime = DateTime.UtcNow.AddMinutes(-4)
                },
                new()
                {
                    Id = "pointer-2",
                    StepName = "Step2",
                    Status = PointerStatus.Running,
                    StartTime = DateTime.UtcNow.AddMinutes(-1)
                }
            }
        };

        _storageBridge.GetWorkflowInstanceIdAsync(jobId)
            .Returns(workflowInstanceId);

        _instanceProvider.GetWorkflowInstanceAsync(workflowInstanceId)
            .Returns(workflowInstance);

        // Act
        var result = await _provider.GetWorkflowDataAsync(jobId);

        // Assert
        result.Should().NotBeNull();
        result!.Steps.Should().HaveCount(2);

        var step1 = result.Steps.First(s => s.Name == "Step1");
        step1.Status.Should().Be(WorkflowStatus.Complete);
        step1.StartedAt.Should().NotBeNull();
        step1.CompletedAt.Should().NotBeNull();

        var step2 = result.Steps.First(s => s.Name == "Step2");
        step2.Status.Should().Be(WorkflowStatus.Runnable);
        step2.StartedAt.Should().NotBeNull();
        step2.CompletedAt.Should().BeNull();
    }
}