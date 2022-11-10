using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Data;
using Models;

public class Processor
{
    private readonly ApplicationDbContext _context;
    public Processor(IDbContextFactory<ApplicationDbContext> factory)
    {
        _context = factory.CreateDbContext();
    }

    public async Task Process(CancellationToken cancellationToken)
    {
        var maxCount = 32;

        var digests = await _context.Users
            .OrderBy(u => u.TimeCreatedUtc)
            .Take(maxCount)
            .Select(source => new DailyDigest
            {
                User = source,
            })
            .ToListAsync(cancellationToken);

        foreach (var digest in digests)
        {
            _context.DailyDigests.Add(digest);
        }

        await _context.SaveChangesAsync();
    }
}