﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZZ.XXX.Domain.Entities;

namespace ZZ.XXX.Data.DbContexts
{
  public class XXXConfiguration : IEntityTypeConfiguration<XXXEntity>
  {
    public void Configure(EntityTypeBuilder<XXXEntity> builder)
    {
      builder.HasKey(e => e.Id);
      builder.Property(e => e.Id).ValueGeneratedOnAdd();
      builder.Property(e => e.CreatedDate).IsRequired();
      builder.Property(e => e.CreatedBy).IsRequired();
      builder.Property(e => e.LastModifiedDate).IsRequired();
      builder.Property(e => e.LastModifiedBy).IsRequired();
    }
  }
}
