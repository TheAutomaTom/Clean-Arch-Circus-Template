﻿using AutoMapper;
using Mediator;
using Microsoft.Extensions.Logging;
using ZZ.Core.Application.Features.XXX.GetAllElastic;
using ZZ.Core.Application.Interfaces.Persistence;
using ZZ.Core.Domain._Deprecated.Elastic;

namespace ZZ.Core.Application.Features.XXX.GetAllElastic
{
  public class PostToElasticHandler : IRequestHandler<PostToElasticRequest, PostToElasticResponse>
  {
    readonly IMapper _mapper;
    readonly IXXXElasticRepository _repo;
    readonly ILogger<PostToElasticHandler> _logger;

    public PostToElasticHandler(ILogger<PostToElasticHandler> logger, IXXXElasticRepository repo, IMapper mapper)
    {
      _logger = logger;
      _repo = repo;
      _mapper = mapper;
    }

    public async ValueTask<PostToElasticResponse> Handle(PostToElasticRequest request, CancellationToken cancellationToken)
    {
      var toSave = _mapper.Map<XXXEls>(request.XXX);

      var result = await _repo.Create(toSave);

      return new PostToElasticResponse(result);



    }
  }
}
