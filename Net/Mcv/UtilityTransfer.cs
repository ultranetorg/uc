namespace Uccs.Net;

public class UtilityTransfer : Operation
{
	public EntityAddress	From { get; set; }
	public EntityAddress	To { get; set; }
	public long				Spacetime { get; set; }
	public long				Energy { get; set; }
	public long				EnergyNext { get; set; }
	public override string	Explanation => $"{From} -> {To}, {string.Join(", ", new string[]   {(Energy > 0 ? Energy + " EC" : null), 
																								(EnergyNext > 0 ? EnergyNext + " EC" : null), 
																								(Spacetime > 0 ? Spacetime + " BD" : null)}.Where(i => i != null))} -> {To}";

	public override bool	IsValid(McvNet net) => Spacetime >= 0 && Energy >= 0 && EnergyNext >= 0 && To.IsValid(net) && From.IsValid(net);

	public UtilityTransfer()
	{
	}

	public UtilityTransfer(byte fromtable, AutoId from, byte totable, AutoId to, long energy, long energynext, long spacetime)
	{
		if(to == null)
			throw new RequirementException("Destination account is null or invalid");

		From				= new (fromtable, from);
		To					= new (totable, to);

		Energy				= energy;
		EnergyNext			= energynext;
		Spacetime			= spacetime;
	}

	public override void Read(BinaryReader reader)
	{
		From					= reader.Read<EntityAddress>();
		To						= reader.Read<EntityAddress>();

		Energy					= reader.Read7BitEncodedInt64();
		EnergyNext				= reader.Read7BitEncodedInt64();
		Spacetime				= reader.Read7BitEncodedInt64();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(From);
		writer.Write(To);

		writer.Write7BitEncodedInt64(Energy);
		writer.Write7BitEncodedInt64(EnergyNext);
		writer.Write7BitEncodedInt64(Spacetime);
	}

	public override void Execute(Execution execution)
	{
		if(To.Id == AutoId.LastCreated && execution.FindExecution(To.Table).LastCreatedId == null)
		{
			Error = NothingLastCreated;
			return;
		}

		var to = execution.Affect(To.Table, To.Id);

		if(to == null)
		{
			Error = NotFound;
			return;
		}

		IHolder h = null;

		if(execution.Round.Id > 0)
		{
			h = execution.Affect(From.Table, From.Id) as IHolder;

			if(h == null)
			{
				Error = NotFound;
				return;
			}

			if(!h.IsSpendingAuthorized(execution, Signer.Id))
			{
				Error = Denied;
				return;
			}
		}

		if(Energy > 0 || EnergyNext > 0)
		{
			if(execution.Round.Id > 0)
			{
				var s = h as IEnergyHolder;

				if(s == null)
				{
					Error = NotEnergyHolder;
					return;
				}
	
				if(Signer.Address != Mcv.God.Address)
				{
					s.Energy		-= Energy;
					s.EnergyNext	-= EnergyNext;
				}

				execution.EnergySpenders.Add(s);
			}

			var d = to as IEnergyHolder;

			if(d == null)
			{
				Error = NotEnergyHolder;
				return;
			}

			d.Energy		+= Energy;
			d.EnergyNext	+= EnergyNext;
		
		}
				
		if(Spacetime > 0)
		{	
			if(execution.Round.Id > 0)
			{
				var s = h as ISpacetimeHolder;

				if(s == null)
				{
					Error = NotSpacetimeHolder;
					return;
				}
				
				s.Spacetime	-= Spacetime;
				
				execution.SpacetimeSpenders.Add(s);
			}

			var d = to as ISpacetimeHolder;

			if(d == null)
			{
				Error = NotSpacetimeHolder;
				return;
			}

			d.Spacetime += Spacetime;
		}
	}
}
