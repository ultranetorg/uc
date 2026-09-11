using System;
using System.Text;

namespace Uccs.Net;

public interface IOutworldOperation
{	
	public abstract void SuccessExecute(Execution execution, OutworldTransaction task);
}

public class OutworldTransaction : IBinarySerializable
{
	public int					Id;
	public Time					Expiration;
	public AutoId				User;
	public Operation			Operation;

	public virtual void Write(Writer writer)
	{
		writer.Write7BitEncodedInt(Id);
		writer.Write(User);
		writer.Write(Expiration);
		writer.WriteVirtual(Operation); 
	}

	public virtual void Read(Reader reader)
	{
		Id			= reader.Read7BitEncodedInt();
		User		= reader.Read<AutoId>();
		Expiration	= reader.Read<Time>();
		Operation	= reader.ReadVirtual<Operation>(); 
	}
}

public struct OutworldResult : IBinarySerializable, IEquatable<OutworldResult>, IComparable<OutworldResult>
{
	public AutoId		User;
	public int			Id;
	public bool			Approved;

	public void Read(Reader reader)
	{
		User		= reader.Read<AutoId>();
		Id			= reader.Read7BitEncodedInt();
		Approved	= reader.ReadBoolean();	
	}

	public void Write(Writer writer)
	{
		writer.Write(User);
		writer.Write7BitEncodedInt(Id);
		writer.Write(Approved);
	}

	public override bool Equals(object obj)
	{
		return obj is OutworldResult id && Equals(id);
	}

	public bool Equals(OutworldResult a)
	{
		return User == a.User && Id == a.Id && Approved == a.Approved;
	}

	public int CompareTo(OutworldResult a)
	{
		var c = User.CompareTo(a.User);

		if(c != 0)
			return c;

		c = Id.CompareTo(a.Id);

		if(c != 0)
			return c;

		c = Approved.CompareTo(a.Approved);

		if(c != 0)
			return c;

		return 0;
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}

	public static bool operator == (OutworldResult left, OutworldResult right)
	{
		return left.Equals(right);
	}

	public static bool operator != (OutworldResult left, OutworldResult right)
	{
		return !left.Equals(right);
	}

	internal void Dump(string tab, StringBuilder b)
	{
		b.Append(tab);		b.AppendLine(User.ToString());
		b.Append(tab);		b.AppendLine(Id.ToString());
		b.Append(tab);		b.AppendLine(Approved.ToString());
	}
}
