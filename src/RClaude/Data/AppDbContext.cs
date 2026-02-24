using Microsoft.EntityFrameworkCore;
using RClaude.Data.Entities;

namespace RClaude.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<SessionEntity> Sessions => Set<SessionEntity>();
    public DbSet<SettingEntity> Settings => Set<SettingEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SessionEntity>(entity =>
        {
            entity.ToTable("sessions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TelegramUserId).HasColumnName("telegram_user_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
            entity.Property(e => e.WorkingDirectory).HasColumnName("working_directory");
            entity.Property(e => e.ClaudeSessionId).HasColumnName("claude_session_id");
            entity.Property(e => e.Model).HasColumnName("model").HasMaxLength(20).HasDefaultValue("sonnet");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.TelegramUserId).HasDatabaseName("idx_sessions_user");
        });

        modelBuilder.Entity<SettingEntity>(entity =>
        {
            entity.ToTable("settings");
            entity.HasKey(e => e.Key);
            entity.Property(e => e.Key).HasColumnName("key").HasMaxLength(100);
            entity.Property(e => e.Value).HasColumnName("value");
        });
    }
}
