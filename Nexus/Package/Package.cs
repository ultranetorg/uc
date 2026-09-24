using Uccs.Rdn;

namespace Uccs.Nexus;

public class Package
{
	public const string		DeltaFile = "d";
	public const string		CompleteFile = "c";
	public const string		Removals = ".removals";
	public const string		Patches = ".patches";
	public const string		Renamings = ".renamings"; /// TODO Maximion

	//public Ura				Address;
	public AutoId			Id;
	public PackageHub		Hub;
	public object			Activity;
	public Release			Release => Hub.Node.ResourceHub.Find(Manifest.Urn);
	PackageManifest			_Manifest;
	PackageInstruction		_Instruction;

	//public HistoryRelease	HistoryRelease => History.Releases.First(i => i.Hash.SequenceEqual(Address.Hash));
	//public History		History => Hub.Sun.ResourceHub.Find(Address).LastAs<History>();

	public PackageManifest Manifest
	{
		get
		{
			if(_Manifest == null)
			{
				lock(Hub.Node.ResourceHub.Lock)
				{
					_Manifest = Hub.Node.ResourceHub.Get(Id).Read<PackageManifest>();
				}
			}
		
			return _Manifest;
		}
		set => _Manifest = value;
	}

	public PackageInstruction Instruction
	{
		get
		{
			if(_Instruction == null)
			{
				if(Release != null && Release.IsReady(PackageInstruction.Extension))
				{
					lock(Hub.Node.ResourceHub.Lock)
					{
						_Instruction = PackageInstruction.Load(Release.Find(PackageInstruction.Extension).DataPath);
					}
				}
			}
		
			return _Instruction;
		}
	}

	public Package(PackageHub hub)
	{
		Hub	= hub;
	}

	public override string ToString()
	{
		return Id.ToString();
	}
}
