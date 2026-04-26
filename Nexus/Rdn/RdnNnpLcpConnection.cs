using System.Numerics;
using System.Text;
using System.Threading.Tasks.Dataflow;
using Uccs.Net;

namespace Uccs.Rdn;

public class RdnNnpLcpConnection : McvNnpLcpConnection<RdnNode, RdnTable>
{
	public new RdnNode	Node => base.Node as RdnNode;

	public RdnNnpLcpConnection(RdnNode node, Flow flow) : base(node, [nameof(User)], flow)
	{
		if(node.Mcv != null)
		{
			node.Mcv.Confirmed += r =>	{
	
											//foreach(var i in r.ConsensusSubnetHashes)
											//{
										 	//	var b = new SubnetMessage();
										 	//
										 	//	b.Net	= node.Net.Name;
										 	//	b.State = new() {RootHash = node.Mcv.LastConfirmedRound.Hash,
										 	//					 Peers = node.Mcv.LastConfirmedRound.Members.Select(i => new NnpState.Peer {IP = i.GraphPpcIPs[0], Port = 0}).ToArray()};
										 	//	
										 	//	Node.NnConnection.Broadcast(b);
											//}
										};
		}
	}

	public Result Transaction(TransactionNna args) /// A message from a subnet to vote for
	{
		lock(Node.Mcv.Lock)
		{	
			var m = Node.Mcv.SubnetTransactions.Find(i => i.Net == args.Net && i.Hash.SequenceEqual(args.Hash));

			if(m != null)
				return null;

			if(m.Nonce != args.Nonce - 1)
				throw new EntityException(EntityError.NotSequential);

			/// ////
			/// Need to ckeck at other nodes or using mutlisignature
			/// ////

			Node.Mcv.SubnetTransactions.Add(m);

			return null;
		}
	}

	public Result TransactionConfirmation(TransactionConfirmationNna args) /// Confirmation on our message to a subnet
	{
		lock(Node.Mcv.Lock)
		{
			var m = Node.Mcv.SubnetTransactionConfirmations.Find(i => i.Net == args.Net && i.Hash.SequenceEqual(args.Hash));

			if(m != null)
				return null;
			
			Node.Mcv.SubnetTransactionConfirmations.Add(m);

			return null;
		}
	}
}
