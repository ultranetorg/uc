using System.Reflection;

namespace Uccs.Rdn
{
	public enum RdnPpcClass : byte
	{
		None = 0, 
		Domain = McvPpcClass._Last + 1, 
		RdnMembers,
		QueryResource, Resource, DeclareRelease, LocateRelease, FileInfo, DownloadRelease, Cost
	}

	public class RdnTcpPeering : McvTcpPeering
	{
		//public override long	Roles => Node.Settings.Roles;
		//public new Rdn			Net => base.Net as Rdn;
		//public new RdnMcv		Mcv => base.Mcv as RdnMcv;
		//public new RdnSettings	Settings => base.Settings as RdnSettings;

		public RdnTcpPeering(RdnNode node, PeeringSettings settings, long roles, Vault vault, Flow flow, IClock clock) : base(node, settings, roles, vault, flow)
		{
			///if(t == typeof(VersionManifest))	
			///	return new VersionManifest();

 			foreach(var i in Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(PeerRequest)) && !i.IsGenericType))
 			{	
 				if(Enum.TryParse<RdnPpcClass>(i.Name.Remove(i.Name.IndexOf("Request")), out var c))
 				{
 					Codes[i] = (byte)c;
					var x = i.GetConstructor([]);
 					Contructors[typeof(PeerRequest)][(byte)c] = () =>	{
																			var r = x.Invoke(null) as PeerRequest;
																			r.Node = node;
																			return r;
																		};
 				}
 			}
 	 
 			foreach(var i in Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(PeerResponse))))
 			{	
 				if(Enum.TryParse<RdnPpcClass>(i.Name.Remove(i.Name.IndexOf("Response")), out var c))
 				{
 					Codes[i] = (byte)c;
					var x = i.GetConstructor([]);
 					Contructors[typeof(PeerResponse)][(byte)c] = () => x.Invoke(null);
 				}
 			}
 	 
			Contructors[typeof(Urr)] = [];

 			foreach(var i in Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(Urr))))
 			{	
 				if(Enum.TryParse<UrrScheme>(i.Name, out var c))
 				{
 					Codes[i] = (byte)c;
					var x = i.GetConstructor([]);
 					Contructors[typeof(Urr)][(byte)c] = () => x.Invoke(null);
 				}
 			}

			Codes[typeof(ResourceException)] = (byte)ExceptionClass._Next;
			Contructors[typeof(NetException)][(byte)ExceptionClass._Next]  = () => typeof(ResourceException).GetConstructor([]).Invoke(null);

			Run();
		}

		public override object Constract(Type t, byte b)
		{
// 			if(t == typeof(VersionManifest))	
// 				return new VersionManifest();
// 
// 			if(t == typeof(PeerRequest))		
// 			{
// 				var o = typeof(RdnTcpPeering).Assembly.GetType(typeof(RdnTcpPeering).Namespace + "." + ((RdnPeerCallClass)b).ToString() + "Request");
// 				
// 				if(o != null)
// 					return o.GetConstructor([]).Invoke(null);
// 			}
// 
// 			if(t == typeof(PeerResponse))		
// 			{
// 				var o = typeof(RdnTcpPeering).Assembly.GetType(typeof(RdnTcpPeering).Namespace + "." + ((RdnPeerCallClass)b).ToString() + "Response");
// 				
// 				if(o != null)
// 					return o.GetConstructor([]).Invoke(null);
// 			}
// 
// 			if(t == typeof(Urr))
// 			{
// 				var o = typeof(RdnTcpPeering).Assembly.GetType(typeof(RdnTcpPeering).Namespace + "." + ((UrrScheme)b).ToString());
// 				
// 				if(o != null)
// 					return o.GetConstructor([]).Invoke(null);
// 			}
// 
// 			if(t == typeof(NetException))
// 				if(b == (byte)ExceptionClass._Next)
// 					return new ResourceException();

			return base.Constract(t, b);
		}

		public override byte TypeToCode(Type i)
		{
//			RdnPeerCallClass c = 0;
//
//			if(i.IsSubclassOf(typeof(PeerRequest)))
//				if(Enum.TryParse(i.Name.Remove(i.Name.IndexOf("Request")), out c))
//					return (byte)c;
//	
//			if(i.IsSubclassOf(typeof(PeerResponse)))
//				if(Enum.TryParse(i.Name.Remove(i.Name.IndexOf("Response")), out c))
//					return (byte)c;
//
//			if(i.IsSubclassOf(typeof(Urr)))
//				if(Enum.TryParse<UrrScheme>(i.Name, true, out var u))
//					return (byte)u;
//
//			if(i == typeof(ResourceException))
//				return (byte)ExceptionClass._Next;

			return base.TypeToCode(i);
		}

		public override bool ProcessIncomingOperation(Operation o)
		{
			#if ETHEREUM
			if(o is Immission e && !Ethereum.IsEmissionValid(e))
				return false;
			#endif

			if(o is DomainMigration m && !(Node as RdnNode).IsDnsValid(m))
				return false;

			return true;
		}

		public override void OnRequestException(Peer peer, NodeException ex)
		{
			base.OnRequestException(peer, ex);

			if(ex.Error == NodeError.NotSeed)	peer.Roles  &= ~(long)RdnRole.Seed;
		}

	}
}
