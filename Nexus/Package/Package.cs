using Uccs.Rdn;

namespace Uccs.Nexus;

public class Package
{
	public const string		Delta = "d";
	public const string		Complete = "c";
	public const string		Removals = ".removals";
	public const string		Patches = ".patches";
	public const string		Renamings = ".renamings"; /// TODO Maximion

	//public Ura				Address;
	public AutoId			Id;
	public Ura				Address;
	public PackageManifest	Manifest;
	public PackageHub		Hub;
	public object			Activity;
	public Release			Release => Hub.Node.ResourceHub.Find(Manifest.Urn);
	
	PackageInstruction		_Instruction;

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
		return Address.ToString();
	}
}
