﻿using CCA.Core.Domain.Common.Responses;
using CCA.Core.Domain.Models.Cruds.Responses;
using Mediator;

namespace CCA.Core.Application.Features.Cruds.UpdateCrud
{
  public class UpdateCrudRequest : IRequest<BasicResponse>
  {
    public UpdateCrudRequest()
    {
      
    }

    public UpdateCrudRequest(CrudUpdate crud)
    {

      Id = crud.Id;
      Name = crud.Name;
      Department = crud.Department;
      Description = crud.Description;
      Tags = crud.Tags;
    }

    public int Id { get; set; }

    public string Name { get; set; }

    public string Department { get; set; }
    public string Description { get; set; }
    public IEnumerable<string> Tags { get; set; }


  }
}
