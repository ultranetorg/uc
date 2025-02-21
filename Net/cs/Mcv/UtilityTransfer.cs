namespace Uccs.Net;

public class UtilityTransfer : Operation
{
	public EntityId			To;
	public byte				ToTable;
	public EntityId			From;
	public byte				FromTable;
	public long				Spacetime;
	public long				Energy;
	public long				EnergyNext;
	public override string	Description => $"{Signer} -> {string.Join(", ", new string[] {(Energy > 0 ? Energy + " EC" : null), 
																						  (EnergyNext > 0 ? EnergyNext + " EC" : null), 
																						  (Spacetime > 0 ? Spacetime + " BD" : null)}.Where(i => i != null))} -> {To}";
	public override bool	IsValid(Mcv mcv) => Spacetime >= 0 && Energy >= 0 && EnergyNext >= 0;

	public UtilityTransfer()
	{
	}

	public UtilityTransfer(byte fromtable, EntityId from, byte totable, EntityId to, long energy, long energynext, long spacetime)
	{
		if(to == null)
			throw new RequirementException("Destination account is null or invalid");

		FromTable			= fromtable;
		From				= from;
		ToTable				= totable;
		To					= to;

		Energy				= energy;
		EnergyNext			= energynext;
		Spacetime			= spacetime;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		FromTable				= reader.ReadByte();
		From					= reader.Read<EntityId>();
		ToTable					= reader.ReadByte();
		To						= reader.Read<EntityId>();

		Energy					= reader.Read7BitEncodedInt64();
		EnergyNext				= reader.Read7BitEncodedInt64();
		Spacetime				= reader.Read7BitEncodedInt64();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(FromTable);
		writer.Write(From);
		writer.Write(ToTable);
		writer.Write(To);

		writer.Write7BitEncodedInt64(Energy);
		writer.Write7BitEncodedInt64(EnergyNext);
		writer.Write7BitEncodedInt64(Spacetime);
	}

	public override void Execute(Mcv mcv, Round round)
	{
		var to = round.Affect(ToTable, To == EntityId.LastCreated ? round.LastCreatedId : To);

		if(to == null)
		{
			Error = NotFound;
			return;
		}

		IHolder h = null;

		if(Signer.Address != mcv.Net.God)
		{
			h = round.Affect(FromTable, From) as IHolder;

			if(h == null)
			{
				Error = NotFound;
				return;
			}

			if(!h.IsSpendingAuthorized(round, Signer.Id))
			{
				Error = Denied;
				return;
			}
		}

		if(Energy > 0 || EnergyNext > 0)
		{
			if(From != EntityId.God)
			{
				var s = h as IEnergyHolder;

				if(s == null)
				{
					Error = NotEnergyHolder;
					return;
				}
	
				if(Signer.Address != mcv.Net.God)
				{
					s.Energy		-= Energy;
					s.EnergyNext	-= EnergyNext;
				}

				EnergySpenders.Add(s);
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
			if(Signer.Address != mcv.Net.God)
			{
				var s = h as ISpacetimeHolder;

				if(s == null)
				{
					Error = NotSpacetimeHolder;
					return;
				}
				
				s.Spacetime	-= Spacetime;
				
				SpacetimeSpenders.Add(s);
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
