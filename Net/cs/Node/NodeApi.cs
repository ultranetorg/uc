using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Uccs.Net
{
	public abstract class NodeApc : Apc
	{
		public abstract object Execute(Node sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow);
	}

	public class PropertyApc : NodeApc
	{
		public string Path { get; set; }

		public override object Execute(Node sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			object o = sun;

			foreach(var i in Path.Split('.'))
			{
				o = o.GetType().GetProperty(i)?.GetValue(o) ?? o.GetType().GetField(i)?.GetValue(o);

				if(o == null)
					throw new NodeException(NodeError.NotFound);
			}

			switch(o)
			{
				case byte[] b:
					return b.ToHex();

				default:
					return o?.ToString();
			}
		}
	}

	public class ExceptionApc : NodeApc
	{
		public string Reason { get; set; }

		public override object Execute(Node sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			throw new Exception("TEST");
		}
	}

	public class ExitApc : NodeApc
	{
		public string Reason { get; set; }

		public override object Execute(Node sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			sun.Stop();
			return null;
		}
	}

	public class SettingsApc : NodeApc
	{
		public override object Execute(Node sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(sun.Lock)
				return new Response{ProfilePath	= sun.Settings.Profile, 
									Settings	= sun.Settings}; /// TODO: serialize
		}

		public class Response
		{
			public string		ProfilePath {get; set;}
			public NodeSettings		Settings {get; set;}
		}
	}

	public class LogReportApc : NodeApc
	{
		public int		Limit  { get; set; }

		public override object Execute(Node sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(sun.Lock)
				return new Response{Log = sun.Flow.Log.Messages.TakeLast(Limit).Select(i => i.ToString()).ToArray() }; 
		}

		public class Response
		{
			public IEnumerable<string> Log { get; set; }
		}
	}

	public class PeersReportApc : NodeApc
	{
		public int		Limit { get; set; }

		public override object Execute(Node sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(sun.Lock)
				return new Return{Peers = sun.Peers.Where(i => i.Status == ConnectionStatus.OK).TakeLast(Limit).Select(i =>	new Return.Peer {
																																				IP			= i.IP,			
																																				Status		= i.StatusDescription,
																																				PeerRank	= i.PeerRank,
																																				Roles		= i.Roles,
																																				LastSeen	= i.LastSeen,
																																				LastTry		= i.LastTry,
																																				Retries		= i.Retries	
																																			}).ToArray()}; 
		}

		public class Return
		{
			public class Peer
			{
				public IPAddress	IP { get; set; }
				public string		Status  { get; set; }
				public int			PeerRank { get; set; }
				public DateTime		LastSeen { get; set; }
				public DateTime		LastTry { get; set; }
				public int			Retries { get; set; }
				public long			Roles { get; set; }
			}

			public IEnumerable<Peer> Peers {get; set;}
		}
	}

	public class SummaryApc : NodeApc
	{
		public int		Limit  { get; set; }

		public override object Execute(Node sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(sun.Lock)
			{ 
				List<KeyValuePair<string, string>> f =
				[
					new ("Version",						sun.Version.ToString()),
					new ("Profile",						sun.Settings.Profile),
					new ("IP(Reported):Port",			$"{sun.Settings.Peering.IP} ({sun.IP}) : {sun.Settings.Peering.Port}"),
					new ("Votes Acceped/Rejected",		$"{sun.Statistics.AccpetedVotes}/{sun.Statistics.RejectedVotes}"),
				];

				if(sun is McvNode m)
				{
					f.Add(new ("Zone",  m.Zone.Name));
				}

				f.Add(new ("Generating (nps/μs)",	$"{sun.Statistics.Generating	.N}/{sun.Statistics.Generating	.Avarage.Ticks/10}"));
				f.Add(new ("Consensing (nps/μs)",	$"{sun.Statistics.Consensing	.N}/{sun.Statistics.Consensing	.Avarage.Ticks/10}"));
				f.Add(new ("Transacting (nps/μs)",	$"{sun.Statistics.Transacting	.N}/{sun.Statistics.Transacting	.Avarage.Ticks/10}"));
				f.Add(new ("Declaring (nps/μs)",	$"{sun.Statistics.Declaring		.N}/{sun.Statistics.Declaring	.Avarage.Ticks/10}"));
				f.Add(new ("Sending (nps/μs)",		$"{sun.Statistics.Sending		.N}/{sun.Statistics.Sending		.Avarage.Ticks/10}"));
				f.Add(new ("Reading (nps/μs)",		$"{sun.Statistics.Reading		.N}/{sun.Statistics.Reading		.Avarage.Ticks/10}"));

				sun.Statistics.Reset();
		
				return new Return{Summary = f.Take(Limit).Select(i => new [] {i.Key, i.Value}).ToArray() }; 
			}
		}

		public class Return
		{
			public IEnumerable<string[]> Summary {get; set;}
		}
	}
}
