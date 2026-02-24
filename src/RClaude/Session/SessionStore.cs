using System.Collections.Concurrent;
using RClaude.Data;
using RClaude.Data.Entities;

namespace RClaude.Session;

public class SessionStore
{
    private readonly SessionRepository _repo;
    // In-memory locks only — DB handles persistence
    private readonly ConcurrentDictionary<long, SemaphoreSlim> _locks = new();

    public SessionStore(SessionRepository repo)
    {
        _repo = repo;
    }

    public async Task<UserSession> GetOrCreateAsync(long userId)
    {
        var entity = await _repo.EnsureDefaultSession(userId);
        return ToUserSession(userId, entity);
    }

    public async Task SaveSessionAsync(UserSession session)
    {
        await _repo.UpdateSessionFields(session.DbSessionId, entity =>
        {
            entity.WorkingDirectory = session.WorkingDirectory;
            entity.ClaudeSessionId = session.ClaudeSessionId;
            entity.Model = session.Model;
        });
    }

    public async Task ClearSessionAsync(long userId)
    {
        var entity = await _repo.GetActiveSession(userId);
        if (entity != null)
        {
            await _repo.UpdateSessionFields(entity.Id, e => e.ClaudeSessionId = null);
        }
    }

    public SemaphoreSlim GetLock(long userId)
    {
        return _locks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
    }

    private static UserSession ToUserSession(long userId, SessionEntity entity)
    {
        return new UserSession
        {
            DbSessionId = entity.Id,
            SessionName = entity.Name,
            TelegramUserId = userId,
            WorkingDirectory = entity.WorkingDirectory,
            ClaudeSessionId = entity.ClaudeSessionId,
            Model = entity.Model
        };
    }
}
