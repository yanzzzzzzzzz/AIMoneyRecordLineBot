using Microsoft.EntityFrameworkCore;

namespace AIMoneyRecordLineBot.Entity;

public partial class AIMoneyRecordLineBotContext : DbContext
{
    public AIMoneyRecordLineBotContext()
    {
    }
    public AIMoneyRecordLineBotContext(DbContextOptions options) : base(options)
    {
    }
    public virtual DbSet<ExpenseRecord> ExpenseRecords { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ExpenseRecord>(entity =>
        {
            entity.ToTable("ExpenseRecords");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Amount)
                .IsRequired()
                .HasColumnType("decimal(10, 2)");

            entity.Property(e => e.Category)
                .HasMaxLength(50);

            entity.Property(e => e.Source)
                .HasMaxLength(50);

            entity.Property(e => e.ConsumptionTime)
                .IsRequired();

            entity.Property(e => e.CreateDateTime)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.UserId)
                .IsRequired();
        });
    }
}
