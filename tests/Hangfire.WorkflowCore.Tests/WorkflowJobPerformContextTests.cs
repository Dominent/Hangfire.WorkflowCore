using FluentAssertions;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;
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

        // Create a mock BackgroundJob with proper structure
        var backgroundJob = new BackgroundJob("test-job-123", Job.FromExpression(() => Console.WriteLine()), DateTime.UtcNow);
        var cancellationToken = Substitute.For<IJobCancellationToken>();
        var connection = Substitute.For<IStorageConnection>();
        var storage = Substitute.For<JobStorage>();
        _performContext = new PerformContext(storage, connection, backgroundJob, cancellationToken);
    }

    [Fact]
    public async Task WorkflowJob_Should_Extract_JobId_From_PerformContext()
    {
        // Arrange
        var expectedJobId = "test-job-123";
        var data = new TestData { Name = "Test", Value = 42 };
        var jsonData = System.Text.Json.JsonSerializer.Serialize(data);
        var workflowInstanceId = "workflow-instance-456";

        // PerformContext is now a real object with the expected job ID

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
        var result = await workflowJob.ExecuteWithContextAsync(_performContext, jsonData);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(WorkflowStatus.Complete);
        result.WorkflowInstanceId.Should().Be(workflowInstanceId);

        // Verify that the storage bridge was called with the correct job ID from context
        await _storageBridge.Received(1).StoreJobWorkflowMappingAsync(expectedJobId, workflowInstanceId);
    }
}