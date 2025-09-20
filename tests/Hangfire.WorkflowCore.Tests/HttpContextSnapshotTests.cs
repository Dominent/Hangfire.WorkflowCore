using FluentAssertions;
using Hangfire.WorkflowCore.Abstractions;

namespace Hangfire.WorkflowCore.Tests;

public class HttpContextSnapshotTests
{
    [Fact]
    public void HttpContextSnapshot_Should_Be_Serializable()
    {
        // Arrange
        var snapshot = new HttpContextSnapshot
        {
            RequestPath = "/api/videos",
            Method = "POST",
            Headers = new Dictionary<string, string> { { "Authorization", "Bearer token" } },
            Claims = new Dictionary<string, string> { { "sub", "user123" } },
            UserId = "user123",
            RequestId = "req-456"
        };

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(snapshot);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<HttpContextSnapshot>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.RequestPath.Should().Be("/api/videos");
        deserialized.Method.Should().Be("POST");
        deserialized.Headers.Should().ContainKey("Authorization");
        deserialized.Claims.Should().ContainKey("sub");
        deserialized.UserId.Should().Be("user123");
        deserialized.RequestId.Should().Be("req-456");
    }

    [Fact]
    public void HttpContextSnapshot_Should_Handle_Null_Values()
    {
        // Arrange
        var snapshot = new HttpContextSnapshot
        {
            RequestPath = null,
            Method = null,
            Headers = new Dictionary<string, string>(),
            Claims = new Dictionary<string, string>(),
            UserId = null,
            RequestId = null
        };

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(snapshot);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<HttpContextSnapshot>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.RequestPath.Should().BeNull();
        deserialized.Method.Should().BeNull();
        deserialized.Headers.Should().BeEmpty();
        deserialized.Claims.Should().BeEmpty();
        deserialized.UserId.Should().BeNull();
        deserialized.RequestId.Should().BeNull();
    }

    [Fact]
    public void HttpContextSnapshot_Should_Be_Creatable_From_Null()
    {
        // Arrange & Act
        var snapshot = HttpContextSnapshot.FromHttpContext(null);

        // Assert
        snapshot.Should().BeNull();
    }
}