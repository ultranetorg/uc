namespace Uccs.Fair;

[Flags]
public enum FairRole : uint
{
	None,
}

public class FairNode : McvNode
{
	new public FairTcpPeering		Peering => base.Peering as FairTcpPeering;
	new public FairMcv				Mcv => base.Mcv as FairMcv;
	public FairNodeSettings			Settings;

	public JsonServer				ApiServer;
	public WebServer				WebServer;

	public FairNode(string name, Fair net, string profile, FairNodeSettings settings, Vault vault, IClock clock, Flow flow) : base(name, net, profile, flow, vault)
	{
		Settings = settings ?? new FairNodeSettings(Path.Join(profile, net.Address));

		if(Flow.Log != null)
		{
			new FileLog(Flow.Log, net.Address, Settings.Profile);
		}

		if(NodeGlobals.Any)
			Flow.Log?.ReportWarning(this, $"Dev: {NodeGlobals.AsString}");

		if(Settings.Api != null)
		{
			ApiServer = new FairApiServer(this, Settings.Api, Flow);
		}


		if(Settings.Mcv != null)
		{
			base.Mcv = new FairMcv(net, Settings.Mcv, Path.Join(Settings.Profile, "Mcv"), [Settings.Peering.IP], clock ?? new RealClock());

			if(Settings.Mcv.Generators.Any())
			{
				WebServer = new WebServer(this, null);
			}
		}

		base.Peering = new FairTcpPeering(this, Settings.Peering, Settings.Roles, vault, flow, clock);
	}

	public override string ToString()
	{
		return string.Join(", ", new string[]{	GetType().Name,
												Name,
												(Settings.Api != null ? "A" : null) +
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

//		protected override void CreateTables(ColumnFamilies columns)
//		{
//			base.CreateTables(columns);
//
//			columns.Add(new (ProductHub.ReleaseFamilyName,	new ()));
//			columns.Add(new (ProductHub.ProductFamilyName,new ()));
//		}
//
}
