﻿using HotChocolate.Authorization;

namespace CCA.Api.GraphQL.Queries
{
  public class Query
  {
    //readonly ILogger<Query> _logger;

    //public Query(ILogger<Query> logger)
    //{
    //  _logger = logger;
    //}

    //public async Task<GetXXXByIdResponse> GetXXXById(int id, [Service] IMediator mediator )
    //{
    //    var request = new GetXXXByIdRequest(id);
    //    var response = await mediator.Send(request);

    //    return response;
    //}

    [Authorize(Policy = "Readers")]
    public async Task<string> GetString()
    {
      return "Working!";
    }


  }
}
