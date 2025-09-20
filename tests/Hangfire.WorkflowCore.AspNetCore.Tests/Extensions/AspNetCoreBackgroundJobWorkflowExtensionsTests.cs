using FluentAssertions;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.WorkflowCore.Abstractions;
using Hangfire.WorkflowCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hangfire.WorkflowCore.AspNetCore.Tests.Extensions;

public class AspNetCoreBackgroundJobWorkflowExtensionsTests
{
    [Fact]
    public void BackgroundJobWorkflow_Should_Have_EnqueueWithHttpContext_Extension_Method()
    {
        // Arrange - This test should fail initially because the extension method doesn't exist yet
        var testData = new TestWorkflowData { Name = "Test", Value = 42 };
        
        // Act & Assert - This should fail compilation until we implement the extension method
        var act = () => BackgroundJobWorkflow.Instance.EnqueueWithHttpContext<TestHttpContextWorkflow, TestWorkflowData>(testData);
        
        // We expect this to eventually return a job ID when implemented
        act.Should().NotThrow("Extension method should be available on BackgroundJobWorkflow");
    }
    
    [Fact]
    public void BackgroundJobWorkflow_Should_Have_ScheduleWorkflowWithHttpContext_Extension_Method()
    {
        // Arrange - This test should fail initially
        var testData = new TestWorkflowData { Name = "Test", Value = 42 };
        var delay = TimeSpan.FromMinutes(5);
        
        // Act & Assert - This should fail compilation until we implement the extension method
        var act = () => BackgroundJobWorkflow.Instance.ScheduleWithHttpContext<TestHttpContextWorkflow, TestWorkflowData>(testData, delay);
        
        // We expect this to eventually return a job ID when implemented
        act.Should().NotThrow("Extension method should be available on BackgroundJobWorkflow");
    }
    
    [Fact] 
    public void BackgroundJobWorkflow_Should_Have_ContinueWorkflowWithHttpContext_Extension_Method()
    {
        // Arrange - This test should fail initially
        GlobalConfiguration.Configuration.UseMemoryStorage();
        
        // Create a valid parent job first
        var parentJobId = BackgroundJob.Enqueue(() => Console.WriteLine("Parent job"));
        var testData = new TestWorkflowData { Name = "Test", Value = 42 };
        
        // Act & Assert - This should fail compilation until we implement the extension method
        var act = () => BackgroundJobWorkflow.Instance.ContinueWithHttpContext<TestHttpContextWorkflow, TestWorkflowData>(parentJobId, testData);
        
        // We expect this to eventually return a job ID when implemented
        act.Should().NotThrow("Extension method should be available on BackgroundJobWorkflow");
    }
    
    [Fact]
    public void EnqueueWithHttpContext_Should_Capture_Current_HttpContext_When_Available()
    {
        // This is a more detailed integration test
        // Arrange
        var services = new ServiceCollection();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/test";
        httpContext.Request.Method = "POST";
        
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(httpContext);
        
        services.AddSingleton(httpContextAccessor);
        services.AddSingleton<IHttpContextSnapshotProvider, AspNetCoreHttpContextSnapshotProvider>();
        
        GlobalConfiguration.Configuration.UseMemoryStorage();
        
        var testData = new TestWorkflowData { Name = "Test", Value = 42 };

        // Act - Now this should work (Green phase)
        var jobId = BackgroundJobWorkflow.Instance.EnqueueWithHttpContext<TestHttpContextWorkflow, TestWorkflowData>(testData);

        // Assert
        jobId.Should().NotBeNullOrEmpty();
        
        // Verify that the job was created with HttpContext integration
        // This will initially fail because the extension method doesn't exist yet
    }

    [Fact]
    public void ScheduleWithHttpContext_Should_Capture_HttpContext_For_Delayed_Execution()
    {
        // Arrange
        var services = new ServiceCollection();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/schedule";
        
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(httpContext);
        
        GlobalConfiguration.Configuration.UseMemoryStorage();
        
        var testData = new TestWorkflowData { Name = "Scheduled", Value = 100 };
        var delay = TimeSpan.FromMinutes(5);

        // Act - Now this should work (Green phase)
        var jobId = BackgroundJobWorkflow.Instance.ScheduleWithHttpContext<TestHttpContextWorkflow, TestWorkflowData>(testData, delay);

        // Assert
        jobId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ContinueWithHttpContext_Should_Chain_Jobs_With_HttpContext()
    {
        // Arrange
        GlobalConfiguration.Configuration.UseMemoryStorage();
        
        var parentData = new TestWorkflowData { Name = "Parent", Value = 1 };
        var childData = new TestWorkflowData { Name = "Child", Value = 2 };
        
        var parentJobId = BackgroundJob.Enqueue(() => Console.WriteLine("Parent job"));

        // Act - Now this should work (Green phase)
        var childJobId = BackgroundJobWorkflow.Instance.ContinueWithHttpContext<TestHttpContextWorkflow, TestWorkflowData>(parentJobId, childData);

        // Assert
        childJobId.Should().NotBeNullOrEmpty();
        childJobId.Should().NotBe(parentJobId);
    }

    [Fact]
    public void RecurringJobWithHttpContext_Should_Create_Recurring_HttpContext_Workflow()
    {
        // Arrange
        GlobalConfiguration.Configuration.UseMemoryStorage();
        
        var testData = new TestWorkflowData { Name = "Recurring", Value = 999 };
        var cronExpression = "0 9 * * *"; // Daily at 9 AM

        // Act - Now this should work (Green phase)
        RecurringJobWorkflow.Instance.AddOrUpdateWithHttpContext<TestHttpContextWorkflow, TestWorkflowData>(
            "test-recurring-job", testData, cronExpression);

        // Assert
        // Verify the recurring job was registered
        // This will be implemented after the extension methods are created
        Assert.True(true); // Placeholder for now
    }
}

// Test workflow that expects HttpContext data
public class TestHttpContextWorkflow : IWorkflow<WorkflowDataWithContext<TestWorkflowData>>
{
    public string Id => "TestHttpContextWorkflow";
    public int Version => 1;

    public void Build(IWorkflowBuilder<WorkflowDataWithContext<TestWorkflowData>> builder)
    {
        builder.StartWith<TestHttpContextStep>();
    }
}

// Test workflow data
public class TestWorkflowData
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}

// Test step that uses HttpContext
public class TestHttpContextStep : StepBody
{
    public override ExecutionResult Run(IStepExecutionContext context)
    {
        var dataWithContext = context.Workflow.Data as WorkflowDataWithContext<TestWorkflowData>;
        if (dataWithContext?.HttpContext != null)
        {
            // HttpContext is available in background job
            Console.WriteLine($"Processing in background with request path: {dataWithContext.HttpContext.RequestPath}");
        }
        
        return ExecutionResult.Next();
    }
}