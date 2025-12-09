using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class NodeController
(
	ILogger<NodeController> logger,
	FairNode node
) : BaseController
{
	[HttpGet("urls/nexus")]
	public string GetNexusUrl()
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called", nameof(ProductsController), nameof(GetNexusUrl));

		return node.NexusSettings.Api.LocalAddress(node.Net.Zone, Api.Nexus);
	}

	[HttpGet("urls/vault")]
	public string GetVaultUrl()
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called", nameof(ProductsController), nameof(GetVaultUrl));

		return node.NexusSettings.Api.LocalAddress(node.Net.Zone, Api.Vault);
	}
}
