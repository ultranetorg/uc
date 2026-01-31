namespace Uccs.Rdn;

public class ResourceLinkCreation : RdnOperation
{
	public AutoId				Source { get; set; }
	public AutoId				Destination { get; set; }
	public ResourceLinkFlag		Flags  { get; set; }
	
	public override string		Explanation => $"Source={Source}, Destination={Destination}";
	public override bool		IsValid(McvNet net) => true;

	public ResourceLinkCreation()
	{
	}

	public ResourceLinkCreation(AutoId source, AutoId destination, ResourceLinkFlag flags)
	{
		Source = source;
		Destination = destination;
		Flags = flags;
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Source);
		writer.Write(Destination);
		writer.Write(Flags);
	}
	
	public override void Read(BinaryReader reader)
	{
		Source		= reader.Read<AutoId>();
		Destination	= reader.Read<AutoId>();
		Flags		= reader.Read<ResourceLinkFlag>();
	}

	public override void Execute(RdnExecution execution)
	{
		if(RequireSignerResource(execution, Source, out var sd, out var s) == false)
			return;

		if(RequireResource(execution, Destination, out var dd, out var d) == false)
			return;

		s = execution.Resources.Affect(sd, s.Address.Resource);
		var l = s.AffectOutbound(d.Id);

		d = execution.Resources.Affect(dd, d.Address.Resource);
		d.AffectInbound(s.Id);

		if(Flags.HasFlag(ResourceLinkFlag.Dependency))
		{
			if(!d.Flags.HasFlag(ResourceFlags.Dependable))
			{
				Error = NotDependable;
				return;
			}

			l.Flags = Flags;

			var n = 0;

			bool circular(ResourceLink[] outs)
			{
				n++;

				if(n > execution.Net.CircularDependeciesChecksMaximum)
				{
					Error = LimitExceeded;
					return false;
				}

				foreach(var i in outs.Where(i => i.Flags.HasFlag(ResourceLinkFlag.Dependency)))
				{
					if(i.Destination == s.Id)
						return true;

					if(circular(execution.Resources.Find(i.Destination).Outbounds))
						return true;
				}

				return false;
			}

			if(circular(d.Outbounds))
			{
				Error ??= CircularDependency;
				return;
			}
		}

		sd = execution.Domains.Affect(sd.Id);
		execution.Allocate(User, sd, execution.Net.EntityLength);
		execution.PayOperationEnergy(User);
	}
}
