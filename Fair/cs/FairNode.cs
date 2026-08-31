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

	public readonly static string	UniqueDirectiryName = typeof(FairNode).FullName;


	public static readonly string	Description = "The Fair is the decentralized autonomous owner-free platform (DAO) of product publishing communities fully governed by the will of its members";

	public static readonly string[]	ClientRequiredMessage = ["This is decentralized autonomous owner-free platform and requires Client Software installed to participate in its communities.",
															"Windows OS is currently supported only.",
															"Please, follow the link below to install."];
	
	public static readonly string[]	ReadyMessage = ["Welcome to the ULTRANET",
													"and to the Fair Network",
													"Now, find UOS icon in the tray, open its menu and select \"Identity and Activity\" option",
													"There you can manage your crypto wallets and keys that are used to identify yourself when participating in RDN, Fair and other decentralized platforms"];

	public static readonly string[]	AuthorMessage =  ["This is decentralized platform of autonomous transparent community-governed stores",
													  "Anyone can become the author, create product pages and publish it in the stores",
													  "Author has full control over its content and behavior",
													  "Follow this link to learn how to become an author, publish your products and participate in stores governance."];

	public static readonly string[]	WelcomeMessage =  [	..AuthorMessage[..^1],
														"The stores can also be created by anyone which act as aggregators for product listings",
														"Unlike authors, a creator of the store has no full control over it - it's completely governed by all its members",
														"A member of the store is a author who has products published in this store",
														"Each time a next member joins the store existing ones lose part of his leverage",
														"Members vote for its governance policy, elect/recall moderators and thus has full control over their store",
														"Moderators responsible for publishing product updates and other routine operations to maintaining store content clean and tidy"
														];

	public FairNode(Zone zone, NexusSettings nexussettings, FairNodeSettings settings, IClock clock, Flow flow) : base(Fair.ByZone(zone), settings.Profile, nexussettings, flow)
	{
		base.Settings = settings;

		if(Flow.Log != null)
			new LogFile(Flow.Log, GetType().Name, Settings.Profile, flow);

		if(NodeGlobals.Any)
			Flow.Log?.ReportWarning(this, $"Dev: {NodeGlobals.AsString}");

		InitializeVaultClient(NexusSettings.Host);

		if(Settings.Mcv != null)
		{
			base.Mcv = new FairMcv(Net as Fair, Settings.Mcv, settings.DataPath, Path.Join(settings.Profile, "Mcv"), [Settings.Peering.Endpoint], clock ?? new RealClock());
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
