using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;
using DnsClient;
using Uccs.Nexus;

namespace Uccs.Rdn;

[Flags]
public enum RdnRole : uint
{
	None,
	Seed = 0b00000100,
}

public class RdnNode : McvNode
{
	new public RdnTcpPeering		Peering => base.Peering as RdnTcpPeering;
	new public RdnMcv				Mcv => base.Mcv as RdnMcv;
	public new Rdn					Net => base.Net as Rdn;
	public new RdnNodeSettings		Settings => base.Settings as RdnNodeSettings;

	public ResourceHub				ResourceHub;
	public SeedHub					SeedHub;
	public JsonServer				ApiServer;
	List<OutwardTransaction>		CurrentOutwards = [];
	public string					DataPath;

	public RdnNode(Zone zone, string profile, NexusSettings nexussettings, RdnNodeSettings settings, IClock clock, Flow flow) : base(Rdn.ByZone(zone), profile, nexussettings, flow)
	{
		base.Settings = settings ?? new RdnNodeSettings(profile);

		if(settings == null && !File.Exists(Settings.Path))
		{
			Settings.Peering	= new () {Endpoint = new (IPAddress.Any, Net.PpiPort)};
			Settings.Api		= new () {LocalIP = nexussettings.Host};
			Settings.Seed		= new ();

			Settings.Save();
		}

		DataPath = Settings.DataPath ?? System.IO.Path.Join(ExeDirectory, nameof(Rdn));;	

		if(Flow.Log != null)
			new FileLog(Flow.Log, GetType().Name, Settings.Profile, flow);

		if(NodeGlobals.Any)
			Flow.Log?.ReportWarning(this, $"Dev: {NodeGlobals.AsString}");

		InitializeVaultClient(NexusSettings.Host);

		if(Settings.Mcv != null)
		{
			base.Mcv = new RdnMcv(Net, Settings.Mcv, DataPath, Path.Join(profile, "Mcv"), [Settings.Peering.Endpoint], [Settings.Peering.Endpoint], clock ?? new RealClock());

			if(Settings.Mcv.Generators.Any())
			{
				SeedHub = new SeedHub(Mcv);
			}

			Mcv.Confirmed += r =>	{
										foreach(var t in r.OutwardTransactions.Where(i => !CurrentOutwards.Any(a => a.User == i.User && a.Id == i.Id) &&
																						  !Mcv.OutwardResults.Any(a => a.User == i.User && a.Id == i.Id)))
										{
											Task.Run(() =>	{
																if(t.Operation is DomainMigration m)
																{
																	var approved = IsWebdomainOwner(m.Name + '.' + m.Tld, t.User);
	
																	lock(Mcv.Lock)
																	{	
																		Mcv.OutwardResults.Add(new OutwardResult {User = t.User, Id = t.Id, Approved = approved});

																		CurrentOutwards.Remove(t);
																	}
																}
																else if(t.Operation is FriendAttachment sa)
																{
																	lock(Mcv.Lock)
																		Mcv.OutwardResults.Add(new OutwardResult {User = t.User, Id = t.Id, Approved = Settings.ProposedFriendAttachments.Contains(sa.Name)});
																}
															});
										}
									};
		}

		Iccp = new McvIccpLcpConnection(this, Name, flow);

		if(Settings.Api != null)
		{
			ApiServer = new RdnApiServer(this, Settings.Api.ToNodeSettings(Net), Flow);
		}

		base.Peering = new RdnTcpPeering(this, Settings.Peering, Settings.Roles, VaultApi, flow, clock);
		
		if(Settings.Seed != null)
		{
			ResourceHub = new ResourceHub(this, Net, Settings.Seed);
			ResourceHub.RunDeclaring();
		}
	}

	public override string ToString()
	{
		string f()=> string.Join(", ", new string[]{GetType().Name,
													Name,
													(ApiServer != null ? "A" : null) +
													(Settings.Mcv != null ? "G" : null) +
													(Settings.Mcv?.Chain != null  ? "C" : null) +
													(Peering.Synchronization == Synchronization.Synchronized && Mcv.NextVotingRound.Senders.Any(i => Settings.Mcv.Generators.Any(g => g.Id == i.User)) ? "M" : null) +
													(Settings.Seed != null  ? "S" : null),
													Peering.Connections.Count() < Settings.Peering.PermanentMin ? "Low Peers" : null,
													Mcv != null ? $"{Peering.Synchronization}{(Peering.SynchronizationInfo != null ? $"-{Peering.SynchronizationInfo}" : null)}/{Mcv.LastConfirmedRound?.Id}/{Mcv.LastConfirmedRound?.Hash.ToHexPrefix()}" : null,
													$"T(i/o)={Peering.CandidateTransactions.Count}/{Peering.OutgoingTransactions.Count}"}
						.Where(i => !string.IsNullOrWhiteSpace(i)));

		if(Mcv != null)
		{
			lock(Mcv.Lock)
				return f();
		} 
		else
			return f();
	}

	public override void Stop()
	{
		Flow.Abort();

		ApiServer?.Stop();
		Peering.Stop();
		Iccp?.Disconnect();
		//NnPeering?.Stop();
		Mcv?.Stop();

		base.Stop();
	}

	public LocalRelease Download(Resource resource, Flow flow)
	{
		new ResourceDownloadApc {Id = resource.Id}.Execute(this, null, null, flow);

		LocalRelease l;

		lock(ResourceHub.Lock)
			l = ResourceHub.Find(resource.Data.Parse<Urr>());

		do
		{
			if(l.Activity is null)
			{
				return l;
			}

			Thread.Sleep(100);
		}
		while(flow.Active);

		throw new OperationCanceledException();
	}

	public override byte[] Do(string query)
	{
		throw new NotImplementedException();
	}
}
