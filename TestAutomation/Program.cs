using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CommonUtils.Ext;
using JunitReporter;
using O;
using TestAutomation.Tests;
using TestAutomation.Tests.Fair;
using TestAutomation.Tests.Rdn;


namespace TestAutomation;

// UO_TA_LOGECHO
// "all" - print called commands and their outputs
// "input" - print only called commands

// TODO : Need to introduce some kind of generic test method to run after each particular test was ran,
//		  to ensure that network did not crash.


/// <summary>
/// This is a test launcher. Classes that represent tests should indirectly inherit 
/// 'TestBase' class to be launched as a tests. Using reflection we get all these test 
/// classes, one by one, we instantiate them and run their `Init()` and then `Logic()` 
/// methods. `Init()` of these test classes acts like a test preparation step, where
/// we set-up test data and conditions. This approach lets us isolate tests very well 
/// and have an ability to set-up a test context easily. It also allows to structure 
/// project nicely. Tests can have different "types" defined in the 'TestType' enum 
/// located inside of 'TestAutomation' namespace. You can assign a tests type to a test 
/// by adding a TestTypeAttribute to the test class, e.g. Right now there are "Generic", 
/// "CommandLine", "EndToEnd" and "TestSet" test types, but there could be more in future 
/// or they can change.
/// <br/> <br/>
/// 
/// Example. <br/>
/// Here we define a test class `AccountUtilityTransfer` that tests utility transfer 
/// functionality:
///  <br/>  <br/>
///  
/// [TestType(TestType.CommandLine)]			<br/> 
/// class AccountUtilityTransfer : FairTest		<br/>
/// {											<br/>
/// 	public override void Init() { .. }		<br/>
/// 	public void Logic() { .. }				<br/>
/// }											<br/>
/// 
/// <br/><br/>
/// 
/// We add a test type attribute with a `TestType.CommandLine` argument, marking this test 
/// as a command line test, meaning it tests functionality through command line. We inherit 
/// our test from `FairTest` class, that in turn inherits 'TestBase' class. Our test 
/// launcher detects everything that indirectly inherits 'TestBase' as a test. We implement 
/// `Init()` and `Logic()` methods that our test launcher expectes test classes to have.
/// </summary>
class Program
{
	//static List<Type> Only = [];
	static List<Type> Only = [typeof(FeeValidation)];
	static List<Type> TestsToSkip = [];

	static int TimeoutSec = 60;

	static void Main(string[] args)
	{
		// Pull test step execution timeout from env variable, if it is provided.
		string? timeout = Environment.GetEnvironmentVariable("UO_TA_TIMEOUT");
		if(timeout != null)
			int.TryParse(timeout, out TimeoutSec);

		/* Ultranet network nodes and vault needs a place to store it's data.
		It is stored in the `G.Developer.Tmp` path, so before we run new 
		iteration of tests - we want to clean old data to prevent conflicts. */
		new DirectoryInfo(G.Developer.Tmp).Empty();
		Assembly a = Assembly.GetAssembly(typeof(Program)) ?? throw new Exception("Could not acquaire current assembly");

		// Select only classes that are tests - ones that are indirectly inherited from `TestBase` class.
		List<Type> tests = [];
		foreach(Type t in a.GetTypes())
		{
			if(t.IsSubclassOf(typeof(TestBase)) && !t.IsAbstract)
				tests.Add(t);
		}

		// Sort tests by TestType.
		Dictionary<TestType, List<Type>> testsByType = [];
		foreach(TestType tt in Enum.GetValues<TestType>()) testsByType.Add(tt, []); // Init dictionary of lists.
		foreach(Type t in tests)
		{
			TestTypeAttribute tta = t.GetCustomAttribute<TestTypeAttribute>() ??
				throw new Exception($"Could not check test type attribute of a test class '{t.Name}'. Maybe attribute is not set?");
			testsByType[tta.TestType].Add(t);
		}
		foreach(var keyval in testsByType) // Print which test were found to be of which test type.
		{
			Console.WriteLine($"{keyval.Value.Count} {keyval.Key} tests:");
			foreach(Type t in keyval.Value) Console.Write(t.Name + " ");
			Console.Write("\n\n");
		}

		// Sort tests by tested network.
		Dictionary<TestNetwork, List<Type>> testsByNet = [];
		foreach(TestNetwork tt in Enum.GetValues(typeof(TestNetwork))) testsByNet.Add(tt, []); // Init dictionary of lists.
		foreach(Type t in tests)
		{
			TestNetworkAttribute tta = t.GetCustomAttribute<TestNetworkAttribute>() ??
				throw new Exception($"Could not check test network attribute of a test class '{t.Name}'. Maybe attribute is not set?");
			testsByNet[tta.TestNetwork].Add(t);
		}
		foreach(var p in testsByNet) // Print which tests were found to be related to which network.
		{
			Console.WriteLine($"{p.Value.Count} {p.Key} tests:");
			foreach(Type t in p.Value) Console.Write(t.Name + " ");
			Console.Write("\n\n");
		}

		GetTestsToRunAndSkipFromEnv();

		// Run tests, grouping by test type.
		DateTime startTime = DateTime.Now;
		foreach(TestType tt in Enum.GetValues<TestType>())
		{
			Reporter.BeginSuite(tt.ToString());
			foreach(Type t in testsByType[tt])
				RunTest(t);
		}
		TimeSpan time = DateTime.Now - startTime;
		Console.WriteLine($"TOTAL TIME: {time}");

		Reporter.Write("Result.xml");
	}

	///<summary> This gets tests to skip and to run only from environment variables UO_TA_SKIP and UO_TA_ONLY
	/// and populates `TestsToSkip` and `Only` lists with them accordingly. </summary>
	static void GetTestsToRunAndSkipFromEnv()
	{
		string toSkip = Environment.GetEnvironmentVariable("UO_TA_SKIP");
		if(toSkip != null)
		{
			string[] toSkipArr = toSkip.Split(' ');
			foreach(string t in toSkipArr)
			{
				Type testType = Type.GetType(t);
				if(testType == null) continue;
				TestsToSkip.Add(testType);
			}
		}

		string only = Environment.GetEnvironmentVariable("UO_TA_ONLY");
		if(only != null)
		{
			string[] onlyArr = only.Split(' ');
			foreach(string t in onlyArr)
			{
				Type testType = Type.GetType(t);
				if(testType == null) continue;
				Only.Add(testType);
			}
		}
	}

	static void RunTest(Type testClass)
	{
		if(Only.Count != 0 && !Only.Contains(testClass)) return;
		if(TestsToSkip.Contains(testClass)) return;

		ConstructorInfo constructor = testClass.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis, [], null) ??
			throw new Exception("Could not get constructor info");
		TestBase test = constructor.Invoke(null) as TestBase ??
			throw new Exception("Failed to construct test class.");

		Reporter.BeginTest(test.Name, testClass.Name, test.Description);

		try
		{
			int timeout = Debugger.IsAttached ? int.MaxValue : TimeoutSec;
			bool first = RunTimed(test.Init, timeout);

			if(!first) throw new Exception($"Test initialization timeout. Init() execution time exceeded {timeout} seconds.");
		}
		catch(Exception e)
		{
			string msg = "Failed to initalize test class.";
			Reporter.Report(e, msg);
			test.Abort();

			return;
		}

		try
		{
			MethodInfo method = testClass.GetMethod("Logic") ?? throw new Exception("Could not acquire method info");
			bool first = RunTimed(() =>
			{
				method.Invoke(test, null);
			}, Debugger.IsAttached ? int.MaxValue : TimeoutSec);

			if(!first) throw new Exception($"Test execution timeout. Test execution time exceeded {TimeoutSec} seconds.");

			Reporter.Report(); // Test passed successfully without exceptions
		}
		catch(Exception e)
		{
			Reporter.Report(e, e.Message);
		}

		// This should kill uoses and their underliyng tcp listeners if any, and free all the locks/ports, etc.
		test.Abort();
	}

	/// <summary> Returns `true` if an action ended execution earlier than a timeout, `false` otherwise </summary>
	static bool RunTimed(Action action, int timeoutSec)
	{
		Task mainTask = Task.Run(action);
		Task<Task> race = Task.WhenAny(mainTask, Task.Delay(timeoutSec <= 0 ? int.MaxValue : (timeoutSec > int.MaxValue / 1000 ? int.MaxValue : timeoutSec * 1000)));
		race.Wait();
		if(race.Result != mainTask) return false;
		if(race.Result.IsFaulted) throw race.Result.Exception;
		return true;
	}
}

