using System.Net;

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

	public class InterPeer : IBinarySerializable
	{
		public IPAddress	IP {get; set;}
		public long			Roles {get; set;}

		public void Read(BinaryReader reader)
		{
			IP = reader.ReadIPAddress();
			Roles = reader.Read7BitEncodedInt64();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(IP);
			writer.Write7BitEncodedInt64(Roles);
		}
	}

	public class NetPeers
	{
		public Guid				Net {get; set;}
		public List<InterPeer>	Peers {get; set;}
	}

	public class NexusNode : Node
	{
		public List<Node>						Nodes = [];
		public List<NetPeers>					Nets = [];
		public override long					Roles => 0;
		bool									MinimalPeersReached;

		//public Clock							Clock;
		public Node								FindMcv(Guid id) => Nodes.Find(i => i.Net.Id == id);
		public T								Find<T>() where T : Node => Nodes.Find(i => i.GetType() == typeof(T)) as T;

		public NexusNode(string name, Guid zuid, string profile, IznSettings settings, Flow workflow) : base(name, Nexus.Byid(zuid), settings ?? new IznSettings(profile), workflow)
		{
			if(Settings.Api != null)
			{
				ApiServer = new NodeApiServer(this, Flow);
			}

			RunPeer();
		}

		public override string ToString()
		{
			return string.Join(",", new string[] {	Connections.Count() < Settings.Peering.PermanentMin ? "Low Peers" : null,
													Settings.Peering.IP?.ToString()}.Where(i => !string.IsNullOrWhiteSpace(i)));
		}

		protected override void Share(Peer peer)
		{
			peer.Post(new ShareNetsRequest {	Broadcast = false,
												Nets = Nets.Select(i => new ShareNetsRequest.Z {	Id = i.Net, 
																									Peers = i.Peers.ToArray()}).ToArray()});
		}

		public NetPeers GetNet(Guid id)
		{
			if(Nets.Find(i => i.Net == id) is not NetPeers z)
			{ 
				z = new NetPeers {Net = id, Peers = [] };
				Nets.Add(z);
			}

			return z;
		}

		public void Connect(Node node)
		{
			Nodes.Add(node);

			var z = GetNet(node.Net.Id);
			var p = new InterPeer {IP = Settings.Peering.IP ?? IP, Roles = node.Roles};
			z.Peers.Add(p);

			foreach(var c in Connections)
			{
				c.Post(new ShareNetsRequest {Broadcast = true, Nets = [new ShareNetsRequest.Z {Id = node.Net.Id, Peers = [p]}]});
			}
		}

		public void Disconnect(Node m)
		{
			Nodes.Remove(m);
		}

		protected override void ProcessConnectivity()
		{
			base.ProcessConnectivity();

			if(!MinimalPeersReached && 
				Connections.Count(i => i.Permanent) >= Settings.Peering.PermanentMin)
			{
				MinimalPeersReached = true;
				Flow.Log?.Report(this, $"Minimal peers reached");
			}
		}
	}
}
