using Microsoft.Data.Sqlite;

namespace AgentBrowser.Api.Persistence;

/// <summary>
/// A single API operation record.
/// </summary>
public sealed record OperationRecord(
    DateTimeOffset Timestamp,
    string Method,
    string Path,
    string? SessionId,
    int StatusCode,
    long DurationMs,
    string? TraceId);

/// <summary>
/// Optional SQLite-backed operation audit store. Enabled by default only when AGENTBROWSER_PERSISTENCE is set.
/// </summary>
public sealed class OperationStore
{
    private readonly string? _connectionString;
    private readonly ILogger<OperationStore> _logger;

    /// <summary>
    /// Creates the operation store.
    /// </summary>
    public OperationStore(ILogger<OperationStore> logger)
    {
        _logger = logger;

        var mode = Environment.GetEnvironmentVariable("AGENTBROWSER_PERSISTENCE")?.Trim().ToLowerInvariant();
        Enabled = mode is "1" or "true" or "sqlite";
        if (!Enabled)
        {
            return;
        }

        var path = Environment.GetEnvironmentVariable("AGENTBROWSER_SQLITE_PATH");
        if (string.IsNullOrWhiteSpace(path))
        {
            path = Path.Combine(AppContext.BaseDirectory, "agentbrowser-operations.db");
        }

        _connectionString = new SqliteConnectionStringBuilder { DataSource = path }.ToString();
        EnsureCreated();
    }

    /// <summary>
    /// Whether persistence is enabled.
    /// </summary>
    public bool Enabled { get; }

    /// <summary>
    /// Writes an operation record.
    /// </summary>
    public async Task RecordAsync(OperationRecord record, CancellationToken cancellationToken = default)
    {
        if (!Enabled)
        {
            return;
        }

        try
        {
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO operations
                    (timestamp, method, path, session_id, status_code, duration_ms, trace_id)
                VALUES
                    ($timestamp, $method, $path, $sessionId, $statusCode, $durationMs, $traceId);
                """;
            command.Parameters.AddWithValue("$timestamp", record.Timestamp.ToString("O"));
            command.Parameters.AddWithValue("$method", record.Method);
            command.Parameters.AddWithValue("$path", record.Path);
            command.Parameters.AddWithValue("$sessionId", (object?)record.SessionId ?? DBNull.Value);
            command.Parameters.AddWithValue("$statusCode", record.StatusCode);
            command.Parameters.AddWithValue("$durationMs", record.DurationMs);
            command.Parameters.AddWithValue("$traceId", (object?)record.TraceId ?? DBNull.Value);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist operation record for {Method} {Path}", record.Method, record.Path);
        }
    }

    private void EnsureCreated()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS operations (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                timestamp TEXT NOT NULL,
                method TEXT NOT NULL,
                path TEXT NOT NULL,
                session_id TEXT NULL,
                status_code INTEGER NOT NULL,
                duration_ms INTEGER NOT NULL,
                trace_id TEXT NULL
            );
            CREATE INDEX IF NOT EXISTS idx_operations_method_path
                ON operations(method, path);
            CREATE INDEX IF NOT EXISTS idx_operations_session_id
                ON operations(session_id);
            """;
        command.ExecuteNonQuery();
    }
}
