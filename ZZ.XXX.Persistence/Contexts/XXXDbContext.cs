﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZZ.XXX.Domain.Common;
using ZZ.XXX.Domain.Entities;

namespace ZZ.XXX.Data.Contexts
{
  public class XXXDbContext : DbContext
  {
    public XXXDbContext(DbContextOptions<XXXDbContext> options) : base(options)
    {
    }

    public DbSet<XXXEntity> XXXs { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {      
      modelBuilder.ApplyConfigurationsFromAssembly(typeof(XXXDbContext).Assembly);

    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
      foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
      {
        switch (entry.State)
        {
          case EntityState.Added:
            entry.Entity.CreatedDate = DateTime.Now;
            entry.Entity.CreatedBy = nameof(ZZ.XXX.Data.Persistence.XXXRepository);
            break;
          case EntityState.Modified:
            entry.Entity.LastModifiedDate = DateTime.Now;
            entry.Entity.LastModifiedBy = nameof(ZZ.XXX.Data.Persistence.XXXRepository);
            break;
        }
      }
      return base.SaveChangesAsync(cancellationToken);
    }



  }
  }