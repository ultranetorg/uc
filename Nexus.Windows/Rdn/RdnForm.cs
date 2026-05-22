using Uccs.Mcv.FUI;
using Uccs.Net;
using Uccs.Rdn;

namespace Uccs.Nexus.Windows;

public class RdnForm : MainForm
{
	public RdnForm(RdnNode uos) : base(uos)
	{
	}

	protected override void LoadNode(McvNode node)
	{
		base.LoadNode(node);


//			var d = new TreeNode("Domains"){ Tag = new DomainPanel(rdn)};
//			root.Nodes.Add(d);
//	
			var r = new TreeNode("Resources"){Tag = new ResourcesPanel(node as RdnNode)};
			Navigator.Nodes.Add(r);
//
//			//var e = new TreeNode("Emission"){ Tag = new EmissionPanel(rdn)};
//			//root.Nodes.Add(e);
//
//			if(rdn.SeedHub != null)
//			{
//				var s = new TreeNode("Seed Hub"){ Tag = new HubPanel(rdn)};
//				root.Nodes.Add(s);
//			}
	}
}
