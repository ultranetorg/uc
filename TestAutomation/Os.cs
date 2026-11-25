using System;
using System.Net;
using System.Collections.Generic;

using Uccs.Net;
using Uccs.Nexus;
using O;
using System.IO;
using Uccs.Rdn;

namespace TestAutomation;

public class Os
{
	public delegate void Delegate(Os d);

	public string Name;
	public string Profile;
	public IPAddress IP;

	public Nexus Nexus;
	public List<McvNode> Nodes = [];
	public NodeDelegate NodeStarted;

	public Delegate Stopped;

	public McvNode FindNode(string netName) => Nodes.Find(i => i.Net.Name == netName);
	public T FindNode<T>() where T : McvNode => Nodes.Find(i => i is T) as T;
	public Dictionary<McvNode, McvCli> Clis = [];

	public Os(string name)
	{
		Name = name;
	}

	public void CreateSession(string username)
	{
		lock(Nodes[0].Peering.Lock)
			Nodes[0].Peering.CreateSession(Name, username, null);
	}

	public override string ToString()
	{
		return Name;
	}

	public string GetProfileDirPath(TestBase test)
	{
		return Path.Combine(G.Developer.Tmp, test.GetType().Name, Name);
	}

	//public LocalResource GetLocalResourceByUra(string ura)
	//{
	//	return FindNode<RdnNode>().ResourceHub.Find(Ura.Parse(ura));
	//}

	//public LocalResource GetLocalResourceByUra(Ura ura)
	//{
	//	return FindNode<RdnNode>().ResourceHub.Find(ura);
	//}

	public LocalRelease GetLocalResourceByUrn(string urn)
	{
		return FindNode<RdnNode>().ResourceHub.Find(Urn.Parse(urn));
	}

	public LocalRelease GetLocalResourceByUrn(Urn urn)
	{
		return FindNode<RdnNode>().ResourceHub.Find(urn);
	}

	/// <summary> Stops all nodes and the Nexus. Keeps going if one of them fails and throws the collected errors at the end. </summary>
	public void Stop()
	{
		List<Exception> errors = [];

		foreach(var j in Nodes)
		{
			try { j.Stop(); }
			catch(Exception e) { errors.Add(e); }
		}

		try { Nexus?.Stop(); }
		catch(Exception e) { errors.Add(e); }

		Stopped?.Invoke(this);

		if(errors.Count > 0)
			throw new AggregateException($"Failed to stop {Name}.", errors);
	}
}