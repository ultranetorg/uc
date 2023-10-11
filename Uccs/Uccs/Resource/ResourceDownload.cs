using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Uccs.Net
{
	public class FileDownloadProgress
	{
		public FileDownloadProgress()
		{
		}

		public FileDownloadProgress(FileDownload file)
		{
			Path				= file.File != null ? file.File.Path : null;
			Length				= file.File != null ? file.File.Length : -1;
			DownloadedLength	= file.File != null ? file.DownloadedLength : -1;
		}

		public string	Path { get; set; }
		public long		Length { get; set; }
		public long		DownloadedLength { get; set; }
	}

	public class ResourceDownloadProgress
	{
		public class Hub
		{
			public AccountAddress	Member { get; set; }
			public HubStatus		Status { get; set; }
		}

		public class Seed
		{
			public IPAddress	IP { get; set; }
			public int			Failures { get; set; }
			public int			Succeses { get; set; }
		}

		public IEnumerable<Hub>						Hubs { get; set; }
		public IEnumerable<FileDownloadProgress>	CurrentFiles { get; set; }
		public IEnumerable<Seed>					Seeds { get; set; }
		public bool									Succeeded  { get; set; }

		public ResourceDownloadProgress()
		{
		}

		public ResourceDownloadProgress(SeedCollector seedCollector)
		{
			Hubs	= seedCollector.Hubs.Select(i => new Hub {Member = i.Member, Status = i.Status}).ToArray();
			Seeds	= seedCollector.Seeds.Select(i => new Seed {IP = i.IP}).ToArray();
		}

		public override string ToString()
		{
			return $"H={{{Hubs.Count()}}}, S={{{Seeds.Count()}}}, F={{{CurrentFiles.Count()}}}, {string.Join(", ", CurrentFiles.Select(i => $"{i.Path}={i.DownloadedLength}/{i.Length}"))}";
		}
	}
}
