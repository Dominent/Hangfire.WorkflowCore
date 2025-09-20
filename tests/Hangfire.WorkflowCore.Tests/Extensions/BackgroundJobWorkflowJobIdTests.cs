using FluentAssertions;
using Hangfire.MemoryStorage;
using Hangfire.WorkflowCore.Extensions;
using NSubstitute;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hangfire.WorkflowCore.Tests.Extensions;

public class BackgroundJobWorkflowJobIdTests : IDisposable
{
    public BackgroundJobWorkflowJobIdTests()
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
    public void WorkflowJob_Should_Have_ExecuteWithJobContext_Method()
    {
        // The issue is that Hangfire doesn't automatically inject job IDs into custom parameters.
        // We need to use either PerformContext or create a different method signature.
        // Let's verify that our WorkflowJob has a method that can receive the job context.

        // Arrange
        var workflowJobType = typeof(WorkflowJob<TestWorkflow, TestWorkflowData>);

        // Act
        var executeMethod = workflowJobType.GetMethod("ExecuteAsync");

        // Assert
        executeMethod.Should().NotBeNull();
        var parameters = executeMethod!.GetParameters();

        // For now, this test will fail because we haven't implemented the fix yet
        // The fix will be to modify WorkflowJob to get job ID from PerformContext
        parameters.Should().HaveCount(3); // jobId, data, cancellationToken
    }
}

// Using test classes from other test files