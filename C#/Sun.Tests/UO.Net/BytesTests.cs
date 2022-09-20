using FluentAssertions;
using Xunit;

namespace UC.Net.Node.Tests
{
	public static class BytesTests
	{
		[Theory]
		[InlineData(new byte[] { 111, 112, 71 }, new byte[] { 85 }, new byte[] { 58, 0, 0 })]
		[InlineData(new byte[] { 113 }, new byte[] { 67, 76, 189 }, new byte[] { 50, 0, 0 })]
		[InlineData(new byte[] { 45, 97, 51 }, new byte[] { 24, 13, 211 }, new byte[] { 53, 108, 224 })]
		[InlineData(new byte[] { 25, 99, 255, 0, 15 }, new byte[] { 255, 18, 0, 1 }, new byte[] { 230, 113, 255, 1, 0 })]
		[InlineData(new byte[] { 255, 0, 255 }, new byte[] { 0, 255, 0, 255 }, new byte[] { 255, 255, 255, 0 })]
		public static void Xor_InputDataSpecified_XoredValuesExpected(byte[] a, byte[] b, byte[] expected)
		{
			byte[] result = Bytes.Xor(a, b);
			result.Should().BeEquivalentTo(expected);
		}
	}
}
