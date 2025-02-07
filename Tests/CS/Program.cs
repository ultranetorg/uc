namespace Uccs.Tests;


class Program
{
	public static void Main(string[] args)
	{
		//IdTests.Main();
		//MoneyTests.Main();
		//AccountKeyTests.Main();
		//XonTest.Custom();
		ECTests.Move();

		//UraTests.UAddresses();
		//UraTests.Resource();
		//UraTests.Package();
		//UraTests.Release();


		//var ja = JsonSerializer.Serialize(new RdcCall {Request = new AuthorRequest()}, JsonApiClient.Options);

		//var ja = JsonSerializer.Serialize(new A{Progress = new FileDownloadProgress()}, JsonApiClient.Options);
		//var jb = JsonSerializer.Serialize(new A{Progress = new ReleaseDownloadProgress()}, JsonApiClient.Options);



//			var sun = new Sun(Net.Localzone, new Settings() {Profile = $"{G.Dev.Tmp}\\Tests" }, null);




//// 			var rq = new MembersResponse()
//// 			{
//// 				Id = 123,
//// 				Error = new NodeException(NodeError.AllNodesFailed),
//// 				Members = new MembersResponse.Member[] {new MembersResponse.Member{ Account = AccountAddress.Zero, 
//// 																					BaseRdcIPs = new IPAddress[] {IPAddress.Parse("1.1.1.1") },
//// 																					SeedHubRdcIPs = new IPAddress[] {IPAddress.Parse("1.1.1.1") },
//// 																					CastingSince = 345,
//// 																					Proxyable = true
//// 																					}}
//// 			};
//
//			var rq = new AuthorResponse{};
//			
//			var s = new MemoryStream();
//			var w = new BinaryWriter(s);
//			var r = new BinaryReader(s);
//
//			BinarySerializator.Serialize(w, rq);
//
//			s.Position = 0;
//
//			rq = BinarySerializator.Deserialize<AuthorResponse>(r, sun.Constract);
	}
}
