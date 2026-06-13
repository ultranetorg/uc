using Uccs.Net;
using Uccs.Nexus;
using Xunit;

namespace Uccs.Tests;

public static class PlatformExpressionTest
{
	[Fact]
	public static void Main()
	{
		if(Environment.MachineName == "M1")
		{
			Assert.True(new Expression(Expression.Equal,			[new (Expression.Family.ToString()), new ($"{Family.Windows}")]).Match(Platform.Current));
			Assert.False(new Expression(Expression.Equal,			[new (Expression.Family.ToString()), new ($"{Family.Android}")]).Match(Platform.Current));
			
			Assert.True(new Expression(Expression.Equal,			[new (Expression.Brand.ToString()), new ($"{Family.Windows}/{WindowsBrand.MicrosoftWindows}")]).Match(Platform.Current));
			Assert.False(new Expression(Expression.Equal,			[new (Expression.Brand.ToString()), new ($"{Family.Windows}/{WindowsBrand.MicrosoftWindowsServer}")]).Match(Platform.Current));
			
			Assert.True(new Expression(Expression.Equal,			[new ($"{Expression.Version}"), new ($"{Family.Windows}/{WindowsBrand.MicrosoftWindows}/{MicrosoftWindowsVersion.NT_10_0_19044}")]).Match(Platform.Current));
			Assert.True(new Expression(Expression.GreaterOrEqual,	[new ($"{Expression.Version}"), new ($"{Family.Windows}/{WindowsBrand.MicrosoftWindows}/{MicrosoftWindowsVersion.NT_10_0_19044}")]).Match(Platform.Current));
			Assert.True(new Expression(Expression.LessOrEqual,		[new ($"{Expression.Version}"), new ($"{Family.Windows}/{WindowsBrand.MicrosoftWindows}/{MicrosoftWindowsVersion.NT_10_0_19044}")]).Match(Platform.Current));
			Assert.True(new Expression(Expression.Greater,			[new ($"{Expression.Version}"), new ($"{Family.Windows}/{WindowsBrand.MicrosoftWindows}/{MicrosoftWindowsVersion.NT_10_0_19043}")]).Match(Platform.Current));
			Assert.True(new Expression(Expression.Less,				[new ($"{Expression.Version}"), new ($"{Family.Windows}/{WindowsBrand.MicrosoftWindows}/{MicrosoftWindowsVersion.NT_10_0_19045}")]).Match(Platform.Current));
			Assert.False(new Expression(Expression.Equal,			[new ($"{Expression.Version}"), new ($"{Family.Windows}/{WindowsBrand.MicrosoftWindows}/{MicrosoftWindowsVersion._1_01}")]).Match(Platform.Current));
			
			Assert.True(new Expression(Expression.Equal,			[new (Expression.Architecture.ToString()), new ($"{Architecture.x86_64}")]).Match(Platform.Current));
			Assert.False(new Expression(Expression.Equal,			[new (Expression.Architecture.ToString()), new ($"{Architecture.x86_32}")]).Match(Platform.Current));

			Assert.Throws<ArgumentException>(() => new Expression(Expression.Equal,	[new ($"{Expression.Version}"), new ($"xxxxxxxxxxxxxxxxxxxxxxxx/{WindowsBrand.MicrosoftWindows}/{MicrosoftWindowsVersion.NT_10_0_19044}")]).Match(Platform.Current));
			Assert.Throws<ArgumentException>(() => new Expression(Expression.Equal,	[new ($"{Expression.Version}"), new ($"{Family.Windows}/xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx/{MicrosoftWindowsVersion.NT_10_0_19044}")]).Match(Platform.Current));
			Assert.Throws<ArgumentException>(() => new Expression(Expression.Equal,	[new ($"{Expression.Version}"), new ($"{Family.Windows}/{WindowsBrand.MicrosoftWindows}/xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")]).Match(Platform.Current));
		}
	}
}
