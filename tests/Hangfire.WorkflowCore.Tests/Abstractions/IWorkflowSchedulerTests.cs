using FluentAssertions;
using Hangfire.WorkflowCore.Abstractions;
using NSubstitute;
using System.Linq.Expressions;

namespace Hangfire.WorkflowCore.Tests.Abstractions;

public class IWorkflowSchedulerTests
{
    [Fact]
    public void Should_Schedule_Workflow_Immediately()
    {
        // Arrange
        var scheduler = Substitute.For<IWorkflowScheduler>();
        var workflowId = "test-workflow";
        var data = new { Name = "Test" };
        var expectedJobId = "job-123";

        scheduler.EnqueueWorkflow(workflowId, data, null)
            .Returns(expectedJobId);

        // Act
        var result = scheduler.EnqueueWorkflow(workflowId, data, null);

        // Assert
        result.Should().Be(expectedJobId);
    }

    [Fact]
    public void Should_Schedule_Workflow_With_Delay()
    {
        // Arrange
        var scheduler = Substitute.For<IWorkflowScheduler>();
        var workflowId = "test-workflow";
        var data = new { Name = "Test" };
        var delay = TimeSpan.FromMinutes(5);
        var expectedJobId = "job-123";

        scheduler.ScheduleWorkflow(workflowId, data, delay, null)
            .Returns(expectedJobId);

        // Act
        var result = scheduler.ScheduleWorkflow(workflowId, data, delay, null);

        // Assert
        result.Should().Be(expectedJobId);
    }

    [Fact]
    public void Should_Schedule_Recurring_Workflow()
    {
        // Arrange
        var scheduler = Substitute.For<IWorkflowScheduler>();
        var recurringJobId = "daily-workflow";
        var workflowId = "test-workflow";
        var data = new { Name = "Test" };
        var cronExpression = "0 2 * * *"; // Daily at 2 AM

        scheduler.AddOrUpdateRecurringWorkflow(recurringJobId, workflowId, data, cronExpression, null);

        // Act & Assert
        scheduler.Received(1).AddOrUpdateRecurringWorkflow(recurringJobId, workflowId, data, cronExpression, null);
    }

    [Fact]
    public void Should_Continue_Workflow_After_Job()
    {
        // Arrange
        var scheduler = Substitute.For<IWorkflowScheduler>();
        var parentJobId = "parent-job-123";
        var workflowId = "continuation-workflow";
        var data = new { Name = "Continuation" };
        var expectedJobId = "continuation-job-456";

        scheduler.ContinueWorkflowWith(parentJobId, workflowId, data, null)
            .Returns(expectedJobId);

        // Act
        var result = scheduler.ContinueWorkflowWith(parentJobId, workflowId, data, null);

        // Assert
        result.Should().Be(expectedJobId);
    }

    [Fact]
    public async Task Should_Create_Workflow_Batch()
    {
        // Arrange
        var scheduler = Substitute.For<IWorkflowScheduler>();
        var batch = Substitute.For<IWorkflowBatch>();
        var expectedBatchId = "batch-789";

        scheduler.CreateBatch().Returns(batch);
        batch.EnqueueAsync().Returns(Task.FromResult(expectedBatchId));

        // Act
        var createdBatch = scheduler.CreateBatch();
        var result = await createdBatch.EnqueueAsync();

        // Assert
        result.Should().Be(expectedBatchId);
    }
}