using FluentAssertions;
using Hangfire.WorkflowCore.Abstractions;
using NSubstitute;
using WorkflowCore.Models;

namespace Hangfire.WorkflowCore.Tests.Abstractions;

public class IWorkflowJobTests
{
    [Fact]
    public async Task IWorkflowJob_Should_Execute_With_JobId_And_Data()
    {
        // Arrange
        var workflowJob = Substitute.For<IWorkflowJob>();
        var jobId = "hangfire-job-123";
        var jsonData = """{"name": "test", "value": 42}""";
        var cancellationToken = CancellationToken.None;

        var expectedResult = new WorkflowExecutionResult
        {
            WorkflowInstanceId = "workflow-instance-456",
            Status = WorkflowStatus.Complete,
            CompletedAt = DateTime.UtcNow
        };

        workflowJob.ExecuteAsync(jobId, jsonData, cancellationToken)
            .Returns(Task.FromResult(expectedResult));

        // Act
        var result = await workflowJob.ExecuteAsync(jobId, jsonData, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.WorkflowInstanceId.Should().Be("workflow-instance-456");
        result.Status.Should().Be(WorkflowStatus.Complete);
    }

    [Fact]
    public void IWorkflowJob_Should_Have_WorkflowInstanceId_Property()
    {
        // Arrange
        var workflowJob = Substitute.For<IWorkflowJob>();

        // Act
        workflowJob.WorkflowInstanceId.Returns("workflow-instance-123");

        // Assert
        workflowJob.WorkflowInstanceId.Should().Be("workflow-instance-123");
    }

    [Fact]
    public void IWorkflowJob_Should_Have_JobId_Property()
    {
        // Arrange
        var workflowJob = Substitute.For<IWorkflowJob>();

        // Act
        workflowJob.JobId.Returns("hangfire-job-456");

        // Assert
        workflowJob.JobId.Should().Be("hangfire-job-456");
    }

    [Fact]
    public void WorkflowExecutionResult_Should_Use_WorkflowCore_Status()
    {
        // Arrange & Act
        var result = new WorkflowExecutionResult
        {
            WorkflowInstanceId = "test-instance",
            Status = WorkflowStatus.Complete, // Using WorkflowCore enum
            Data = new { Result = "Success" },
            CompletedAt = DateTime.UtcNow
        };

        // Assert
        result.Status.Should().Be(WorkflowStatus.Complete);
        result.Data.Should().NotBeNull();
    }
}