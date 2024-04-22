﻿using Mediator;
using Microsoft.AspNetCore.Mvc;
using ZZ.Core.Application.Features.Cruds.CreateCrud;

namespace ZZ.Api.Controllers
{
  /// <summary> Basic C.R.U.D. operations on an example object. </summary>
  [ApiController]
  [Route("[controller]/[action]")]
  public class CrudController : Controller
  {
    readonly ILogger<CrudController> _logger;
    readonly IMediator _mediator;

    public CrudController(ILogger<CrudController> logger, IMediator mediator)
    {
      _logger = logger;
      _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCrudRequest request)
    {
      var result = _mediator.Send(request);

      return Ok(result);
    }


  }
}
