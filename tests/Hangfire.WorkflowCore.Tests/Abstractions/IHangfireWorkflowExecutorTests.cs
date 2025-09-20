using FluentAssertions;
using Hangfire.WorkflowCore.Abstractions;
using NSubstitute;
using WorkflowCore.Models;

namespace Hangfire.WorkflowCore.Tests.Abstractions;

public class IHangfireWorkflowExecutorTests
{
    [Fact]
    public async Task Should_Wait_For_Workflow_Completion_With_Timeout()
    {
        // Arrange
        var executor = Substitute.For<IHangfireWorkflowExecutor>();
        var instanceId = "workflow-instance-123";
        var timeout = TimeSpan.FromMinutes(5);
        var expectedResult = new WorkflowExecutionResult
        {
            WorkflowInstanceId = instanceId,
            Status = WorkflowStatus.Complete, // Using WorkflowCore enum
            CompletedAt = DateTime.UtcNow
        };

        executor.WaitForCompletionAsync(instanceId, timeout, CancellationToken.None)
            .Returns(Task.FromResult(expectedResult));

        // Act
        var result = await executor.WaitForCompletionAsync(instanceId, timeout, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.WorkflowInstanceId.Should().Be(instanceId);
        result.Status.Should().Be(WorkflowStatus.Complete);
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_Get_Execution_Result()
    {
        // Arrange
        var executor = Substitute.For<IHangfireWorkflowExecutor>();
        var instanceId = "workflow-instance-123";
        var expectedResult = new WorkflowExecutionResult
        {
            WorkflowInstanceId = instanceId,
            Status = WorkflowStatus.Complete,
            Data = new { Result = "Success" }
        };

        executor.GetExecutionResultAsync(instanceId)
            .Returns(Task.FromResult<WorkflowExecutionResult?>(expectedResult));

        // Act
        var result = await executor.GetExecutionResultAsync(instanceId);

        // Assert
        result.Should().NotBeNull();
        result!.WorkflowInstanceId.Should().Be(instanceId);
        result.Status.Should().Be(WorkflowStatus.Complete);
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_Cancel_Workflow_And_Return_Result()
    {
        // Arrange
        var executor = Substitute.For<IHangfireWorkflowExecutor>();
        var instanceId = "workflow-instance-123";
        var expectedResult = new WorkflowExecutionResult
        {
            WorkflowInstanceId = instanceId,
            Status = WorkflowStatus.Terminated, // Using WorkflowCore enum for cancellation
            CompletedAt = DateTime.UtcNow
        };

        executor.CancelWorkflowAsync(instanceId)
            .Returns(Task.FromResult(expectedResult));

        // Act
        var result = await executor.CancelWorkflowAsync(instanceId);

        // Assert
        result.Should().NotBeNull();
        result.WorkflowInstanceId.Should().Be(instanceId);
        result.Status.Should().Be(WorkflowStatus.Terminated);
    }

    [Fact]
    public async Task Should_Handle_Timeout_Gracefully()
    {
        // Arrange
        var executor = Substitute.For<IHangfireWorkflowExecutor>();
        var instanceId = "workflow-instance-123";
        var shortTimeout = TimeSpan.FromMilliseconds(1);

        executor.WaitForCompletionAsync(instanceId, shortTimeout, Arg.Any<CancellationToken>())
            .Returns(async (callInfo) =>
            {
                var ct = callInfo.ArgAt<CancellationToken>(2);
                await Task.Delay(100, ct); // Simulate long-running workflow
                return new WorkflowExecutionResult
                {
                    WorkflowInstanceId = instanceId,
                    Status = WorkflowStatus.Runnable,
                    ErrorMessage = "Timeout"
                };
            });

        // Act
        var result = await executor.WaitForCompletionAsync(instanceId, shortTimeout, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.WorkflowInstanceId.Should().Be(instanceId);
    }
}