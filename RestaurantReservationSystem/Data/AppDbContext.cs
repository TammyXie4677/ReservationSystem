using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YourNamespace.Models;

    public class AppDbContext : DbContext
    {
        public AppDbContext (DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<YourNamespace.Models.Restaurant> Restaurant { get; set; } = default!;

public DbSet<YourNamespace.Models.Booking> Booking { get; set; } = default!;
    }
