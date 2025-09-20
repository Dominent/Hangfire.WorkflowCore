using FluentAssertions;
using Hangfire.MemoryStorage;
using Hangfire.WorkflowCore.Extensions;
using NSubstitute;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hangfire.WorkflowCore.Tests.Extensions;

public class BackgroundJobWorkflowExtensionsTests : IDisposable
{
    public BackgroundJobWorkflowExtensionsTests()
    {
        // Initialize Hangfire with in-memory storage for testing
        GlobalConfiguration.Configuration.UseMemoryStorage();
    }

    public void Dispose()
    {
        // Clean up Hangfire storage after each test
        GlobalConfiguration.Configuration.UseMemoryStorage();
    }

    [Fact]
    public void EnqueueWorkflow_Should_Return_JobId()
    {
        // Arrange
        var data = new TestWorkflowData { Name = "Test", Value = 42 };
        
        // Act
        var jobId = BackgroundJobWorkflow.Enqueue<TestWorkflow, TestWorkflowData>(data);
        
        // Assert
        jobId.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public void ScheduleWorkflow_Should_Return_JobId()
    {
        // Arrange
        var data = new TestWorkflowData { Name = "Test", Value = 42 };
        var delay = TimeSpan.FromMinutes(5);
        
        // Act
        var jobId = BackgroundJobWorkflow.ScheduleWorkflow<TestWorkflow, TestWorkflowData>(data, delay);
        
        // Assert
        jobId.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public void ScheduleWorkflow_WithDateTimeOffset_Should_Return_JobId()
    {
        // Arrange
        var data = new TestWorkflowData { Name = "Test", Value = 42 };
        var scheduleTime = DateTimeOffset.Now.AddHours(1);
        
        // Act
        var jobId = BackgroundJobWorkflow.ScheduleWorkflow<TestWorkflow, TestWorkflowData>(data, scheduleTime);
        
        // Assert
        jobId.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public void ContinueWorkflowWith_Should_Return_JobId()
    {
        // Arrange
        // First create a parent job
        var parentData = new TestWorkflowData { Name = "Parent", Value = 1 };
        var parentJobId = BackgroundJobWorkflow.Enqueue<TestWorkflow, TestWorkflowData>(parentData);
        
        var data = new TestWorkflowData { Name = "Continuation", Value = 99 };
        
        // Act
        var jobId = BackgroundJobWorkflow.ContinueWorkflowWith<TestWorkflow, TestWorkflowData>(parentJobId, data);
        
        // Assert
        jobId.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public void AddOrUpdateRecurringWorkflow_Should_Not_Throw()
    {
        // Arrange
        var recurringJobId = "daily-workflow";
        var data = new TestWorkflowData { Name = "Recurring", Value = 1 };
        var cronExpression = "0 2 * * *"; // Daily at 2 AM
        
        // Act & Assert
        var action = () => RecurringJobWorkflow.AddOrUpdateWorkflow<TestWorkflow, TestWorkflowData>(
            recurringJobId, data, cronExpression);
        
        action.Should().NotThrow();
    }
    
    [Fact]
    public void RemoveRecurringWorkflow_Should_Not_Throw()
    {
        // Arrange
        var recurringJobId = "daily-workflow";
        
        // Act & Assert
        var action = () => RecurringJobWorkflow.RemoveWorkflow(recurringJobId);
        
        action.Should().NotThrow();
    }
    
    [Fact]
    public void TriggerRecurringWorkflow_Should_Not_Throw()
    {
        // Arrange
        var recurringJobId = "daily-workflow";
        
        // Act & Assert
        var action = () => RecurringJobWorkflow.TriggerWorkflow(recurringJobId);
        
        action.Should().NotThrow();
    }
}

// Test workflow and data for the extension tests
public class TestWorkflow : IWorkflow<TestWorkflowData>
{
    public string Id => "TestWorkflow";
    public int Version => 1;
    
    public void Build(IWorkflowBuilder<TestWorkflowData> builder)
    {
        builder.StartWith(context => ExecutionResult.Next());
    }
}

public class TestWorkflowData
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}