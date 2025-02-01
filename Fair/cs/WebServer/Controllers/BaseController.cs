using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

[ApiController]
[Produces(MediaTypeNames.Application.Json)]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
}
