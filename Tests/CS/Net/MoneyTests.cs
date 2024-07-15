using System;
using Uccs.Net;
using Xunit;

namespace Uccs.Tests
{
	public static class MoneyTests
	{
		[Fact]
		public static void Main()
		{
			Assert.Equal(new Money(0), Money.Zero);

			Assert.True("0.001" == new Money(0.001).ToString());
			Assert.True("1" == new Money(1).ToString());
			

			void pts1(string a)
			{ 
				Assert.True(a == Money.Parse(a).ToString());
				Assert.True('-' + a == Money.Parse('-' + a).ToString());
			}

			void pts2(string a, string b)
			{ 
				Assert.True(Money.Parse(a).ToString() == b);
			}

			pts1("0.000000000000000001");
			pts1("1.001");
			pts1("0.001");
			pts1("123");

			pts2("-0", "0");
			pts2("0123", "123");
			pts2("123.000", "123");

			Assert.Throws<FormatException>(() => Money.Parse("0.0000000000000000001"));
			Assert.Throws<FormatException>(() => Money.Parse("11111111111111111111111111111111111111111111.0000000000000000001"));
		}

		//[Theory]
		//[MemberData(nameof(FromWei_TestData))]
		//public static void FromWei(BigInteger wei, Coin expected)
		//{
		//	Coin actual = Coin.FromWei(wei);
		//	Assert.Equal(expected, actual);
		//}

		//public static IEnumerable<object[]> FromWei_TestData()
		//{
		//	yield return new object[] { 1_002, new Coin() };
		//	yield return new object[] { 1_002, new Coin() };
		//	yield return new object[] { 1_002, new Coin() };
		//}

		//[Fact]
		//public static void Ctor_Int()
		//{
		//}

		//[Fact]
		//public static void Ctor_Decimal()
		//{
		//}

		//[Fact]
		//public static void Ctor_Double()
		//{
		//}
	}
}
