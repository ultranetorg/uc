using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Uccs.Net
{
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
				var r = sun.ResourceHub.Find(Release) ?? sun.ResourceHub.Add(Release, Type);
			
				if(Type == DataType.File)
				{
					sun.ResourceHub.DownloadFile(r, "f", Release, null, workflow);
					return r.Hash;
				}
				else if(Type == DataType.Directory)
				{
					sun.ResourceHub.DownloadDirectory(r, workflow);
					return r.Hash;
				}
				else
					throw new ResourceException(ResourceError.DataTypeNotSupported);
			}
		}
	}
	
	public class ReleaseDownloadProgressCall : SunApiCall
	{
		public byte[] Release { get; set; }
		
		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{
				var r = sun.ResourceHub.Find(Release);

				return sun.ResourceHub.GetDownloadProgress(r);
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
