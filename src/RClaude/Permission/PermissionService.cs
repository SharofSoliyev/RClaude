using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

namespace RClaude.Permission;

/// <summary>
/// Local HTTP server that receives permission requests from the Claude CLI hook script
/// and waits for user decisions via Telegram buttons.
/// </summary>
public class PermissionService : IDisposable
{
    private readonly ILogger<PermissionService> _logger;
    private readonly HttpListener _listener;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _pending = new();
    private readonly ConcurrentDictionary<string, string> _messageHtml = new();
    private CancellationTokenSource? _cts;

    private static readonly TimeSpan PermissionTimeout = TimeSpan.FromSeconds(120);

    public int Port { get; }

    /// <summary>
    /// Fired when a permission request arrives from the hook script.
    /// The handler should send Telegram buttons and call Respond() when the user decides.
    /// </summary>
    public event Func<PermissionRequest, Task>? OnPermissionRequested;

    public PermissionService(ILogger<PermissionService> logger)
    {
        _logger = logger;
        Port = FindAvailablePort();
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{Port}/");
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();
        _listener.Start();
        _ = ListenLoop(_cts.Token);
        _logger.LogInformation("PermissionService started on port {Port}", Port);
    }

    /// <summary>
    /// Called by the Telegram callback handler when user clicks Allow/Deny.
    /// </summary>
    public void Respond(string requestId, bool allow)
    {
        if (_pending.TryRemove(requestId, out var tcs))
        {
            tcs.TrySetResult(allow);
            _logger.LogInformation("Permission {RequestId}: {Decision}", requestId, allow ? "allowed" : "denied");
        }
        _messageHtml.TryRemove(requestId, out _);
    }

    /// <summary>
    /// Store the original HTML message for a permission request so callback can reuse it.
    /// </summary>
    public void StoreMessageHtml(string requestId, string html) => _messageHtml[requestId] = html;

    /// <summary>
    /// Retrieve and remove the stored HTML for a permission request.
    /// </summary>
    public string? GetMessageHtml(string requestId)
    {
        _messageHtml.TryRemove(requestId, out var html);
        return html;
    }

    private async Task ListenLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _listener.IsListening)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = HandleRequest(context);
            }
            catch (ObjectDisposedException) { break; }
            catch (HttpListenerException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PermissionService listener error");
            }
        }
    }

    private async Task HandleRequest(HttpListenerContext context)
    {
        var response = context.Response;

        try
        {
            if (context.Request.HttpMethod != "POST" || context.Request.Url?.AbsolutePath != "/permission")
            {
                response.StatusCode = 404;
                response.Close();
                return;
            }

            using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
            var body = await reader.ReadToEndAsync();

            var request = JsonSerializer.Deserialize<PermissionRequest>(body);
            if (request == null || string.IsNullOrEmpty(request.RequestId))
            {
                response.StatusCode = 400;
                response.Close();
                return;
            }

            _logger.LogInformation("Permission request: {Tool} from user {UserId} (id: {RequestId})",
                request.ToolName, request.UserId, request.RequestId);

            // Register a pending decision
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pending[request.RequestId] = tcs;

            // Fire event to send Telegram buttons
            if (OnPermissionRequested != null)
            {
                try { await OnPermissionRequested(request); }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in OnPermissionRequested handler");
                }
            }

            // Wait for user decision (with timeout)
            using var cts = new CancellationTokenSource(PermissionTimeout);
            bool allowed;

            try
            {
                var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(PermissionTimeout, cts.Token));
                allowed = completedTask == tcs.Task && tcs.Task.Result;
            }
            catch
            {
                allowed = false;
            }
            finally
            {
                _pending.TryRemove(request.RequestId, out _);
            }

            // Respond to hook script
            var json = JsonSerializer.Serialize(new PermissionResponse
            {
                Decision = allowed ? "allow" : "deny"
            });

            response.ContentType = "application/json";
            response.StatusCode = 200;
            var buffer = System.Text.Encoding.UTF8.GetBytes(json);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling permission request");
            response.StatusCode = 500;
        }
        finally
        {
            try { response.Close(); } catch { }
        }
    }

    private static int FindAvailablePort()
    {
        var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public void Dispose()
    {
        _cts?.Cancel();
        try { _listener.Stop(); } catch { }
        try { _listener.Close(); } catch { }

        foreach (var tcs in _pending.Values)
            tcs.TrySetResult(false);
        _pending.Clear();
    }
}
