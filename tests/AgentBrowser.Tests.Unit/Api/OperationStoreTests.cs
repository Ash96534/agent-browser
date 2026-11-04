using AgentBrowser.Api.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgentBrowser.Tests.Unit.Api;

public class OperationStoreTests
{
    [Fact]
    public async Task RecordAsync_WritesOperationWhenEnabled()
    {
        var database = Path.Combine(Directory.GetCurrentDirectory(), $"operation-store-{Guid.NewGuid():N}.db");

        Environment.SetEnvironmentVariable("AGENTBROWSER_PERSISTENCE", "true");
        Environment.SetEnvironmentVariable("AGENTBROWSER_SQLITE_PATH", database);
        try
        {
            var store = new OperationStore(NullLogger<OperationStore>.Instance);
            Assert.True(store.Enabled);

            await store.RecordAsync(new OperationRecord(
                DateTimeOffset.UtcNow, "POST", "/session/abc/navigate", "abc", 200, 123, "trace-1"));

            using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = database }.ToString());
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*), MAX(duration_ms) FROM operations";
            await using var reader = await command.ExecuteReaderAsync();

            Assert.True(await reader.ReadAsync());
            Assert.Equal(1, reader.GetInt64(0));
            Assert.Equal(123, reader.GetInt64(1));
        }
        finally
        {
            Environment.SetEnvironmentVariable("AGENTBROWSER_PERSISTENCE", null);
            Environment.SetEnvironmentVariable("AGENTBROWSER_SQLITE_PATH", null);
            if (File.Exists(database))
            {
                File.Delete(database);
            }
        }
    }
}
