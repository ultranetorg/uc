using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RocksDbSharp;

namespace Uccs.Net
{
	public class IznSettings : NodeSettings
	{
		public IznSettings()
		{
		}

		public IznSettings(string profile) : base(profile)
		{
		}
	}

	public class InterzoneNode : Node
	{
		public override long					Roles => 0;
		public override Dictionary<Guid, long>	Zones => Nodes.ToDictionary(i => i.Zone.Id, i => i.Roles);

		public List<Node>						Nodes = [];
		//public Clock							Clock;
		public Node								FindMcv(Guid id) => Nodes.Find(i => i.Zone.Id == id);
		public T								Find<T>() where T : Node => Nodes.Find(i => i.GetType() == typeof(T)) as T;

		public InterzoneNode(string name, Guid zuid, string profile, IznSettings settings, Flow workflow) : base(name, settings ?? new IznSettings(Path.Join(profile, zuid.ToString())), workflow)
		{
			Zone = Interzone.Byid(zuid);
		}

		public override string ToString()
		{
			return string.Join(",", new string[] {	Connections().Count() < Settings.Peering.PermanentMin ? "Low Peers" : null,
													Settings.IP != null ? IP.ToString() : null}
													.Where(i => !string.IsNullOrWhiteSpace(i)));
		}

		protected override void CreateTables(ColumnFamilies columns)
		{
		}

		public void Connect(Node m)
		{
			Nodes.Add(m);

			foreach(var c in Connections())
			{
				c.Post(new PeersBroadcastRequest {Peers = [new Peer {IP = Settings.IP, Zones = Zones}]});
			}
		}

		public void Disconnect(Node m)
		{
			Nodes.Remove(m);
		}

	}
}
