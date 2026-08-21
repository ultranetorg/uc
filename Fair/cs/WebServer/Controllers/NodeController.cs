using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class NodeController
(
#if DEBUG
	ILogger<NodeController> logger,
#endif
	FairNode node
) : BaseController
{
	[HttpGet("urls/nexus")]
	public string GetNexusUrl()
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called", nameof(NodeController), nameof(NodeController.GetNexusUrl));
#endif

		return new IpApiSettings {LocalIP = node.Net.Zone == Zone.Simulation ? node.NexusSettings.Host : NexusSettings.StandardHost}.LocalSystemAddress(node.Net.Zone, Api.Nexus);
	}

	[HttpGet("urls/vault")]
	public string GetVaultUrl()
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called", nameof(NodeController), nameof(NodeController.GetVaultUrl));
#endif

		return new IpApiSettings {LocalIP = node.Net.Zone == Zone.Simulation ? node.NexusSettings.Host : NexusSettings.StandardHost}.LocalSystemAddress(node.Net.Zone, Api.Vault);
	}
}
