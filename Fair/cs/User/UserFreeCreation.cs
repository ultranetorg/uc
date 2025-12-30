using System.Numerics;

namespace Uccs.Fair;

public class UserFreeCreation : FairOperation
{
	public byte[]				Pow { get; set; }
	public override string		Explanation => $"Pow={Pow.ToHex()}";
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(BinaryReader reader)
	{
		Pow = reader.ReadBytes();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteBytes(Pow);
	}

	public override void PreTransact(McvNode node, Flow flow)
	{
		Pow = null;
	
		
		var s = node.Peering.Call(new StampPpc {}, flow);
		
		var ts = Enumerable.Range(0, Environment.ProcessorCount)
						   .Select(i => node.CreateThread(() => { 
																 	var b = new byte[32];
																 	var a = Blake2Fast.Blake2b.CreateHashAlgorithm(32);
																	var r = new Random();
																 
																 	while(flow.Active && Pow == null)
																 	{
																 		r.NextBytes(b);
																 		var h = a.ComputeHash([..s.GraphHash, ..b]);
																 
																 		var f = h.Sum(i => BitOperations.PopCount(i));
																 
																 		if(f >= (node.Net as Fair).UserFreeCreationPoWComplexity)
																 		{
																 			Pow = b;
																 		}
																 	}
																 })).ToArray();
		foreach(var i in ts)
			i.Start();
						
		foreach(var i in ts)
			i.Join();
	}

	public override void Execute(FairExecution execution)
	{
		if(execution.Transaction.Nonce != 0)
		{
			Error = AlreadyExists;
			return;
		}

		if(Cryptography.Hash([..execution.Mcv.GraphHash, ..Pow]).Sum(i => BitOperations.PopCount(i)) < execution.Net.UserFreeCreationPoWComplexity)
		{
			Error = DoesNotSatisfy;
			return;
		}
	}
}
