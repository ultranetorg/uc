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

	public Ura				Address;
	public Urn				Urn;
	public LocalRelease		Release => Hub.Node.ResourceHub.Find(Urn);
	public PackageHub		Hub;
	public volatile object	Activity;
	
	PackageManifest			_Manifest;
	PackageInstruction		_Instruction;

	public bool IsDeployed(string packagespath)
	{
		var path = PackageHub.AddressToDeployment(packagespath, Address, Package.Deployed);
		return File.Exists(path) && File.ReadAllText(path) == Urn.ToString();
	}

	public void SetDeployed(string packagespath)
	{
		File.WriteAllText(PackageHub.AddressToDeployment(packagespath, Address, Package.Deployed), Urn.ToString());
	}

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
						_Manifest = new Reader(File.ReadAllBytes(Release.Find(PackageManifest.Extension).DataPath), Rdn.Rdn.Any.Constructor).Read<PackageManifest>();
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
						_Instruction = new Reader(File.ReadAllBytes(Release.Find(PackageInstruction.Extension).DataPath), Rdn.Rdn.Any.Constructor).Read<PackageInstruction>();
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
