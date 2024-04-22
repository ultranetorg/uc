using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Uccs.Net
{
	[JsonDerivedType(typeof(FileDownloadProgress), typeDiscriminator: "FileDownloadProgress")]
	[JsonDerivedType(typeof(ReleaseDownloadProgress), typeDiscriminator: "ReleaseDownloadProgress")]
	public class ResourceActivityProgress
	{

	}

	public class ReleaseBuildApc : SunApc
	{
		public ResourceAddress			Resource { get; set; }
		public IEnumerable<string>		Sources { get; set; }
		public string					FilePath { get; set; }
		public ReleaseAddressCreator	AddressCreator { get; set; }

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{
				if(FilePath != null)
					return sun.ResourceHub.Add(Resource, FilePath, AddressCreator, workflow).Address;
				else if(Sources != null && Sources.Any())
					return sun.ResourceHub.Add(Resource, Sources, AddressCreator, workflow).Address;
			}

			return null;
		}
	}

	public class ResourceDownloadApc : SunApc
	{
		public ResourceAddress	Address { get; set; }

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			var r = sun.Call(i => i.Request<ResourceByNameResponse>(new ResourceByNameRequest {Name = Address}), workflow).Resource;

			IIntegrity itg;

			var rza = r.Data.Interpretation as ReleaseAddress;

			switch(rza)
			{ 
				case DHAddress a :
					itg = new DHIntegrity(a.Hash); 
					break;

				case SDAddress a :
					var au = sun.Call(c => c.Request(new DomainRequest {Name = Address.Domain}), workflow).Domain;
					itg = new SPDIntegrity(sun.Zone.Cryptography, a, au.Owner);
					break;

				default:
					throw new ResourceException(ResourceError.NotSupportedDataType);
			};

			lock(sun.ResourceHub.Lock)
			{
				var lrs = sun.ResourceHub.Find(Address) ?? sun.ResourceHub.Add(Address);
				lrs.AddData(r.Data);

				var lrl = sun.ResourceHub.Find(rza) ?? sun.ResourceHub.Add(rza, r.Data.Type);

				if(r.Data.Type == DataType.File)
				{
					sun.ResourceHub.DownloadFile(lrl, "f", itg, null, workflow);
					return r;
				}
				else if(r.Data.Type == DataType.Directory)
				{
					sun.ResourceHub.DownloadDirectory(lrl, itg, workflow);
					return r;
				}
				else
					throw new ResourceException(ResourceError.NotSupportedDataType);
			}
		}
	}
	
	public class ReleaseActivityProgressApc : SunApc
	{
		public ReleaseAddress Release { get; set; }
		
		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
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
// 		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
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
		
	public class QueryLocalResourcesApc : SunApc
	{
		public string	Query { get; set; }
		public int		Skip { get; set; } = 0;
		public int		Take { get; set; } = int.MaxValue;
		
		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{	
				return sun.ResourceHub.Resources.Where(i => i.Address.ToString().Contains(Query)).Skip(Skip).Take(Take);
			}
		}
	}
	
	public class LocalResourceApc : SunApc
	{
		public ResourceAddress		Resource { get; set; }
		
		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{	
				return sun.ResourceHub.Resources.Find(i => i.Address == Resource);
			}
		}
	}
	
	public class LocalReleaseApc : SunApc
	{
		public ReleaseAddress		Address { get; set; }

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
			public DataType						Type { get; set; }
			public MembersResponse.Member[]		DeclaredOn { get; set; }
			public Availability					Availability { get; set; }
			public File[]						Files { get; set; }
			public string						Path { get; set; }

			public Release()
			{
			}

			public Release(LocalRelease release)
			{
				Type		= release.Type;
				DeclaredOn	= release.DeclaredOn.Select(i => i.Member).ToArray();
				Availability= release.Availability;
				Files		= release.Files.Select(i => new File(i)).ToArray();
				Path		= release.Path;
			}
		}
		
		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{	
				var r = sun.ResourceHub.Find(Address);

				return r != null ? new Release(r) : null;
			}
		}
	}
}
