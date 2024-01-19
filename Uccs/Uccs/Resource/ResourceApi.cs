using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading;

namespace Uccs.Net
{
	[JsonDerivedType(typeof(FileDownloadProgress), typeDiscriminator: "FileDownloadProgress")]
	[JsonDerivedType(typeof(ReleaseDownloadProgress), typeDiscriminator: "ReleaseDownloadProgress")]
	public class ResourceActivityProgress
	{

	}

	public class ReleaseBuildCall : SunApiCall
	{
		public ResourceAddress		Resource { get; set; }
		public IEnumerable<string>	Sources { get; set; }
		public string				FilePath { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{
				if(FilePath != null)
					return sun.ResourceHub.Build(Resource, FilePath, workflow).Hash;
				else if(Sources != null && Sources.Any())
					return sun.ResourceHub.Build(Resource, Sources, workflow).Hash;
			}

			return null;
		}
	}

	public class ReleaseDownloadCall : SunApiCall
	{
		public byte[]		Release { get; set; }
		public DataType		Type { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{
				if(Type == DataType.File)
				{
					var r = sun.ResourceHub.Find(Release) ?? sun.ResourceHub.Add(Release, Type);
					sun.ResourceHub.DownloadFile(r, "f", Release, null, workflow);
					return null;
				}
				else if(Type == DataType.Directory)
				{
					var r = sun.ResourceHub.Find(Release) ?? sun.ResourceHub.Add(Release, Type);
					sun.ResourceHub.DownloadDirectory(r, workflow);
					return null;
				}
				else
					throw new ResourceException(ResourceError.DataTypeNotSupported);
			}
		}
	}
	
	public class ReleaseActivityProgressCall : SunApiCall
	{
		public byte[] Release { get; set; }
		
		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{
				var r = sun.ResourceHub.Find(Release);

				if(r.Activity is FileDownload f)
				{
					var s = new ReleaseDownloadProgress(f.SeedCollector);
	
					s.Succeeded	= f.Succeeded;
					s.CurrentFiles = new [] {new FileDownloadProgress(f)};
	
					return s;
				}
				else if(r.Activity is DirectoryDownload d)
				{
					var s = new ReleaseDownloadProgress(d.SeedCollector);
	
					s.Succeeded	= d.Succeeded;
					s.CurrentFiles = r.Files.Where(i => i.Activity is FileDownload).Select(i => new FileDownloadProgress(i.Activity as FileDownload)).ToArray();
	
					return s;
				}
				else
					return null;
			}
		}
	}
	
// 	public class ResourceEntityCall : SunApiCall
// 	{
// 		public ResourceAddress	Resource { get; set; }
// 		//public byte[]			Hash { get; set; }
// 		
// 		public override object Execute(Sun sun, Workflow workflow)
// 		{
// 			lock(sun.ResourceHub.Lock)
// 			{	
// 				var r = sun.Call<ResourceResponse>(p => p.FindResource(Resource), workflow);
// 
// 				//var a = sun.ResourceHub.Find(Resource, Hash);
// 
// 				return r.Resource;
// 			}
// 		}
// 	}
		
	public class QueryLocalResourcesCall : SunApiCall
	{
		public string	Query { get; set; }
		public int		Skip { get; set; } = 0;
		public int		Take { get; set; } = int.MaxValue;
		
		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{	
				return sun.ResourceHub.Resources.Where(i => i.Address.ToString().Contains(Query)).Skip(Skip).Take(Take);
			}
		}
	}
	
	public class LocalResourceCall : SunApiCall
	{
		public ResourceAddress		Resource { get; set; }
		
		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{	
				return sun.ResourceHub.Resources.Find(i => i.Address == Resource);
			}
		}
	}
	
	public class LocalReleaseCall : SunApiCall
	{
		public byte[]		Address { get; set; }

		public class File
		{
			public string			Path { get; set; }
			public int				PieceLength { get; set; }
			public long				Length { get; set; }
			public bool[]			Pieces { get; set; }

			public int[]			CompletedPieces { get; set; }
			public long				CompletedLength { get; set; }
			public bool				Completed { get; set; }

			public File()
			{
			}

			public File(LocalFile i)
			{
				Path =			i.Path;			
				PieceLength	=	i.PieceLength;
				Length =		i.Length;
				Pieces =		i.Pieces;
				
				if(i.Pieces != null)
				{
					CompletedPieces = i.CompletedPieces.ToArray();
					CompletedLength = i.CompletedLength;
				}
			
				Completed = i.Completed;
			}
		}

		public class Release
		{
			public byte[]							Hash { get; set; }
			public DataType							Type { get; set; }
			public List<MembersResponse.Member>		DeclaredOn { get; set; }
			public Availability						Availability { get; set; }
			public File[]							Files { get; set; }
			public string							Path { get; set; }

			public Release()
			{
			}

			public Release(LocalRelease release)
			{
				Hash		= release.Hash;
				Type		= release.Type;
				DeclaredOn	= release.DeclaredOn;
				Availability= release.Availability;
				Files		= release.Files.Select(i => new File(i)).ToArray();
				Path		= release.Path;
			}
		}
		
		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{	
				var r = sun.ResourceHub.Releases.Find(i => i.Hash.SequenceEqual(Address));

				return r != null ? new Release(r) : r;
			}
		}
	}
}
