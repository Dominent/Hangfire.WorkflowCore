using FluentAssertions;
using Hangfire.WorkflowCore.Abstractions;
using Hangfire.WorkflowCore.Dashboard.Abstractions;
using Hangfire.WorkflowCore.Dashboard.Extensions;
using Hangfire.WorkflowCore.Dashboard.Models;
using Hangfire.WorkflowCore.Dashboard.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Hangfire.WorkflowCore.Dashboard.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddWorkflowDashboard_Should_Register_DefaultServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add required dependencies as mocks
        services.AddSingleton(Substitute.For<IWorkflowStorageBridge>());
        services.AddSingleton(Substitute.For<IWorkflowInstanceProvider>());
        services.AddSingleton(Substitute.For<ILoggerFactory>());
        services.AddSingleton(typeof(ILogger<>), typeof(TestLogger<>));

        // Act
        services.AddWorkflowDashboard();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetService<IWorkflowDataProvider>().Should().NotBeNull();
        serviceProvider.GetService<IWorkflowDataProvider>().Should().BeOfType<WorkflowDataProvider>();

        serviceProvider.GetService<IWorkflowStatusCalculator>().Should().NotBeNull();
        serviceProvider.GetService<IWorkflowStatusCalculator>().Should().BeOfType<WorkflowStatusCalculator>();

        serviceProvider.GetService<IWorkflowRenderer>().Should().NotBeNull();
        serviceProvider.GetService<IWorkflowRenderer>().Should().BeOfType<HtmlWorkflowRenderer>();

        serviceProvider.GetService<IWorkflowDashboardService>().Should().NotBeNull();
        serviceProvider.GetService<IWorkflowDashboardService>().Should().BeOfType<WorkflowDashboardService>();
    }

    [Fact]
    public void AddWorkflowDashboard_Should_Register_CustomServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add required dependencies as mocks
        services.AddSingleton(Substitute.For<IWorkflowStorageBridge>());
        services.AddSingleton(Substitute.For<IWorkflowInstanceProvider>());
        services.AddSingleton(Substitute.For<ILoggerFactory>());
        services.AddSingleton(typeof(ILogger<>), typeof(TestLogger<>));

        // Act
        services.AddWorkflowDashboard(options =>
        {
            options.UseRenderer<CustomRenderer>();
            options.UseStatusCalculator<CustomStatusCalculator>();
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetService<IWorkflowRenderer>().Should().BeOfType<CustomRenderer>();
        serviceProvider.GetService<IWorkflowStatusCalculator>().Should().BeOfType<CustomStatusCalculator>();

        // Default services should still be registered
        serviceProvider.GetService<IWorkflowDataProvider>().Should().BeOfType<WorkflowDataProvider>();
        serviceProvider.GetService<IWorkflowDashboardService>().Should().BeOfType<WorkflowDashboardService>();
    }

    [Fact]
    public void AddWorkflowDashboard_Should_AllowMultipleRegistrations()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add required dependencies as mocks
        services.AddSingleton(Substitute.For<IWorkflowStorageBridge>());
        services.AddSingleton(Substitute.For<IWorkflowInstanceProvider>());
        services.AddSingleton(Substitute.For<ILoggerFactory>());
        services.AddSingleton(typeof(ILogger<>), typeof(TestLogger<>));

        // Act - register twice with different configurations
        services.AddWorkflowDashboard();
        services.AddWorkflowDashboard(options =>
        {
            options.UseRenderer<CustomRenderer>();
        });

        // Assert - last registration should win for configured services
        var serviceProvider = services.BuildServiceProvider();
        serviceProvider.GetService<IWorkflowRenderer>().Should().BeOfType<CustomRenderer>();
    }

    [Fact]
    public void WorkflowDashboardOptions_Should_SupportFluentConfiguration()
    {
        // Arrange
        var options = new WorkflowDashboardOptions();

        // Act
        var result = options
            .UseRenderer<CustomRenderer>()
            .UseStatusCalculator<CustomStatusCalculator>();

        // Assert
        result.Should().BeSameAs(options);
        options.RendererType.Should().Be(typeof(CustomRenderer));
        options.StatusCalculatorType.Should().Be(typeof(CustomStatusCalculator));
        options.DataProviderType.Should().BeNull(); // Not configured
    }
}

// Test implementations for custom services
public class CustomRenderer : IWorkflowRenderer
{
    public string Render(WorkflowDashboardData? workflowData)
    {
        return "<div>Custom renderer</div>";
    }
}

public class CustomStatusCalculator : IWorkflowStatusCalculator
{
    public WorkflowStatusInfo CalculateStatus(WorkflowDashboardData workflowData)
    {
        return new WorkflowStatusInfo
        {
            Status = workflowData.Status,
            ProgressPercentage = 50
        };
    }
}

public class TestLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}