﻿using CCA.Core.Domain.Common.Responses;
using CCA.Core.Domain.Models.Cruds;
using CCA.Core.Infra.Models.Search;

namespace CCA.Core.Application.Features.Cruds.ReadCruds
{
  public class ReadCrudsResponse : BasicResponse
  {
    public ReadCrudsResponse()
    {
      
    }

    public ReadCrudsResponse(IEnumerable<Crud> cruds, Paging? paging, DateRange? updatedDateRange) : base()
    {
      Cruds = cruds;

    }

      public IEnumerable<Crud> Cruds {get; set;}
      public Paging? Paging {get; set;}
      public DateRange? UpdatedDateRange { get; set; }
  }
}