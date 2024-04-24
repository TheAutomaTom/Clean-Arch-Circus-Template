﻿using ZZ.Core.Domain.Models.Cruds.Repo;

namespace ZZ.Core.Application.Interfaces.Persistence
{
  public interface ICrudRepository : IAsyncRepository<CrudEntity>
  {

  }
}