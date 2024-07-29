using System.Net;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Uccs.Rdn
{
	[JsonDerivedType(typeof(FileDownloadProgress),		typeDiscriminator: "FileDownloadProgress")]
	[JsonDerivedType(typeof(ReleaseDownloadProgress),	typeDiscriminator: "ReleaseDownloadProgress")]
	[JsonDerivedType(typeof(PackageDownloadProgress),	typeDiscriminator: "PackageDownloadProgress")]
	[JsonDerivedType(typeof(DeploymentProgress),		typeDiscriminator: "DeploymentProgress")]
	public class ResourceActivityProgress
	{
	}

	public class ReleaseBuildApc : RdnApc
	{
		public IEnumerable<string>		Sources { get; set; }
		public string					Source { get; set; }
		public ReleaseAddressCreator	AddressCreator { get; set; }

		public override object Execute(RdnNode sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{
				if(Source != null)
					return sun.ResourceHub.Add(Source, AddressCreator, workflow).Address;
				else if(Sources != null && Sources.Any())
					return sun.ResourceHub.Add(Sources, AddressCreator, workflow).Address;
			}

			return null;
		}
	}

	public class ResourceUpdateApc : RdnApc
	{
		public Ura				Address { get; set; }
		public ResourceId		Id { get; set; }
		public ResourceData		Data { get; set; }

		public override object Execute(RdnNode sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{
				var r = sun.ResourceHub.Find(Address) ?? sun.ResourceHub.Add(Address);
				r.Id = Id;
				
				if(Data != null)
				{
					r.AddData(Data);
				}
			}

			return null;
		}
	}

	public class ResourceDownloadApc : RdnApc
	{
		public ResourceIdentifier	Identifier { get; set; }
		public string				LocalPath { get; set; }

		public override object Execute(RdnNode sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			var r = sun.Call(() => new ResourceRequest(Identifier), workflow).Resource;

			IIntegrity itg;

			var urr = r.Data.Parse<Urr>();

			switch(urr)
			{ 
				case Urrh a :
					itg = new DHIntegrity(a.Hash); 
					break;

				case Urrsd a :
					///.var au = sun.Call(c => c.Request(new DomainRequest(Idedtifier)), workflow).Domain;
					///.itg = new SPDIntegrity(sun.Zone.Cryptography, a, au.Owner);
					throw new NotSupportedException();
					
				default:
					throw new ResourceException(ResourceError.NotSupportedDataType);
			};

			lock(sun.ResourceHub.Lock)
			{
				var lrs = sun.ResourceHub.Find(r.Address) ?? sun.ResourceHub.Add(r.Address);
				lrs.AddData(r.Data);

				var lrl = sun.ResourceHub.Find(urr) ?? sun.ResourceHub.Add(urr);

				if(r.Data.Type.Control == DataType.File)
				{
					sun.ResourceHub.DownloadFile(lrl, true, "", LocalPath ?? sun.ResourceHub.ToReleases(urr), itg, null, workflow);
					return r;
				}
				else if(r.Data.Type.Control == DataType.Directory)
				{
					sun.ResourceHub.DownloadDirectory(lrl, LocalPath ?? sun.ResourceHub.ToReleases(urr), itg, workflow);
					return r;
				}
				else
					throw new ResourceException(ResourceError.NotSupportedDataType);
			}
		}
	}
	
	public class ReleaseActivityProgressApc : RdnApc
	{
		public Urr Release { get; set; }
		
		public override object Execute(RdnNode sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{
				var r = sun.ResourceHub.Find(Release);

				if(r.Activity is FileDownload f)
				{
					var s = new ReleaseDownloadProgress(f.Harvester);
	
					s.Succeeded	= f.Succeeded;
					s.CurrentFiles = [new FileDownloadProgress(f)];
	
					return s;
				}
				else if(r.Activity is DirectoryDownload d)
				{
					var s = new ReleaseDownloadProgress(d.Harvester);
	
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
		
	public class QueryLocalResourcesApc : RdnApc
	{
		public string	Query { get; set; }
		public int		Skip { get; set; } = 0;
		public int		Take { get; set; } = int.MaxValue;
		
		public override object Execute(RdnNode sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{	
				return (Query == null ? sun.ResourceHub.Resources : sun.ResourceHub.Resources.Where(i => i.Address.ToString().Contains(Query))).Skip(Skip).Take(Take);
			}
		}
	}
	
	public class LocalResourceApc : RdnApc
	{
		public Ura		Resource { get; set; }
		
		public override object Execute(RdnNode sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{	
				return sun.ResourceHub.Resources.Find(i => i.Address == Resource);
			}
		}
	}
	
	public class LocalReleaseApc : RdnApc
	{
		public Urr		Address { get; set; }

		public class File
		{
			public string			Path { get; set; }
			public int				PieceLength { get; set; }
			public long				Length { get; set; }
			public bool[]			Pieces { get; set; }

			public int[]			CompletedPieces { get; set; }
			public long				CompletedLength { get; set; }
			public LocalFileStatus	Status { get; set; }

			public File()
			{
			}

			public File(LocalFile i)
			{
				Path =			i.Path;			
				PieceLength	=	i.PieceLength;
				Length =		i.Length;
				Pieces =		i.Pieces;
				Status =		i.Status;
				
				if(i.Pieces != null)
				{
					CompletedPieces = i.CompletedPieces.ToArray();
					CompletedLength = i.CompletedLength;
				}
			}
		}

		public class Return
		{
			public Member[]			DeclaredOn { get; set; }
			public Availability		Availability { get; set; }
			public File[]			Files { get; set; }

			public Return()
			{
			}

			public Return(LocalRelease release)
			{
				DeclaredOn	= release.DeclaredOn.Select(i => i.Member).ToArray();
				Availability= release.Availability;
				Files		= release.Files.Select(i => new File(i)).ToArray();
			}
		}
		
		public override object Execute(RdnNode sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{	
				var r = sun.ResourceHub.Find(Address);

				return r != null ? new Return(r) : null;
			}
		}
	}
}
