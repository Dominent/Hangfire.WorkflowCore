using FluentAssertions;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.WorkflowCore.Abstractions;
using Hangfire.WorkflowCore.AspNetCore;
using Hangfire.WorkflowCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WorkflowCore.Models;

namespace Hangfire.WorkflowCore.AspNetCore.Tests.Extensions;

public class ServiceCollectionExtensionsEnhancedTests
{
    [Fact]
    public void AddAspNetCoreIntegration_Should_Register_HttpContext_Services()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - This should fail initially until we enhance the method
        services.AddAspNetCoreIntegration();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Should register HttpContextAccessor
        var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
        httpContextAccessor.Should().NotBeNull();

        // Should register AspNetCore HttpContext snapshot provider
        var snapshotProvider = serviceProvider.GetService<IHttpContextSnapshotProvider>();
        snapshotProvider.Should().NotBeNull();
        snapshotProvider.Should().BeOfType<AspNetCoreHttpContextSnapshotProvider>();
    }

    [Fact]
    public void AddAspNetCoreIntegration_Should_Register_HttpContext_Job_Types()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - This should fail initially
        services.AddAspNetCoreIntegration();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Should be able to resolve HttpContextWorkflowJob types
        // This will fail initially because we haven't implemented the auto-registration yet
        var jobType = typeof(HttpContextWorkflowJob<,>);
        var registration = services.FirstOrDefault(s => s.ServiceType.IsGenericTypeDefinition &&
                                                       s.ServiceType == jobType);
        registration.Should().NotBeNull("HttpContextWorkflowJob should be registered");
    }

    [Fact]
    public void AddAspNetCoreIntegration_Should_Not_Override_Existing_HttpContextAccessor()
    {
        // Arrange
        var services = new ServiceCollection();
        var existingAccessor = new HttpContextAccessor();
        services.AddSingleton<IHttpContextAccessor>(existingAccessor);

        // Act
        services.AddAspNetCoreIntegration();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var resolvedAccessor = serviceProvider.GetService<IHttpContextAccessor>();
        resolvedAccessor.Should().BeSameAs(existingAccessor);
    }

    [Fact]
    public void AddHangfireWorkflowCoreAspNetCore_Should_Configure_Complete_Setup()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - This method should be a one-liner setup
        services.AddHangfireWorkflowCoreAspNetCore(
            hangfireConfig => hangfireConfig.UseMemoryStorage(),
            workflowOptions =>
            {
                workflowOptions.UseStorageBridge<MockStorageBridge>();
                workflowOptions.UseInstanceProvider<MockInstanceProvider>();
            });

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Should have all the core services
        serviceProvider.GetService<IHttpContextAccessor>().Should().NotBeNull();
        serviceProvider.GetService<IHttpContextSnapshotProvider>().Should().NotBeNull();
        serviceProvider.GetService<IWorkflowStorageBridge>().Should().NotBeNull();
        serviceProvider.GetService<IWorkflowInstanceProvider>().Should().NotBeNull();
    }
}

// Mock implementations for testing
public class MockStorageBridge : IWorkflowStorageBridge
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

public class MockInstanceProvider : IWorkflowInstanceProvider
{
    public Task<WorkflowInstance?> GetWorkflowInstanceAsync(string workflowInstanceId) =>
        Task.FromResult<WorkflowInstance?>(null);
}