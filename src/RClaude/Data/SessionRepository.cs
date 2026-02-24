using Microsoft.EntityFrameworkCore;
using RClaude.Data.Entities;

namespace RClaude.Data;

public class SessionRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public SessionRepository(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<SessionEntity?> GetActiveSession(long userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Sessions
            .FirstOrDefaultAsync(s => s.TelegramUserId == userId && s.IsActive);
    }

    public async Task<List<SessionEntity>> GetAllSessions(long userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Sessions
            .Where(s => s.TelegramUserId == userId)
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync();
    }

    public async Task<SessionEntity> CreateSession(long userId, string name)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        // Deactivate current active session
        var active = await db.Sessions
            .Where(s => s.TelegramUserId == userId && s.IsActive)
            .ToListAsync();
        foreach (var s in active)
            s.IsActive = false;

        var session = new SessionEntity
        {
            TelegramUserId = userId,
            Name = name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Sessions.Add(session);
        await db.SaveChangesAsync();
        return session;
    }

    public async Task SwitchToSession(long userId, int sessionId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var sessions = await db.Sessions
            .Where(s => s.TelegramUserId == userId)
            .ToListAsync();

        foreach (var s in sessions)
        {
            s.IsActive = s.Id == sessionId;
            if (s.IsActive)
                s.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
    }

    public async Task UpdateSession(SessionEntity session)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        session.UpdatedAt = DateTime.UtcNow;
        db.Sessions.Update(session);
        await db.SaveChangesAsync();
    }

    public async Task UpdateSessionFields(int sessionId, Action<SessionEntity> update)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var session = await db.Sessions.FindAsync(sessionId);
        if (session == null) return;

        update(session);
        session.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task DeleteSession(int sessionId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var session = await db.Sessions.FindAsync(sessionId);
        if (session == null) return;

        var userId = session.TelegramUserId;
        db.Sessions.Remove(session);
        await db.SaveChangesAsync();

        // If deleted was active, activate the most recent one
        if (session.IsActive)
        {
            var next = await db.Sessions
                .Where(s => s.TelegramUserId == userId)
                .OrderByDescending(s => s.UpdatedAt)
                .FirstOrDefaultAsync();

            if (next != null)
            {
                next.IsActive = true;
                await db.SaveChangesAsync();
            }
        }
    }

    public async Task<SessionEntity> EnsureDefaultSession(long userId)
    {
        var active = await GetActiveSession(userId);
        if (active != null) return active;

        return await CreateSession(userId, "Default");
    }
}
