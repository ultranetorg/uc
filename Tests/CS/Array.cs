using Uccs.Net;
using Xunit;

namespace Uccs.Tests;

public static class Array
{
	static void Same(int[] a, int[] b) => Assert.True(a.SequenceEqual(b));

	[Fact]
	public static void Main()
	{
		var a = new int[] {0, 1, 2, 3, 4, 5};
		Same(a.RemoveAt(0), [1,2,3,4,5]);
		Same(a.RemoveAt(5), [0,1,2,3,4]);
		Same(a.RemoveAt(1), [0,2,3,4,5]);
		Same(a.RemoveAt(4), [0,1,2,3,5]);
		Same(a.RemoveAt(2), [0,1,3,4,5]);
		Same(new int[]{1}.RemoveAt(0), []);
	}
}
