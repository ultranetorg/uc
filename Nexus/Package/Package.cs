using Uccs.Rdn;

namespace Uccs.Nexus;

public class Package
{
	public const string		CompleteFile = "c";
	public const string		DeltaExtention = "delta";
	
	public const string		New = "new/";
	public const string		Deletion = "delete";
	public const string		Patch = "patch";
	public const string		Renaming = "rename"; /// TODO Maximion

	public const string		Deployed = ".deployed";

	public AutoId			Id;
	public Ura				Address;
	public Urn				Urn;
	public Release			Release => Hub.Node.ResourceHub.Find(Urn);
	public PackageHub		Hub;
	public volatile object	Activity;
	
	PackageManifest			_Manifest;
	PackageInstruction		_Instruction;

	public PackageManifest Manifest
	{
		get
		{
			if(_Manifest == null)
			{
				if(Release != null && Release.IsReady(PackageManifest.Extension))
				{
					lock(Hub.Node.ResourceHub.Lock)
					{
						_Manifest = new Reader(File.ReadAllBytes(Release.Find(PackageManifest.Extension).DataPath)).Read<PackageManifest>();
					}
				}
			}
		
			return _Manifest;
		}
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
						_Instruction = new Reader(File.ReadAllBytes(Release.Find(PackageInstruction.Extension).DataPath)).Read<PackageInstruction>();
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
