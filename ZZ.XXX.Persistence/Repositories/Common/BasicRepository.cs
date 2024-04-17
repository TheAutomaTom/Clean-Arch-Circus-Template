﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ZZ.XXX.Application.Interfaces.Persistence;
using ZZ.XXX.Data.Contexts;

namespace ZZ.XXX.Data.Persistence.Common
{
  public class BasicRepository<T> : IAsyncRepository<T> where T : class
  {
    protected readonly XXXDbContext _dbContext;

    public BasicRepository(XXXDbContext dbContext)
    {
      _dbContext = dbContext;      
    }

    public virtual async Task<T> ReadById(int id)
    {
      return await _dbContext.Set<T>().FindAsync(id);
    }

    public virtual async Task<IReadOnlyList<T>> Read()
    {
      return await _dbContext.Set<T>().ToListAsync();
    }

    public virtual async Task<int> Create(T entity)
    {
      _dbContext.Entry(entity).State = EntityState.Added;
      return await _dbContext.SaveChangesAsync();
    }

    public virtual async Task<int> Update(T entity)
    {
      _dbContext.Entry(entity).State = EntityState.Modified;
      return await _dbContext.SaveChangesAsync();
    }

    public virtual async Task<int> Delete(T entity)
    {
      _dbContext.Set<T>().Remove(entity);
      return await _dbContext.SaveChangesAsync();

    }




  }
}