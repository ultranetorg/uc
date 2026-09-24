using System.Reflection;

namespace Uccs.Fair;

public class FairCli : McvCli
{
	public FairCli()
	{
		Boot = new NetBoot(ExeDirectory);

		Net				= Fair.ByZone(Boot.Zone);
		NexusSettings	= new NexusSettings(Boot.Zone, Boot.Profile);
		Settings		= new FairNodeSettings(NexusSettings);

		Api				= new FairApiClient(Settings.Api.LocalNodeAddress(Net));

		Execute(Boot.Profile, Boot.Commnand);
	}

	public FairCli(NexusSettings nexussettings, FairNodeSettings settings, FairApiClient api) : base(nexussettings, settings, api)
	{
		Net	= Fair.ByZone(nexussettings.Zone);
	}

	public override void Collect()
	{
		base.Collect();

		Commands.Remove(typeof(Uccs.Net.UserCommand));

		Commands.Add(typeof(AuthorCommand));
		Commands.Add(typeof(FileCommand));
		Commands.Add(typeof(NodeCommand));
		Commands.Add(typeof(ProductCommand));
		Commands.Add(typeof(PublicationCommand));
		Commands.Add(typeof(StoreCommand));
		Commands.Add(typeof(UserCommand));
	}
}
