namespace Uccs.Fair;

public class FairNode : McvNode
{
	new public FairTcpPeering		Peering => base.Peering as FairTcpPeering;
	new public FairMcv				Mcv => base.Mcv as FairMcv;
	public new FairNodeSettings		Settings => base.Settings as FairNodeSettings;

	public JsonServer				ApiServer;
	public WebServer				WebServer;

	public FairNode(string name, Zone zone, string profile, Settings settings, ApiSettings uosapisettings, ApiSettings apisettings, IClock clock, Flow flow) : base(name, Fair.ByZone(zone), profile, uosapisettings, apisettings, flow)
	{
		base.Settings = settings as FairNodeSettings ?? new FairNodeSettings(profile);

		if(Flow.Log != null)
		{
			new FileLog(Flow.Log, Uccs.Net.Net.Escape(Net.Address), Settings.Profile);
		}

		if(NodeGlobals.Any)
			Flow.Log?.ReportWarning(this, $"Dev: {NodeGlobals.AsString}");

		if(apisettings != null)
		{
			ApiServer = new FairApiServer(this, apisettings, Flow);
		}

		if(Settings.Mcv != null)
		{
			base.Mcv = new FairMcv(Net as Fair, Settings.Mcv, Path.Join(profile, "Mcv"), [Settings.Peering.IP], clock ?? new RealClock());

			if(Settings.Mcv.Generators.Any())
			{
				WebServer = new WebServer(this, null);
			}
		}

		base.Peering = new FairTcpPeering(this, Settings.Peering, Settings.Roles, UosApi, flow, clock);
	}

	public override string ToString()
	{
		return string.Join(", ", new string[]{	GetType().Name,
												Name,
												(ApiServer != null ? "A" : null) +
												(Settings.Mcv != null ? "B" : null) +
												(Settings.Mcv?.Chain != null  ? "C" : null),
												Peering.Connections.Count() < Settings.Peering.PermanentMin ? "Low Peers" : null,
												Mcv != null ? $"{Peering.Synchronization}/{Mcv.LastConfirmedRound?.Id}/{Mcv.LastConfirmedRound?.Hash.ToHexPrefix()}" : null,
												$"T(i/o)={Peering.IncomingTransactions.Count}/{Peering.OutgoingTransactions.Count}"}
					.Where(i => !string.IsNullOrWhiteSpace(i)));
	}

	public override void Stop()
	{
		Flow.Abort();

		WebServer?.Stop();
		ApiServer?.Stop();
		Peering.Stop();
		Mcv?.Stop();

		base.Stop();
	}
}
