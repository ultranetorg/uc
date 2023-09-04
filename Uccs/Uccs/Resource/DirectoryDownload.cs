using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Uccs.Net
{
	public class DirectoryDownload
	{
		public ResourceAddress		Address;
		public bool					Succeeded;
		public Queue<Xon>			Files = new();
		public int					CompletedCount;
		public List<FileDownload>	CurrentDownloads = new();
		public Task					Task;
		public SeedCollector		SeedCollector;

		public DirectoryDownload(ResourceAddress address, Sun sun,  Workflow workflow)
		{
			Address = address;

			void run()
			{
				try
				{
					var h = sun.Call(c => c.FindResource(address), workflow).Resource.Data;
		 									
					SeedCollector = new SeedCollector(sun, h, workflow);
	
					sun.Resources.GetFile(address, h, ".index", h, SeedCollector, workflow);
	
											
					var index = new XonDocument(sun.Resources.ReadFile(address, h, ".index"));
	
					void enumearate(Xon xon)
					{
						if(xon.Parent != null && xon.Parent.Name != null)
							xon.Name = xon.Parent.Name + "/" + xon.Name;
	
						if(xon.Value != null)
						{
							Files.Enqueue(xon);
						}
	
						foreach(var i in xon.Nodes)
						{
							enumearate(i);
						}
					}
	
					enumearate(index);
	
					do 
					{
						if(CurrentDownloads.Count < 10 && Files.Any())
						{
							var f = Files.Dequeue();
	
							lock(sun.Resources.Lock)
							{
								var dd = sun.Resources.DownloadFile(address, h, f.Name, f.Value as byte[], SeedCollector, workflow);
	
								if(dd != null)
								{
									CurrentDownloads.Add(dd);
								}
							}
						}
	
						var i = Task.WaitAny(CurrentDownloads.Select(i => i.Task).ToArray());
	
						if(CurrentDownloads[i].Succeeded)
						{
							CompletedCount++;
							CurrentDownloads.Remove(CurrentDownloads[i]);
						}
					}
					while(Files.Any() && workflow.Active);
	
					SeedCollector.Stop();
	
					lock(sun.Resources.Lock)
					{
						Succeeded = true;
						sun.Resources.DirectoryDownloads.Remove(this);
					}
				}
				catch(OperationCanceledException)
				{
				}
			}

			Task = Task.Run(run, workflow.Cancellation.Token);
		}

		public override string ToString()
		{
			return Address.ToString();
		}
	}
}
