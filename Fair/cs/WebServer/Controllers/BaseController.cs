using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Uccs.Fair;

[ApiController]
[Produces(MediaTypeNames.Application.Json)]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
}
