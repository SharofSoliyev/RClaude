using Microsoft.EntityFrameworkCore;
using RClaude.Data.Entities;

namespace RClaude.Data;

/// <summary>
/// Key-value settings stored in DB.
/// Keys: telegram:bot_token, telegram:allowed_usernames, telegram:allowed_user_ids,
///       claude:cli_path, claude:model, claude:max_timeout, agent:default_dir
/// </summary>
public class SettingsRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public SettingsRepository(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<string?> GetAsync(string key)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var entity = await db.Settings.FindAsync(key);
        return entity?.Value;
    }

    public async Task SetAsync(string key, string value)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var entity = await db.Settings.FindAsync(key);
        if (entity != null)
        {
            entity.Value = value;
        }
        else
        {
            db.Settings.Add(new SettingEntity { Key = key, Value = value });
        }
        await db.SaveChangesAsync();
    }

    public async Task<Dictionary<string, string>> GetAllAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);
    }

    public async Task DeleteAsync(string key)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var entity = await db.Settings.FindAsync(key);
        if (entity != null)
        {
            db.Settings.Remove(entity);
            await db.SaveChangesAsync();
        }
    }
}
