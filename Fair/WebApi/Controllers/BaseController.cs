using System.Net.Mime;

namespace Explorer.WebApi.Controllers;

[Produces(MediaTypeNames.Application.Json)]
[Route("api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
}
