using System.Net;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Uccs.Rdn;

[JsonDerivedType(typeof(FileDownloadProgress),	 	typeDiscriminator: "FileDownloadProgress")]
[JsonDerivedType(typeof(ReleaseDownloadProgress),	typeDiscriminator: "ReleaseDownloadProgress")]
public class ResourceActivityProgress
{
}

//public class LocalResourceAddApc : RdnApc
//{
//	public Ura		Address { get; set; }
//
//	public override object Execute(RdnNode node, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
//	{
//		lock(node.ResourceHub.Lock)
//		{
//			node.ResourceHub.Add(Address);
//		}
//
//		return null;
//	}
//}

public class ResourceDownloadApc : RdnApc
{
	public AutoId			Id { get; set; }
	public string			To { get; set; }

	public override object Execute(RdnNode node, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		var d = node.ResourceHub.Get(Id);

		if(d == null)
			throw new ResourceException(ResourceError.NoData);

		if(d.Meaning == Meaning.Rex_File && d.Meaning == Meaning.Rex_Directory)
			throw new ResourceException(ResourceError.InvalidMeaning);

		IIntegrity itg;

		var urr = d.ReadVirtual<Urn>(Rdn.Any.Constructor);

		switch(urr)
		{ 
			case Hcid a :
				itg = new DHIntegrity(a.Hash); 
				break;

			//case Urrsd a :
			//	///.var au = node.Call(c => c.Request(new DomainRequest(Idedtifier)), flow).Domain;
			//	///.itg = new SPDIntegrity(node.Net.Cryptography, a, au.Owner);
			//	throw new NotSupportedException();
				
			default:
				throw new ResourceException(ResourceError.NotSupportedDataType);
		};

		lock(node.ResourceHub.Lock)
		{
			var lrl = node.ResourceHub.Find(urr) ?? node.ResourceHub.Add(urr, Id);

			if(d.Meaning == Meaning.Rex_File)
			{
				node.ResourceHub.DownloadFile(lrl, true, "", To ?? node.ResourceHub.ToReleases(urr), itg, null, flow);
			}
			else if(d.Meaning == Meaning.Rex_Directory)
			{
				node.ResourceHub.DownloadDirectory(lrl, To ?? node.ResourceHub.ToReleases(urr), itg, flow);
			}
			else
				throw new ResourceException(ResourceError.NotSupportedDataType);
		}
		return null;
	}
}

public class CancelResourceDownloadApc : RdnApc
{
	public Urn	Release { get; set; }

	public override object Execute(RdnNode node, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(node.ResourceHub.Lock)
		{
			var lrs = node.ResourceHub.Find(Release);

			if(lrs?.Activity is FileDownload f)
				f.Stop();
			else if(lrs?.Activity is DirectoryDownload d)
				d.Stop();
		}

		return null;
	}
}

public class LocalReleaseBuildApc : RdnApc
{
	public IEnumerable<string>		Sources { get; set; }
	public ReleaseAddressCreator	AddressCreator { get; set; }

	public override object Execute(RdnNode node, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(node.ResourceHub.Lock)
		{
			if(Sources.Count() == 1 && File.Exists(Sources.First()))
				return new LocalReleaseApe(node.ResourceHub.Add(Sources.First(), AddressCreator, flow));
			else
				return new LocalReleaseApe(node.ResourceHub.Add(Sources, AddressCreator, flow));
		}
	}
}

//public class LocalReleaseAddApc : RdnApc
//{
//	public AutoId			Resource { get; set; }
//	public ResourceData		Data  { get; set; }
//	public Urr				Release { get; set; }
//	public byte[]			Content { get; set; }
//	public string			Path  { get; set; }
//	public string			LocalPath  { get; set; }
//	public Availability		Availability { get; set; } = Availability.Full;
//
//	public override object Execute(RdnNode node, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
//	{
//		lock(node.ResourceHub.Lock)
//		{
//			var rc = node.ResourceHub.Find(Resource) ?? node.ResourceHub.Add(Resource);
//			rc.AddData(Data);
//
//			if(Release != null)
//			{
//				var rl = node.ResourceHub.Find(Release);
//				
//				if(rl == null)
//				{
//					rl = node.ResourceHub.Add(Release);
//					rl.AddCompleted(Path, LocalPath, Content);
//					rl.Complete(Availability);
//				}
//			}
//		}
//
//		return null;
//	}
//}

//public class LocalResourceUpdateApc : RdnApc
//{
//	public AutoId			Id { get; set; }
//	public ResourceData		Data { get; set; }
//
//	public override object Execute(RdnNode node, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
//	{
//		lock(node.ResourceHub.Lock)
//		{
//			var r = node.ResourceHub.Find(Id) ?? node.ResourceHub.Add(Address);
//			
//			if(Id != null)
//			{
//				r.Id = Id;
//			}
//			
//			if(Data != null)
//			{
//				r.AddData(Data);
//			}
//		}
//
//		return null;
//	}
//}

public class LocalReleaseActivityProgressApc : RdnApc
{
	public Urn Release { get; set; }
	
	public override object Execute(RdnNode node, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(node.ResourceHub.Lock)
		{
			var r = node.ResourceHub.Find(Release);

			if(r.Activity is FileDownload f)
			{
				var s = new ReleaseDownloadProgress(f.Seeker);

				s.Succeeded	= f.Succeeded;
				s.CurrentFiles = [new FileDownloadProgress(f)];

				return s;
			}
			else if(r.Activity is DirectoryDownload d)
			{
				var s = new ReleaseDownloadProgress(d.Seeker);

				s.Succeeded	= d.Succeeded;
				s.CurrentFiles = r.Files.Where(i => i.Activity is FileDownload).Select(i => new FileDownloadProgress(i.Activity as FileDownload)).ToArray();

				return s;
			}
			else
				return null;
		}
	}
}

public class LocalReleaseReadApc : RdnApc
{
	public Urn			Address { get; set; }
	public string		Path  { get; set; }
	public long			Offset  { get; set; } = 0;
	public long			Length  { get; set; } = -1;

	public override object Execute(RdnNode node, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(node.ResourceHub.Lock)
		{
			var rc = node.ResourceHub.Find(Address);

			if(rc == null)
				throw new ResourceException(ResourceError.NotFound);

			return rc.Find(Path).Read();
		}
	}
}

// 	public class ResourceEntityCall : SunApiCall
// 	{
// 		public ResourceAddress	Resource { get; set; }
// 		//public byte[]			Hash { get; set; }
// 		
// 		public override object Execute(Sun node, HttpListenerRequest request, HttpListenerResponse response, Workflow flow)
// 		{
// 			lock(node.ResourceHub.Lock)
// 			{	
// 				var r = node.Call<ResourceResponse>(p => p.Resources.Find(Resource), flow);
// 
// 				//var a = node.ResourceHub.Find(Resource, Hash);
// 
// 				return r.Resource;
// 			}
// 		}
// 	}
	
public class LocalResourcesSearchApc : RdnApc
{
	public string	Query { get; set; }
	public int		Skip { get; set; } = 0;
	public int		Take { get; set; } = int.MaxValue;
	
	public override object Execute(RdnNode node, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(node.ResourceHub.Lock)
		{
			throw new NotImplementedException();
		//	return (Query == null ? node.ResourceHub.Resources : node.ResourceHub.Resources.Where(i => i.Address.ToString().Contains(Query))).Skip(Skip).Take(Take);
		}
	}
}

//public class CachedResourceApc : RdnApc
//{
//	public AutoId		Id { get; set; }
//	
//	public override object Execute(RdnNode node, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
//	{
//		lock(node.ResourceHub.Lock)
//		{	
//			return node.ResourceHub.Find(Id);
//		}
//	}
//}

public class LocalReleaseApe
{
	public Urn				Address { get; set; }
	public Member[]			DeclaredOn { get; set; }
	public Availability		Availability { get; set; }
	//public File[]			Files { get; set; }

	public LocalReleaseApe()
	{
	}

	public LocalReleaseApe(Release release)
	{
		Address		= release.Address;
		DeclaredOn	= release.DeclaredOn.Select(i => i.Member).ToArray();
		Availability= release.Availability;
		//Files		= release.Files.Select(i => new File(i)).ToArray();
	}
}

public class LocalReleaseApc : RdnApc
{
	public Urn		Address { get; set; }

// 		public class File
// 		{
// 			public string			Path { get; set; }
// 			public int				PieceLength { get; set; }
// 			public long				Length { get; set; }
// 			public bool[]			Pieces { get; set; }
// 
// 			public int[]			CompletedPieces { get; set; }
// 			public long				CompletedLength { get; set; }
// 			public LocalFileStatus	Status { get; set; }
// 
// 			public File()
// 			{
// 			}
// 
// 			public File(LocalFile i)
// 			{
// 				Path =			i.Path;			
// 				PieceLength	=	i.PieceLength;
// 				Length =		i.Length;
// 				Pieces =		i.Pieces;
// 				Status =		i.Status;
// 				
// 				if(i.Pieces != null)
// 				{
// 					CompletedPieces = i.CompletedPieces.ToArray();
// 					CompletedLength = i.CompletedLength;
// 				}
// 			}
// 		}

	
	public override object Execute(RdnNode node, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(node.ResourceHub.Lock)
		{	
			var r = node.ResourceHub.Find(Address);

			return r != null ? new LocalReleaseApe(r) : null;
		}
	}
}
