using System;
using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class ResourceQueryCall : SunApiCall
	{
		public string		Query { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.Lock)
				return sun.QueryResource(Query).Resources;
		}
	}

	public class ResourceBuildCall : SunApiCall
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

	public class ResourceDownloadCall : SunApiCall
	{
		public ResourceAddress Resource { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			var r = sun.Call(c => c.FindResource(Resource), workflow).Resource;
					
			Release rs;

			lock(sun.ResourceHub.Lock)
			{
				rs = sun.ResourceHub.Find(Resource, r.Data) ?? sun.ResourceHub.Add(Resource, r.Type, r.Data);
	
				if(r.Type == ResourceType.File)
				{
					sun.ResourceHub.DownloadFile(rs, "f", r.Data, null, workflow);
					
					return r.Data;
				}
				else if(r.Type == ResourceType.Directory)
				{
					sun.ResourceHub.DownloadDirectory(rs, workflow);
					
					return r.Data;
				}
			}
	
			throw new NotSupportedException();
		}
	}
	
	public class ResourceDownloadProgressCall : SunApiCall
	{
		public ResourceAddress	Resource { get; set; }
		public byte[]			Hash { get; set; }
		
		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.ResourceHub.Lock)
				return sun.ResourceHub.GetDownloadProgress(Resource, Hash);
		}
	}
	
	public class ResourceInfoCall : SunApiCall
	{
		public ResourceAddress	Resource { get; set; }
		public byte[]			Hash { get; set; }
		
		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{	
				var r = sun.Call<ResourceResponse>(p => p.FindResource(Resource), workflow);

				var a = sun.ResourceHub.Find(Resource, Hash);

				return new ResourceInfo{ LocalAvailability	= a != null ? a.Availability : Availability.None,
										 LocalLatest		= a != null ? a.Hash : null,
										 LocalLatestFiles	= a != null ? a.Files.Count() : 0,
										 Entity				= r.Resource };
			}
		}
	}

	public class ResourceInfo
	{
		public Availability		LocalAvailability { get; set; }
		public byte[]			LocalLatest { get; set; }
		public int				LocalLatestFiles { get; set; }
		public Resource			Entity { get; set; }
	}
}
