﻿using Xunit;

namespace Uccs.Tests;

public static class XonTest
{
	[Fact]
	public static void Basic()
	{
		var x = new Xon(@"a b="""" c=""2 3 4""");
	}

	[Fact]
	public static void Custom()
	{
		var x = new Xon($@"	Title = ""Uos""
							Realization = _uo/uos/dotnet
							{{
								Channel = Prototype
							}}
							Realization = _uo/uos/winx64
							{{
								Channel = Prototype
								Condition
								{{
									AND
									{{
										""=="" {{F Windows}}
										""=="" {{B Windows/MicrosoftWindows}}
										"">="" {{V Windows/MicrosoftWindows/NT_6_1}}
									}}
								}}
							}}
							");
	}
}
