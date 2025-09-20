using FluentAssertions;
using Hangfire.Server;
using Hangfire.WorkflowCore.Abstractions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hangfire.WorkflowCore.AspNetCore.Tests;

public class HttpContextWorkflowJobTests
{
    private readonly IWorkflowHost _workflowHost;
    private readonly IWorkflowStorageBridge _storageBridge;
    private readonly IWorkflowInstanceProvider _workflowInstanceProvider;
    private readonly IHttpContextSnapshotProvider _httpContextProvider;
    private readonly ILogger<HttpContextWorkflowJob<HttpContextTestWorkflow, HttpContextTestData>> _logger;

    public HttpContextWorkflowJobTests()
    {
        _workflowHost = Substitute.For<IWorkflowHost>();
        _storageBridge = Substitute.For<IWorkflowStorageBridge>();
        _workflowInstanceProvider = Substitute.For<IWorkflowInstanceProvider>();
        _httpContextProvider = Substitute.For<IHttpContextSnapshotProvider>();
        _logger = Substitute.For<ILogger<HttpContextWorkflowJob<HttpContextTestWorkflow, HttpContextTestData>>>();
    }

    [Fact]
    public async Task WorkflowJob_Should_Include_HttpContext_In_Workflow_Data()
    {
        // Arrange
        var httpContextSnapshot = new HttpContextSnapshot
        {
            RequestPath = "/api/test",
            Method = "POST",
            UserId = "user123"
        };

        _httpContextProvider.GetCurrentSnapshot()
            .Returns(httpContextSnapshot);

        var testData = new HttpContextTestData { Name = "Test", Value = 42 };
        var jsonData = System.Text.Json.JsonSerializer.Serialize(testData);
        var workflowInstanceId = "workflow-instance-456";

        _workflowHost.StartWorkflow(typeof(HttpContextTestWorkflow).Name, Arg.Any<object>())
            .Returns(Task.FromResult(workflowInstanceId));

        var workflowInstance = new WorkflowInstance
        {
            Id = workflowInstanceId,
            Status = WorkflowStatus.Complete,
            CompleteTime = DateTime.UtcNow,
            Data = testData
        };

        _workflowInstanceProvider.GetWorkflowInstanceAsync(workflowInstanceId)
            .Returns(Task.FromResult<WorkflowInstance?>(workflowInstance));

        var workflowJob = new HttpContextWorkflowJob<HttpContextTestWorkflow, HttpContextTestData>(
            _workflowHost, _storageBridge, _workflowInstanceProvider, _httpContextProvider, _logger);

        // Act
        var result = await workflowJob.ExecuteWithHttpContextAsync(null, jsonData);

        // Assert
        result.Should().NotBeNull();

        // Verify that StartWorkflow was called with enhanced data that includes HttpContext
        await _workflowHost.Received(1).StartWorkflow(
            typeof(HttpContextTestWorkflow).Name,
            Arg.Is<WorkflowDataWithContext<HttpContextTestData>>(data =>
                data.Data.Name == "Test" &&
                data.Data.Value == 42 &&
                data.HttpContext != null &&
                data.HttpContext.RequestPath == "/api/test" &&
                data.HttpContext.UserId == "user123"));
    }

    [Fact]
    public async Task WorkflowJob_Should_Handle_Null_HttpContext()
    {
        // Arrange
        _httpContextProvider.GetCurrentSnapshot()
            .Returns((HttpContextSnapshot?)null);

        var testData = new HttpContextTestData { Name = "Test", Value = 42 };
        var jsonData = System.Text.Json.JsonSerializer.Serialize(testData);
        var workflowInstanceId = "workflow-instance-456";

        _workflowHost.StartWorkflow(typeof(HttpContextTestWorkflow).Name, Arg.Any<object>())
            .Returns(Task.FromResult(workflowInstanceId));

        var workflowInstance = new WorkflowInstance
        {
            Id = workflowInstanceId,
            Status = WorkflowStatus.Complete,
            CompleteTime = DateTime.UtcNow,
            Data = testData
        };

        _workflowInstanceProvider.GetWorkflowInstanceAsync(workflowInstanceId)
            .Returns(Task.FromResult<WorkflowInstance?>(workflowInstance));

        var workflowJob = new HttpContextWorkflowJob<HttpContextTestWorkflow, HttpContextTestData>(
            _workflowHost, _storageBridge, _workflowInstanceProvider, _httpContextProvider, _logger);

        // Act
        var result = await workflowJob.ExecuteWithHttpContextAsync(null, jsonData);

        // Assert
        result.Should().NotBeNull();

        // Verify that StartWorkflow was called with enhanced data where HttpContext is null
        await _workflowHost.Received(1).StartWorkflow(
            typeof(HttpContextTestWorkflow).Name,
            Arg.Is<WorkflowDataWithContext<HttpContextTestData>>(data =>
                data.Data.Name == "Test" &&
                data.Data.Value == 42 &&
                data.HttpContext == null));
    }
}

// Test classes for HttpContext tests
public class HttpContextTestWorkflow : IWorkflow<WorkflowDataWithContext<HttpContextTestData>>
{
    public string Id => "HttpContextTestWorkflow";
    public int Version => 1;
    public void Build(IWorkflowBuilder<WorkflowDataWithContext<HttpContextTestData>> builder) => builder.StartWith<HttpContextTestStep>();
}

public class HttpContextTestData
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class HttpContextTestStep : StepBody
{
    public override ExecutionResult Run(IStepExecutionContext context) => ExecutionResult.Next();
}