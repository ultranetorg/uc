using Uccs.Net;
using Xunit;

namespace Uccs.Tests;

public static class Extentions
{
	[Fact]
	public static void General()
	{
		var s = new string(Enumerable.Range(0, 65536).Select(i => (char)(i)).ToArray());

		Assert.Equal(s, s.EscapeFilePath().UnescapeFilePath());
	}
}
