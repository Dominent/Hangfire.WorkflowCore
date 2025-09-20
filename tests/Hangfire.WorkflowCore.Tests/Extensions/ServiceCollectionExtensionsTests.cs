using FluentAssertions;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.WorkflowCore.Abstractions;
using Hangfire.WorkflowCore.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hangfire.WorkflowCore.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHangfireWorkflowCore_Should_Register_Required_Services()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(); // Add logging for WorkflowCore

        // Act
        services.AddHangfireWorkflowCore(
            hangfire => hangfire.UseMemoryStorage(),
            workflow =>
            {
                workflow.UseStorageBridge<MockWorkflowStorageBridge>();
                workflow.UseInstanceProvider<MockWorkflowInstanceProvider>();
            });
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<IWorkflowHost>().Should().NotBeNull();
        serviceProvider.GetService<IWorkflowStorageBridge>().Should().NotBeNull();
        serviceProvider.GetService<IWorkflowInstanceProvider>().Should().NotBeNull();
        serviceProvider.GetService<IGlobalConfiguration>().Should().NotBeNull();
    }

    [Fact]
    public void AddHangfireWorkflowCore_Should_Throw_When_StorageBridge_Not_Configured()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddHangfireWorkflowCore(
                hangfire => { /* UseMemoryStorage not available in test */ },
                workflow => { /* no storage bridge configured */ }));

        exception.Message.Should().Contain("storage bridge is required");
    }

    [Fact]
    public void AddHangfireWorkflowCore_WithOptions_Should_Allow_Custom_Configuration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(); // Add logging for WorkflowCore
        var hangfireConfigCalled = false;

        // Act
        services.AddHangfireWorkflowCore(
            hangfire =>
            {
                hangfireConfigCalled = true;
                hangfire.UseMemoryStorage();
            },
            workflow =>
            {
                workflow.UseStorageBridge<MockWorkflowStorageBridge>();
                workflow.UseInstanceProvider<MockWorkflowInstanceProvider>();
            });

        // Build service provider to trigger the configuration
        var serviceProvider = services.BuildServiceProvider();

        // Trigger Hangfire service resolution to invoke configuration
        var _ = serviceProvider.GetService<IGlobalConfiguration>();

        // Assert
        hangfireConfigCalled.Should().BeTrue();
        serviceProvider.GetService<IWorkflowStorageBridge>()
            .Should().BeOfType<MockWorkflowStorageBridge>();
    }

    [Fact]
    public void AddHangfireWorkflowCore_Should_Configure_Hangfire_And_WorkflowCore()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(); // Add logging for WorkflowCore

        // Act
        services.AddHangfireWorkflowCore(
            hangfire => hangfire.UseMemoryStorage(),
            workflow =>
            {
                workflow.UseStorageBridge<MockWorkflowStorageBridge>();
                workflow.UseInstanceProvider<MockWorkflowInstanceProvider>();
            });
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Hangfire services should be registered
        serviceProvider.GetService<IBackgroundJobClient>().Should().NotBeNull();
        serviceProvider.GetService<IRecurringJobManager>().Should().NotBeNull();

        // Assert - WorkflowCore services should be registered
        serviceProvider.GetService<IWorkflowHost>().Should().NotBeNull();
        serviceProvider.GetService<IWorkflowRegistry>().Should().NotBeNull();
    }
}

// Mock classes for testing
public class MockWorkflowStorageBridge : IWorkflowStorageBridge
{
    public Task StoreJobWorkflowMappingAsync(string jobId, string workflowInstanceId) => Task.CompletedTask;
    public Task<string?> GetWorkflowInstanceIdAsync(string jobId) => Task.FromResult<string?>(null);
    public Task<string?> GetJobIdAsync(string workflowInstanceId) => Task.FromResult<string?>(null);
    public Task StoreWorkflowResultAsync(string workflowInstanceId, WorkflowExecutionResult result) => Task.CompletedTask;
    public Task<WorkflowExecutionResult?> GetWorkflowResultAsync(string workflowInstanceId) => Task.FromResult<WorkflowExecutionResult?>(null);
    public Task<bool> DeleteMappingAsync(string jobId) => Task.FromResult(false);
    public Task<IDictionary<string, string>> GetAllMappingsAsync() => Task.FromResult<IDictionary<string, string>>(new Dictionary<string, string>());
    public Task<int> CleanupOldEntriesAsync(DateTime olderThan) => Task.FromResult(0);
}

public class MockWorkflowInstanceProvider : IWorkflowInstanceProvider
{
    public Task<WorkflowInstance?> GetWorkflowInstanceAsync(string workflowInstanceId) =>
        Task.FromResult<WorkflowInstance?>(null);
}