using System;
using System.Collections.Generic;
using System.Linq;

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
	
	public class ResourceEntityCall : SunApiCall
	{
		public ResourceAddress	Resource { get; set; }
		//public byte[]			Hash { get; set; }
		
		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{	
				var r = sun.Call<ResourceResponse>(p => p.FindResource(Resource), workflow);

				//var a = sun.ResourceHub.Find(Resource, Hash);

				return r.Resource;
			}
		}
	}
		
	public class LocalResourcesCall : SunApiCall
	{
		public string	Query { get; set; }
		public int		Skip { get; set; } = 0;
		public int		Take { get; set; } = int.MaxValue;
		
		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{	
				return sun.ResourceHub.Releases	.Where(i => i.Address.ToString().Contains(Query))
												.GroupBy(i => i.Address)
												.Skip(Skip)
												.Take(Take)
												.Select(i => new LocalResource{	Resource = i.Key,
																				Latest = sun.ResourceHub.Find(i.Key, null).Hash,
																				Releases = i.Count()});
			}
		}
	}

	public class LocalResource
	{
		public ResourceAddress		Resource  { get; set; }
		public int					Releases { get; set; }
		public byte[]				Latest { get; set; }
	}
	
	public class LocalReleasesCall : SunApiCall
	{
		public ResourceAddress		Resource { get; set; }
		
		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{	
				return sun.ResourceHub.Releases	.Where(i => i.Address == Resource)
												.Select(i => new LocalRelease {	Availability = i.Availability,
																				Hash = i.Hash,
																				Type = i.Type });
			}
		}
	}

	public class LocalRelease
	{
		public ResourceType		Type { get; set; }
		public Availability		Availability { get; set; }
		public byte[]			Hash { get; set; }
	}
}
