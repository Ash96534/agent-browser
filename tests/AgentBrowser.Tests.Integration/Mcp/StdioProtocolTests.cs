using System.Diagnostics;
using System.Text.Json;

namespace AgentBrowser.Tests.Integration.Mcp;

[Trait("Category", "Mcp")]
public class StdioProtocolTests
{
    [Fact]
    public async Task Server_ListsBrowserTools_AndKeepsStdoutClean()
    {
        var repoRoot = FindRepositoryRoot();
        var project = Path.Combine(repoRoot, "src", "AgentBrowser.MCP", "AgentBrowser.MCP.csproj");

        var startInfo = new ProcessStartInfo("dotnet", $"run --project \"{project}\" --no-build")
        {
            WorkingDirectory = repoRoot,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(startInfo)!;
        var stdout = process.StandardOutput;
        var stderrTask = process.StandardError.ReadToEndAsync();
        var lines = new List<string>();

        try
        {
            await process.StandardInput.WriteLineAsync(
                """{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-06-18","capabilities":{},"clientInfo":{"name":"stdio-test","version":"1.0"}}}""");
            var initializeLine = await ReadRequiredLineAsync(stdout, TimeSpan.FromSeconds(10), stderrTask);
            lines.Add(initializeLine);
            AssertJsonRpcLine(initializeLine, expectedId: 1);

            await process.StandardInput.WriteLineAsync("""{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}""");
            var toolsLine = await ReadRequiredLineAsync(stdout, TimeSpan.FromSeconds(10), stderrTask);
            lines.Add(toolsLine);
            AssertJsonRpcLine(toolsLine, expectedId: 2);
            Assert.Contains("browse_create_session", toolsLine);
            Assert.Contains("browse_navigate", toolsLine);
        }
        finally
        {
            process.StandardInput.Close();
            try
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await process.WaitForExitAsync(timeout.Token);
            }
            catch (OperationCanceledException)
            {
                process.Kill(entireProcessTree: true);
            }
        }

        var remaining = await stdout.ReadToEndAsync();
        foreach (var line in remaining.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            AssertJsonRpcLine(line, expectedId: null);
            lines.Add(line);
        }

        Assert.NotEmpty(lines);
    }

    private static async Task<string> ReadRequiredLineAsync(
        StreamReader stdout,
        TimeSpan timeout,
        Task<string> stderrTask)
    {
        var readTask = stdout.ReadLineAsync();
        var completed = await Task.WhenAny(readTask, Task.Delay(timeout));
        if (completed != readTask)
        {
            var stderr = await stderrTask;
            throw new TimeoutException($"MCP server did not write a response within {timeout}. stderr:{Environment.NewLine}{stderr}");
        }

        return await readTask ??
            throw new InvalidOperationException("MCP server closed stdout before sending a response.");
    }

    private static void AssertJsonRpcLine(string line, int? expectedId)
    {
        using var document = JsonDocument.Parse(line);
        var root = document.RootElement;
        Assert.Equal("2.0", root.GetProperty("jsonrpc").GetString());
        if (expectedId is not null)
        {
            Assert.Equal(expectedId.Value, root.GetProperty("id").GetInt32());
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AgentBrowser.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ??
            throw new InvalidOperationException("Could not locate AgentBrowser.sln above the test output directory.");
    }
}
