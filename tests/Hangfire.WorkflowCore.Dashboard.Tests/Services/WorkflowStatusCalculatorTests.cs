using FluentAssertions;
using Hangfire.WorkflowCore.Dashboard.Models;
using Hangfire.WorkflowCore.Dashboard.Services;
using WorkflowCore.Models;
using Xunit;

namespace Hangfire.WorkflowCore.Dashboard.Tests.Services;

public class WorkflowStatusCalculatorTests
{
    private readonly WorkflowStatusCalculator _calculator = new();

    [Fact]
    public void CalculateStatus_Should_Return_CompletedStatus_For_CompleteWorkflow()
    {
        // Arrange
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            Status = WorkflowStatus.Complete,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            CompletedAt = DateTime.UtcNow,
            Steps = new List<WorkflowStepInfo>
            {
                new() { Status = WorkflowStatus.Complete, Order = 1 },
                new() { Status = WorkflowStatus.Complete, Order = 2 }
            }
        };

        // Act
        var result = _calculator.CalculateStatus(workflowData);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(WorkflowStatus.Complete);
        result.ProgressPercentage.Should().Be(100);
        result.Duration.Should().BeCloseTo(TimeSpan.FromMinutes(10), TimeSpan.FromMilliseconds(100));
        result.IsCompleted.Should().BeTrue();
        result.IsRunning.Should().BeFalse();
        result.HasError.Should().BeFalse();
        result.Performance.CompletedSteps.Should().Be(2);
        result.Performance.TotalSteps.Should().Be(2);
    }

    [Fact]
    public void CalculateStatus_Should_Return_RunningStatus_For_RunnableWorkflow()
    {
        // Arrange
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            Status = WorkflowStatus.Runnable,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            Steps = new List<WorkflowStepInfo>
            {
                new() { Status = WorkflowStatus.Complete, Order = 1 },
                new() { Status = WorkflowStatus.Runnable, Order = 2 },
                new() { Status = WorkflowStatus.Runnable, Order = 3 }
            }
        };

        // Act
        var result = _calculator.CalculateStatus(workflowData);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(WorkflowStatus.Runnable);
        result.ProgressPercentage.Should().Be(33); // 1 out of 3 completed
        result.Duration.Should().BeCloseTo(TimeSpan.FromMinutes(5), TimeSpan.FromMilliseconds(100));
        result.IsCompleted.Should().BeFalse();
        result.IsRunning.Should().BeTrue();
        result.HasError.Should().BeFalse();
        result.Performance.CompletedSteps.Should().Be(1);
        result.Performance.TotalSteps.Should().Be(3);
    }

    [Fact]
    public void CalculateStatus_Should_Return_FailedStatus_For_TerminatedWorkflow()
    {
        // Arrange
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            Status = WorkflowStatus.Terminated,
            ErrorMessage = "Workflow failed due to timeout",
            CreatedAt = DateTime.UtcNow.AddMinutes(-3),
            CompletedAt = DateTime.UtcNow,
            Steps = new List<WorkflowStepInfo>
            {
                new() { Status = WorkflowStatus.Complete, Order = 1 },
                new() { Status = WorkflowStatus.Terminated, Order = 2, ErrorMessage = "Step failed" }
            }
        };

        // Act
        var result = _calculator.CalculateStatus(workflowData);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(WorkflowStatus.Terminated);
        result.Duration.Should().BeCloseTo(TimeSpan.FromMinutes(3), TimeSpan.FromMilliseconds(100));
        result.IsCompleted.Should().BeTrue();
        result.IsRunning.Should().BeFalse();
        result.HasError.Should().BeTrue();
        result.ErrorMessage.Should().Be("Workflow failed due to timeout");
        result.Performance.CompletedSteps.Should().Be(1);
        result.Performance.TotalSteps.Should().Be(2);
    }

    [Fact]
    public void CalculateStatus_Should_Return_SuspendedStatus_For_SuspendedWorkflow()
    {
        // Arrange
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            Status = WorkflowStatus.Suspended,
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            Steps = new List<WorkflowStepInfo>
            {
                new() { Status = WorkflowStatus.Complete, Order = 1 },
                new() { Status = WorkflowStatus.Suspended, Order = 2 }
            }
        };

        // Act
        var result = _calculator.CalculateStatus(workflowData);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(WorkflowStatus.Suspended);
        result.ProgressPercentage.Should().Be(50);
        result.IsCompleted.Should().BeFalse();
        result.IsRunning.Should().BeFalse();
        result.HasError.Should().BeFalse();
    }

    [Fact]
    public void CalculateStatus_Should_Handle_EmptySteps()
    {
        // Arrange
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            Status = WorkflowStatus.Runnable,
            CreatedAt = DateTime.UtcNow.AddMinutes(-2),
            Steps = new List<WorkflowStepInfo>()
        };

        // Act
        var result = _calculator.CalculateStatus(workflowData);

        // Assert
        result.Should().NotBeNull();
        result.ProgressPercentage.Should().Be(0);
        result.Performance.CompletedSteps.Should().Be(0);
        result.Performance.TotalSteps.Should().Be(0);
    }

    [Fact]
    public void CalculateStatus_Should_Calculate_PerformanceMetrics()
    {
        // Arrange
        var baseTime = DateTime.UtcNow.AddMinutes(-10);
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            Status = WorkflowStatus.Runnable,
            CreatedAt = baseTime,
            Steps = new List<WorkflowStepInfo>
            {
                new()
                {
                    Status = WorkflowStatus.Complete,
                    Order = 1,
                    StartedAt = baseTime.AddMinutes(1),
                    CompletedAt = baseTime.AddMinutes(3) // 2 minutes duration
                },
                new()
                {
                    Status = WorkflowStatus.Complete,
                    Order = 2,
                    StartedAt = baseTime.AddMinutes(3),
                    CompletedAt = baseTime.AddMinutes(7) // 4 minutes duration
                },
                new()
                {
                    Status = WorkflowStatus.Runnable,
                    Order = 3,
                    StartedAt = baseTime.AddMinutes(7)
                }
            }
        };

        // Act
        var result = _calculator.CalculateStatus(workflowData);

        // Assert
        result.Should().NotBeNull();
        result.Performance.TotalProcessingTime.Should().BeCloseTo(TimeSpan.FromMinutes(6), TimeSpan.FromMilliseconds(100)); // 2 + 4 minutes
        result.Performance.AverageStepDuration.Should().BeCloseTo(TimeSpan.FromMinutes(3), TimeSpan.FromMilliseconds(100)); // 6 minutes / 2 completed steps
        result.Performance.CompletedSteps.Should().Be(2);
        result.Performance.TotalSteps.Should().Be(3);
    }

    [Fact]
    public void CalculateStatus_Should_EstimateCompletionTime_For_RunningWorkflow()
    {
        // Arrange
        var baseTime = DateTime.UtcNow.AddMinutes(-6);
        var workflowData = new WorkflowDashboardData
        {
            JobId = "job-123",
            Status = WorkflowStatus.Runnable,
            CreatedAt = baseTime,
            Steps = new List<WorkflowStepInfo>
            {
                new()
                {
                    Status = WorkflowStatus.Complete,
                    Order = 1,
                    StartedAt = baseTime.AddMinutes(1),
                    CompletedAt = baseTime.AddMinutes(3) // 2 minutes
                },
                new()
                {
                    Status = WorkflowStatus.Runnable,
                    Order = 2,
                    StartedAt = baseTime.AddMinutes(3)
                },
                new()
                {
                    Status = WorkflowStatus.Runnable,
                    Order = 3
                }
            }
        };

        // Act
        var result = _calculator.CalculateStatus(workflowData);

        // Assert
        result.Should().NotBeNull();
        result.EstimatedCompletionTime.Should().NotBeNull();
        // With 2 remaining steps and 2 minutes average, estimated completion should be ~4 minutes from now
        result.EstimatedCompletionTime.Should().BeAfter(DateTime.UtcNow.AddMinutes(3));
        result.EstimatedCompletionTime.Should().BeBefore(DateTime.UtcNow.AddMinutes(5));
    }
}