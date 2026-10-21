namespace Uccs.Fair;

public class StoreApprovalPolicyChange : StoreOperation
{
	public FairOperationClass	Operation { get; set; }
	//public Role					Creators { get; set; }
	public ApprovalRequirement	Approval { get; set; }

	public override bool		IsValid(McvNet net) =>	Enum.IsDefined<FairOperationClass>(Operation) && 
														//Enum.IsDefined<Role>(Creators)&&
														Enum.IsDefined<ApprovalRequirement>(Approval);

	public override string		Explanation => $"Store={Store}, Operation+{Operation}, Approval={Approval}";
	
	public override void Read(Reader reader)
	{
		Operation	= reader.Read<FairOperationClass>();
		//Creators	= reader.Read<Role>();
		Approval	= reader.Read<ApprovalRequirement>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Operation);
		//writer.Write(Creators);
		writer.Write(Approval);
	}

// 	public override bool ValidateProposal(FairExecution execution, out string error)
// 	{
//		if(Store.CreationPolicies.TryGetValue(Change, out var c) && c.SequenceEqual(Creators) &&
//			Store.ApprovalPolicies.TryGetValue(Change, out var a) && a == Approval)
//		{	
//			error = AlreadyExists;
//			return false;
//		}
//
//		if(Change == FairOperationClass.StorePolicyChange)
//		{
//			error = NotAvailable;
//			return false;
//		}
//		
//		error = null;
//		return true;
// 	}

	public override void Execute(FairExecution execution)
	{
		if(!Store.Restrictions.First(i => i.OperationClass == Operation).Flags.HasFlag(PolicyFlag.VariableApproval))
		{
			Error = Denied;
			return;
		}

		//if(!Store.Restrictions.First(i => i.OperationClass == Operation).Creators.HasFlag(Creators))
		//{
		//	Error = Denied;
		//	return;
		//}

 		var s = Store;

		s.Policies = [..s.Policies];
		
		var j = Array.FindIndex(s.Policies, i => i.OperationClass == Operation);

		s.Policies[j] = new Policy(Operation, s.Policies[j].Creators, Approval);

		foreach(var i in s.Proposals.Where(i => execution.Proposals.Find(i).OptionClass == s.Policies[j].OperationClass))
		{
			var p = execution.Proposals.Affect(i);

			p.Neither = [];
			p.Any = [];
			p.Ban = [];
			p.Banish = [];

			p.Options = [..p.Options];

			for(int l = 0; l < p.Options.Length; l++)
			{
				p.Options[l] =	new () 
								{
									Title = p.Options[l].Title, 
									Operation = p.Options[l].Operation, 
									Yes = [], 
								};
			}
		}
	}
}