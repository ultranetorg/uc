using Xunit;

namespace Uccs.Tests;

public class Bech32Tests
{
 	[Fact]
 	public static void Main()
 	{
		var r = new Random();
		var h = new byte[32];

		for(int i = 0; i < 100; i++)
		{
			r.NextBytes(h);
	
			Assert.True(Bech32m.Decode("test", Bech32m.Encode("test", h)).SequenceEqual(h));
		}
 	}

}
