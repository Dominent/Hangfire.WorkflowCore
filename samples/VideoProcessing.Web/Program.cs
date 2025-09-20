using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.WorkflowCore.Abstractions;
using Hangfire.WorkflowCore.AspNetCore;
using Hangfire.WorkflowCore.Extensions;
using VideoProcessing.Core.Services;
using VideoProcessing.Core.Workflows;
using VideoProcessing.Core.Models;
using WorkflowCore.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HttpContext accessor for ASP.NET Core integration
builder.Services.AddHttpContextAccessor();

// Add complete Hangfire.WorkflowCore with ASP.NET Core integration (one-liner setup!)
builder.Services.AddHangfireWorkflowCoreAspNetCore(
    // Configure Hangfire
    hangfireConfig => 
    {
        hangfireConfig.UseMemoryStorage();
        hangfireConfig.SetDataCompatibilityLevel(CompatibilityLevel.Version_180);
        hangfireConfig.UseSimpleAssemblyNameTypeSerializer();
        hangfireConfig.UseRecommendedSerializerSettings();
    },
    
    // Configure WorkflowCore integration components
    workflowOptions =>
    {
        workflowOptions.UseStorageBridge<MockWorkflowStorageBridge>();
        workflowOptions.UseInstanceProvider<MockWorkflowInstanceProvider>();
    });

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

// Add Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

app.UseAuthorization();

app.MapControllers();

// Start WorkflowCore
var workflowHost = app.Services.GetRequiredService<IWorkflowHost>();
workflowHost.RegisterWorkflow<VideoProcessingWorkflow, VideoData>();
workflowHost.RegisterWorkflow<HttpContextVideoProcessingWorkflow, WorkflowDataWithContext<VideoData>>();
await workflowHost.StartAsync(CancellationToken.None);

Console.WriteLine("🌐 Video Processing Web API started!");
Console.WriteLine("📊 Hangfire Dashboard: http://localhost:5000/hangfire");
Console.WriteLine("📖 Swagger Documentation: http://localhost:5000/swagger");

app.Run();