using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using server.Data.DTOs;

namespace server.Data;

public class WalletContextFactory : IDesignTimeDbContextFactory<WalletContext>
{
    public static WalletContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<WalletContext>();
        // TODO: we should hide this away somehow
        // https://www.benday.com/2017/02/17/ef-core-migrations-without-hard-coding-a-connection-string-using-idbcontextfactory/
        //var connection = @"Server=(localdb)\mssqllocaldb;Database=family.time;Trusted_Connection=True;ConnectRetryCount=0";
        var connection = "";
        optionsBuilder.UseSqlServer(connection, x => 
            x.MigrationsAssembly("Migrations"));

        return new WalletContext(optionsBuilder.Options);
    }

    public WalletContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WalletContext>();
        // TODO: we should hide this away somehow
        //var connection = @"Server=(localdb)\mssqllocaldb;Database=family.time;Trusted_Connection=True;ConnectRetryCount=0";
        var connection = "";
        optionsBuilder.UseSqlServer(connection, x => 
            x.MigrationsAssembly("Migrations"));

        return new WalletContext(optionsBuilder.Options);
    }
}
public class WalletContext : DbContext
{
    public WalletContext(DbContextOptions<WalletContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StatDto>()
            .HasKey(c => new { c.TaskId, c.UserId});

        //modelBuilder.Entity<NotificationDto>()
        //    .HasOne()
    }
    public DbSet<TaskDto> Tasks { get; set; } = null!;
    public DbSet<UserDto> Users { get; set; } = null!;
    public DbSet<SpendingAccountDto> SpendingAccounts { get; set; } = null!;
    public DbSet<SavingAccountDto> SavingAccounts { get; set; } = null!;
    public DbSet<StatDto> Stats { get; set; } = null!;
    public DbSet<NotificationDto> Notifications { get; set; } = null!;
    public DbSet<ReserveDto> Reserves { get; set; } = null!;
    public DbSet<AccountHistoryDto> AccountHistories { get; set; } = null!;
}