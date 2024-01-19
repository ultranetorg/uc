namespace Uccs.Net
{
	// 	public class ReleaseInstallation
	// 	{
	// 		public LocalRelease	Release;
	// 		public int			TotalFiles;
	// 		public int			UnpackedFiles;
	// 	}
	// 
	public class PackageDeployment
	{
		public LocalPackage		Target;
		public LocalPackage		Incremental;
		public LocalPackage		Complete;
	}

	public class PackageDeploymentProgress : ResourceActivityProgress
	{
		public override string ToString()
		{
			return $"";
		}
	}

}
