using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Net;

namespace UC.Net
{
	public abstract class ApiCall
	{
		public string			Version { get; set; }
		public string			AccessKey { get; set; }

		public static string NameOf<C>() => NameOf(typeof(C));
		public static string NameOf(Type type) => type.Name.Remove(type.Name.IndexOf("Call"));
	}

	public class ExitCall : ApiCall
	{
		public string			Reason { get; set; }
	}

	public class StatusCall : ApiCall
	{
		public int				Limit  { get; set; }
	}

	public class GetStatusResponse
	{
		public IEnumerable<string>	Log {get; set;}
		public IEnumerable<string>	Rounds {get; set;}
		public IEnumerable<string>	InfoFields {get; set;}
		public IEnumerable<string>	InfoValues {get; set;}
	}

	public class RunNodeCall : ApiCall
	{
	}

	public class AddWalletCall : ApiCall
	{
		public Account	Account { get; set; }
		public byte[]	Wallet { get; set; }
	}

	public class UnlockWalletCall : ApiCall
	{
		public Account	Account { get; set; }
		public string	Password { get; set; }
	}

	public class SetGeneratorCall : ApiCall
	{
		public Account			Account {get; set;}
	}

	public class TransferUntCall : ApiCall
	{
		public Account	From { get; set; }
		public Account	To { get; set; }
		public Coin		Amount { get; set; }
	}

	public class QueryReleaseCall : ApiCall
	{
		public IEnumerable<ReleaseQuery>	Queries { get; set; }
		public bool							Confirmed { get; set; }
	}
	
	public class QueryReleaseResult
	{
		public IEnumerable<XonDocument> Manifests { get; set; }
	}

	public class DownloadPackageCall : ApiCall
	{
		public PackageAddress	Package { get; set; }
	}

	public class DownloadStatusCall : ApiCall
	{
		public PackageAddress	Package { get; set; }
	}
}
