using System;

namespace TestAutomation;

[AttributeUsage(AttributeTargets.Class)]
public class TestTypeAttribute : Attribute
{
	public TestType TestType { get; }

	public TestTypeAttribute(TestType testType)
	{
		TestType = testType;
	}
}

[AttributeUsage(AttributeTargets.Class)]
public class TestNetworkAttribute : Attribute
{
	public TestNetwork TestNetwork { get; }

	public TestNetworkAttribute(TestNetwork testNet)
	{
		TestNetwork = testNet;
	}
}