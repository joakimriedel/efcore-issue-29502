using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

#nullable enable

public static class Your
{
    public static string ConnectionString = @"Server=(localdb)\mssqllocaldb;Database=Issue29502;Trusted_Connection=True;MultipleActiveResultSets=true;Connect Timeout=30";
}

public class User
{
    public string Id { get; set; } = null!;
    public DateTime TimeCreatedUtc { get; set; }
    public ICollection<DailyDigest> DailyDigests { get; set; } = null!;
}

public class DailyDigest
{
    public int Id { get; set; }
    public User? User { get; set; }
}

public class SomeDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .UseSqlServer(Your.ConnectionString)
            .LogTo(Console.WriteLine, LogLevel.Trace)
            .EnableSensitiveDataLogging();

    public DbSet<User> Users => Set<User>();
    public DbSet<DailyDigest> DailyDigests => Set<DailyDigest>();
}

public class Program
{
    public static async Task Main()
    {
        using (var context = new SomeDbContext())
        {
            if (!await context.Database.CanConnectAsync() || !await context.Users.AnyAsync())
            {
                throw new ApplicationException("Run database.sql script first!");
            }
        }

        using (var context = new SomeDbContext())
        {
            var maxCount = 23;

            var digests = await context.Users
                .OrderBy(u => u.TimeCreatedUtc)
                .Take(maxCount)
                .Select(u => new DailyDigest
                {
                    User = u,
                })
                .ToListAsync();

            foreach (var digest in digests)
            {
                context.DailyDigests.Add(digest);
            }

            await context.SaveChangesAsync();
        }
    }
}