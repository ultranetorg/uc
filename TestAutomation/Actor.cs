using System;
using System.Linq;
using System.Threading;
using Uccs.Fair;
using Uccs.Net;
using System.Diagnostics;

namespace TestAutomation;

public class Actor
{
	public readonly Os Os;
	public User User { get; private set; }

	public Actor(Os os, User u)
	{
		Os = os;
		User = u;
	}

	public void Update(TestBase currentTest) 
	{
		User = currentTest.FindUser(User.Name);
	}
}