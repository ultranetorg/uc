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
		public byte[]			Release { get; set; }
		public ResourceType		Type { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{
				var r = sun.ResourceHub.Find(Release);

				if(r == null)
				{
					r = sun.ResourceHub.Add(Release, Type);
				}
			
				if(Type == ResourceType.File)
				{
					sun.ResourceHub.DownloadFile(r, "f", Release, null, workflow);
					return r.Hash;
				}
				else if(Type == ResourceType.Directory)
				{
					sun.ResourceHub.DownloadDirectory(r, workflow);
					return r.Hash;
				}
			}
	
			return null;
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
				return sun.ResourceHub.Resources.Where(i => i.Address.ToString().Contains(Query)).Skip(Skip).Take(Take);
			}
		}
	}
	
	public class LocalReleasesCall : SunApiCall
	{
		public ResourceAddress		Resource { get; set; }

		public class Item
		{
			public ResourceType		Type { get; set; }
			public Availability		Availability { get; set; }
			public byte[]			Hash { get; set; }
		}
		
		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{	
				return sun.ResourceHub.Resources.Find(i => i.Address == Resource).Datas
												.Select(i =>{ 
																var r = sun.ResourceHub.Find(i);
																return new Item {Hash = r.Hash, Type = r.Type, Availability = r.Availability};
															});
			}
		}
	}
}
