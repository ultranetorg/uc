namespace Uccs.Net;

public abstract class EntityId : IBinarySerializable, IEquatable<EntityId>, IComparable<EntityId>//, ITableKey
{
	public abstract int		B { get; set; }

	public abstract int		CompareTo(EntityId other);
	public abstract bool	Equals(EntityId other);
	public abstract void	Read(BinaryReader reader);
	public abstract void	Write(BinaryWriter writer);

	public static byte[]	BucketToBytes(int id) => [(byte)id, (byte)(id >> 8), (byte)(id >> 16)];
	
	byte[]					_Raw;

	public byte[] Raw
	{
		get
		{
			if(_Raw != null)
				return _Raw;

			var s = new MemoryStream();
			var w = new BinaryWriter(s);
								
			Write(w);
								
			return _Raw = s.ToArray();
		}
	}

	public static int BytesToBucket(byte[] id)
	{
		if(id.Length >= 3) return id[0] | id[1] << 8 | ((id[2] & 0xf) << 16); /// for 20 bit bucket
		if(id.Length == 2) return id[0] | id[1] << 8;
 		
		return id[0];
	}

 	public static bool operator == (EntityId left, EntityId right)
 	{
 		return left is null && right is null || left is not null && left.Equals((object)right); /// object cast is IMPORTANT!!
 	}
 
 	public static bool operator != (EntityId left, EntityId right)
 	{
 		return !(left == right);
 	}

	public override int GetHashCode()
	{
		return B.GetHashCode();
	}

	public override abstract bool Equals(object obj);
}

public interface ITableEntry
{
	EntityId	Key { get; }
	//bool		New { get; set; }
	bool		Deleted { get; }

	object		Clone();
	void		Cleanup(Round lastInCommit);

	void		ReadMain(BinaryReader r);
	void		WriteMain(BinaryWriter r);

	public byte[] ToMain()
	{
		var s = new MemoryStream();
		var w = new BinaryWriter(s);
								
		WriteMain(w);
								
		return s.ToArray();
	}
}
