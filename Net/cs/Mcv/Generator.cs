using System.Net;

namespace Uccs.Net;

public class Generator
{
	public AccountAddress	Address { get; set; }
	public EntityId			Id { get; set; }
	public int				Registered { get; set; }
	public IPAddress[]		BaseRdcIPs { get; set; } = [];
	public int				CastingSince { get; set; }
	
	public Peer         	Proxy;

	public override string ToString()
	{
		return $"Address={Address}, JoinedAt={CastingSince}, BaseRdcIPs={{{BaseRdcIPs.Length}}}";
	}

  		public virtual void WriteMember(BinaryWriter writer)
 		{
 			writer.Write(Id);
 			writer.Write(Address);
		writer.Write(BaseRdcIPs, i => writer.Write(i));
		writer.Write7BitEncodedInt(CastingSince);
 		}
 
 		public virtual void ReadMember(BinaryReader reader)
 		{
		Id				= reader.Read<EntityId>();
		Address			= reader.Read<AccountAddress>();
		BaseRdcIPs		= reader.ReadArray(() => reader.ReadIPAddress());
 			CastingSince	= reader.Read7BitEncodedInt();
	}

  		public virtual void WriteCandidate(BinaryWriter writer)
 		{
 			writer.Write(Id);
 			writer.Write7BitEncodedInt(Registered);
		writer.Write(BaseRdcIPs, i => writer.Write(i));
 		}
 
 		public virtual void ReadCandidate(BinaryReader reader)
 		{
		Id			= reader.Read<EntityId>();
		Registered	= reader.Read7BitEncodedInt();
		BaseRdcIPs	= reader.ReadArray(() => reader.ReadIPAddress());
	}

	public virtual Generator Clone()
	{
		throw new NotImplementedException();
	}

	public void Clone(Generator generator)
	{
		generator.Address		= Address;
		generator.Id			= Id;
		generator.Registered	= Registered;
		generator.BaseRdcIPs	= BaseRdcIPs;
		generator.CastingSince	= CastingSince;
	}
}
