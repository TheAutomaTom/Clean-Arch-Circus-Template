﻿using FluentValidation.Results;
using Mediator;
using Microsoft.Extensions.Logging;
using ZZ.Core.Application.Interfaces.Persistence;
using ZZ.Core.Domain.Models.Cruds;
using ZZ.Core.Domain.Models.Cruds.Repo;

namespace ZZ.Core.Application.Features.Cruds.CreateCrud
{
  public class CreateCrudHandler : IRequestHandler<CreateCrudRequest, CreateCrudResponse>
	{
		private readonly ICrudDetailRepository _details;
		private readonly ICrudRepository _entities;
		private readonly IMediator _mediator;
		private readonly ILogger<CreateCrudHandler> _logger;

		public CreateCrudHandler(ILogger<CreateCrudHandler> logger, IMediator mediator, ICrudRepository entities, ICrudDetailRepository details)
		{
			_logger = logger;
			_mediator = mediator;
			_entities = entities;
			_details = details;
		}

		public async ValueTask<CreateCrudResponse> Handle(CreateCrudRequest request, CancellationToken ct)
		{
			var validator = new CreateCrudValidator();
			var validationResult = await validator.ValidateAsync(request);

			if (validationResult.Errors.Count > 0)
			{
        var errors = new List<ValidationFailure>();
				foreach (var error in validationResult.Errors)
				{
					errors.Add(error);
				}
        return new CreateCrudResponse() { ValidationErrors = errors };
			}

      try
      {
        var entity = new Crud(request.Name, request.Location);
			  var createdId = await _entities.Create(entity);
        if(createdId == 0)
        {
          throw new Exception("Failed to create Entity.");        
        }

        var detail = new CrudDetail(createdId, request.Description, request.Tags);
        var createdDetailId = await _details.Create(detail);
        if (createdDetailId == 0)
        {
          throw new Exception("Failed to create Detail.");
        }

        var result = new Crud(createdId, entity, detail);

        
        return new CreateCrudResponse(new Crud(createdId, entity, detail));
      } 
      catch (Exception ex)
      {
        var response = new CreateCrudResponse() { Exception = ex };
        return response;
      }
    }





	}
}
