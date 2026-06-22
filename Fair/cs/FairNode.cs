using System.Reflection;
using DnsClient;

namespace Uccs.Fair;

public class FairNode : McvNode
{
	new public FairTcpPeering		Peering => base.Peering as FairTcpPeering;
	new public FairMcv				Mcv => base.Mcv as FairMcv;
	public new FairNodeSettings		Settings => base.Settings as FairNodeSettings;

	public JsonServer				ApiServer;
	public WebServer				WebServer;
	List<OutwardTransaction>		CurrentOutwards = [];

	public FairNode(Zone zone, string profile, NexusSettings nexussettings, FairNodeSettings settings, IClock clock, Flow flow) : base(Fair.ByZone(zone), profile, nexussettings, flow)
	{
		base.Settings = settings;

		if(Flow.Log != null)
			new FileLog(Flow.Log, GetType().Name, Settings.Profile, flow);

		if(NodeGlobals.Any)
			Flow.Log?.ReportWarning(this, $"Dev: {NodeGlobals.AsString}");

		InitializeVaultClient(NexusSettings.Host);

		if(Settings.Mcv != null)
		{
			base.Mcv = new FairMcv(Net as Fair, Settings.Mcv, settings.DataPath, Path.Join(profile, "Mcv"), [Settings.Peering.Endpoint], clock ?? new RealClock());
			base.Mcv.Log = Flow.Log;

			Mcv.Confirmed += r =>	{
										foreach(var t in r.OutwardTransactions.Where(i =>	!CurrentOutwards.Any(a => a.User == i.User && a.Id == i.Id) &&
																							!Mcv.OutwardResults.Any(a => a.User == i.User && a.Id == i.Id)))
										{
											Task.Run(() =>	{
																if(t.Operation is AuthorVerification o)
																{
																	var approved = IsWebdomainOwner(o.Webdomain, t.User);
	
																	lock(Mcv.Lock)
																	{	
																		Mcv.OutwardResults.Add(new OutwardResult {User = t.User, Id = t.Id, Approved = approved});

																		CurrentOutwards.Remove(t);
																	}
																}
															});
										}
									};

			if(Settings.Web != null)
			{
				WebServer = new WebServer(this, null);
			}
		}
	
		Iccp = new FairIccpLcpConnection(this, flow);
		
		base.Peering = new FairTcpPeering(this, Settings.Peering, Settings.Roles, VaultApi, flow, clock);
		
		ApiServer = new FairApiServer(this, (Settings.Api ?? new ()).ToNodeSettings(Net), Flow);
	}

	public override string ToString()
	{
		lock(Peering.Lock)
			return string.Join(", ", new string[]{	GetType().Name,
													Name,
													(Settings.Mcv != null ? "G" : null) +
													(Settings.Mcv?.Chain != null  ? "C" : null),
													Peering.Connections.Count() < Settings.Peering.PermanentMin ? "Low Peers" : null,
													Mcv != null ? $"{Peering.Synchronization}{(Peering.SynchronizationInfo != null ? $"-{Peering.SynchronizationInfo}" : null)}/{Mcv.LastConfirmedRound?.Id}/{Mcv.LastConfirmedRound?.Hash.ToHexPrefix()}" : null,
													$"T(i/o)={Peering.CandidateTransactions.Count}/{Peering.OutgoingTransactions.Count}"}
						.Where(i => !string.IsNullOrWhiteSpace(i)));
	}

	public override void Stop()
	{
		Flow.Abort();

		ApiServer?.Stop();
		Peering.Stop();
		Iccp?.Disconnect();
		WebServer?.Stop();
		Mcv?.Stop();

		base.Stop();
	}

	public override byte[] Do(string query)
	{
		return null;
	}
}
