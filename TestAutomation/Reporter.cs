using System;
using System.Xml;
using System.Diagnostics;
using System.Collections.Generic;

using CommonUtils;
using CommonUtils.Ext;
using System.Text;

namespace JunitReporter;

public static class Reporter
{
	static List<TestSuiteRecord> TestSuites = [];
	static TestSuiteRecord CurrentTestSuite;
	static TestRecord? CurrentTest;
	class TestSuiteRecord
	{
		public string Name = string.Empty;
		public DateTime Timestamp;
		public List<TestRecord> Tests = [];
	}
	class TestRecord
	{
		public string Name = string.Empty;
		public string ClassName = string.Empty;
		public string Description = string.Empty;
		public TimeSpan ExecutionTime;
		public string? FailMessage;
		public Exception? Exception;

		public string SystemOut = "";
		public string SystemErr = "";
	}

	public static Action? OnEndTest = null;
	public static bool LogToConsole = true;

	static readonly Stopwatch time = new();

	static XmlWriter? w = null;
	static XmlWriterSettings settings = new XmlWriterSettings
	{
		Indent = true,              // Enable indentation
		NewLineOnAttributes = false, // New line for attributes
		OmitXmlDeclaration = false  // Include XML declaration
	};

	public static void BeginSuite(string suitName)
	{
		CurrentTestSuite = new TestSuiteRecord() { Name = suitName, Timestamp = DateTime.Now };
		TestSuites.Add(CurrentTestSuite);
	}

	public static void BeginTest(string name, string className, string desc)
	{
		if (CurrentTestSuite == null) throw new Exception("BeginTest() should be called only after BeginTestSuite() because a test is a part of a test suite.");

		CurrentTest = new() { Name = name, ClassName = className, Description = desc };
		CurrentTestSuite.Tests.Add(CurrentTest);

		time.Reset();
		time.Start();

		Console.ForegroundColor = ConsoleColor.Blue;
		Console.WriteLine($"Starting test class: {CurrentTest.ClassName}; Test name: {CurrentTest.Name}");
		Console.ResetColor();

	}

	public static void Report(Exception? e = null, string ? failMsg = null)
	{
		if (CurrentTest == null) throw new Exception("Report() should be called only after BeginTest() because report is being done on the current test.");

		CurrentTest.ExecutionTime = time.Elapsed;
		CurrentTest.Exception = e;
		CurrentTest.FailMessage = failMsg;

		if (CurrentTest.Exception != null)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("Failed! Time: {0}s; Test: \"{1}\"; Message: \"{2}\";", CurrentTest.ExecutionTime.ToString(), CurrentTest.Name, CurrentTest.FailMessage);
			Console.Write(" Stack:{0}\n", CurrentTest.Exception.GetFullStack());
			Console.ResetColor();
		}
		else
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Passed! Time: {0}s; Test: \"{1}\";", CurrentTest.ExecutionTime.ToString(), CurrentTest.Name);
			Console.ResetColor();
		}

	}

	public static void Message(StandardOutputStreamType type, string msg, ConsoleColor? color = null)
	{
		if (CurrentTest == null) throw new Exception("You can call 'Message()' only after 'BeginTest()'.");

		if (type == StandardOutputStreamType.Out) CurrentTest.SystemOut += msg + "\n";
		if (type == StandardOutputStreamType.Err) CurrentTest.SystemErr += msg + "\n";

		Console.ForegroundColor = color ?? (type == StandardOutputStreamType.Out ? ConsoleColor.Cyan : ConsoleColor.Red);
		Console.WriteLine(msg);
		Console.ResetColor();
	}

	static public void Write(string filePath)
	{
		w = XmlWriter.Create(filePath, settings);
		w.WriteStartDocument();
		w.WriteStartElement("testsuites");

		foreach (TestSuiteRecord ts in TestSuites)
		{
			w.WriteStartElement("testsuite");
			w.WriteAttributeString("name", ts.Name);
			w.WriteAttributeString("timestamp", DateTime.Now.ToString());

			foreach (TestRecord tr in ts.Tests)
			{
				w.WriteStartElement("testcase");
				w.WriteAttributeString("name", tr.Name);
				w.WriteAttributeString("className", tr.ClassName);
				w.WriteAttributeString("time", tr.ExecutionTime.TotalSeconds.ToString() + "s");

				Console.Write($"{tr.ClassName} ");

				if (tr.Description != string.Empty)
				{
					w.WriteAttributeString("description", tr.Description);
					// w.WriteStartElement("property");
					// w.WriteAttributeString("name", "description");
					// w.WriteString(tr.Description);
					// w.WriteEndElement(); // Close "property"
				}

				if (!string.IsNullOrEmpty(tr?.SystemOut))
					w.WriteElementString("system-out", tr.SystemOut.PrepXml());
				if (!string.IsNullOrEmpty(tr?.SystemErr))
					w.WriteElementString("system-err", tr.SystemErr.PrepXml());

				if (tr?.Exception != null)
				{
					w.WriteStartElement("failure");
					if (tr.FailMessage != null) w.WriteAttributeString("message", tr.FailMessage);
					w.WriteString(tr.Exception.GetFullStack());
					w.WriteEndElement(); // Close "failure"
					Console.ForegroundColor = ConsoleColor.DarkRed;
					ConsoleWriteLineColored("FAILED", ConsoleColor.Red);
				} else ConsoleWriteLineColored("PASSED", ConsoleColor.Green);

				w.WriteEndElement(); // Close "testcase"
			}

			w.WriteEndElement(); // Close "testsuite"
		}

		w.WriteEndElement(); // Close "testsuites"
		w.WriteEndDocument();
		w.Close();
	}

	public static string ToUTF8(string s)
	{
		byte[] bytes = Encoding.Default.GetBytes(s);
		return Encoding.UTF8.GetString(bytes);
	}

	public static void ConsoleWriteLineColored(string s, ConsoleColor fg)
	{
		Console.ForegroundColor = fg;
		Console.WriteLine(s);
		Console.ResetColor();
	}

}
