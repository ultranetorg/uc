using System.Numerics;

namespace Uccs.Net;

public class UserCreation : Operation
{
	public byte[]				Pow { get; set; }
	public AccountAddress		Owner { get; set; }
	public override string		Explanation => $"Pow={Pow?.ToHex()} {nameof(Owner)}=Owner";
	
	public override bool IsValid(McvNet net)
	{ 
		return (Pow == null || Pow.Length <= 32) && Owner != null;
	}

	public override void Read(Reader reader)
	{
		Pow = reader.ReadBytes();
		Owner = reader.Read<AccountAddress>();
	}

	public override void Write(Writer writer)
	{
		writer.WriteBytes(Pow);
		writer.Write(Owner);
	}

	public override void PreTransact(McvNode node, Flow flow)
	{
		Pow = null;
			
		if(node.Net.UserCreationPoWDifficulity > 0)
		{
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
																	 
																	 		if(f >= node.Net.UserCreationPoWDifficulity)
																	 		{
																	 			Pow = b;
																	 		}
																	 	}
																	 })).ToArray();
			foreach(var i in ts)
				i.Start();
			
			while(Pow == null)
				Thread.Sleep(100);
		} 
		else /// simulation
		{
			Pow = new byte[32];
		}
	}

	public override void Execute(Execution execution)
	{
		if(execution.Transaction.Nonce != 0)
		{
			Error = AlreadyExists;
			return;
		}

		if(Cryptography.Hash([..execution.Mcv.GraphHash, ..Pow]).Sum(i => BitOperations.PopCount(i)) < execution.Net.UserCreationPoWDifficulity)
		{
			Error = DoesNotSatisfy;
			return;
		}

		User.Owner = Owner;
	}
}
