using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Uccs.Net
{
	public class DirectoryDownload
	{
		public LocalRelease			Release;
		public bool					Succeeded;
		public Queue<Xon>			Files = new();
		public int					CompletedCount;
		public int					TotalCount;
		public List<FileDownload>	CurrentDownloads = new();
		public Task					Task;
		public SeedCollector		SeedCollector;

		public DirectoryDownload(Sun sun, LocalRelease release, Workflow workflow)
		{
			Release = release;
			Release.Activity = this;
			SeedCollector = new SeedCollector(sun, release.Hash, workflow);

			void run()
			{
				try
				{
					sun.ResourceHub.GetFile(release, ".index", release.Hash, SeedCollector, workflow);
												
					var index = new XonDocument(release.ReadFile(".index"));
	
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
	
					TotalCount = Files.Count;

					do 
					{
						if(CurrentDownloads.Count < 10 && Files.Any())
						{
							var f = Files.Dequeue();
	
							lock(sun.ResourceHub.Lock)
							{
								var dd = sun.ResourceHub.DownloadFile(release, f.Name, f.Value as byte[], SeedCollector, workflow);
	
								if(dd != null)
								{
									CurrentDownloads.Add(dd);
								}
							}
						}
	
						var i = Task.WaitAny(CurrentDownloads.Select(i => i.Task).ToArray(), workflow.Cancellation);
	
						if(CurrentDownloads[i].Succeeded)
						{
							CompletedCount++;
							CurrentDownloads.Remove(CurrentDownloads[i]);
						}
					}
					while(Files.Any() && workflow.Active);
	
					SeedCollector.Stop();
	
					lock(sun.ResourceHub.Lock)
					{
						Succeeded = true;
						release.Complete(Availability.Full);
					}
				}
				catch(Exception) when(workflow.Aborted)
				{
				}
				finally
				{
					lock(sun.ResourceHub.Lock)
						Release.Activity = null;
				}
			}

			Task = Task.Run(run, workflow.Cancellation);
		}

		public override string ToString()
		{
			return Release.Hash.ToHex();
		}
	}
}
