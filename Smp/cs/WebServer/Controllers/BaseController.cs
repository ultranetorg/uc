using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Uccs.Smp;

[ApiController]
[Produces(MediaTypeNames.Application.Json)]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
}
