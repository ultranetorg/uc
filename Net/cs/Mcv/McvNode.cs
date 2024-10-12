namespace Uccs.Net
{
	public class McvNode : Node
	{
		public McvNet			Net;
		public Vault			Vault; 
		public Mcv				Mcv;
		public McvTcpPeering	Peering;
		public HomoTcpPeering		NtnPeering;
		//public NodeSettings	Settings;

		public McvNode(string name, McvNet net, string profile, Flow flow, Vault vault) : base(name, profile, flow)
		{
			Net = net;
			Vault = vault;
		}

		public override string ToString()
		{
			return Peering.ToString();
		}
	}
}
