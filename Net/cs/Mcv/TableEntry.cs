namespace Uccs.Net;

//public interface ITableKey
//{
//	int	B { get; }
//}

public abstract class BaseId : IBinarySerializable, IEquatable<BaseId>, IComparable<BaseId>//, ITableKey
{
	public abstract int		B { get; set; }

	public abstract int		CompareTo(BaseId other);
	public abstract bool	Equals(BaseId other);
	public abstract void	Read(BinaryReader reader);
	public abstract void	Write(BinaryWriter writer);

	public static byte[]	BucketToBytes(int id) => [(byte)id, (byte)(id >> 8), (byte)(id >> 16)];
	
	public static int BytesToBucket(byte[] id)
	{
		if(id.Length >= 3) return	  id[2] << 16 | id[1] << 8 | id[0];
		if(id.Length == 2) return					id[1] << 8 | id[0];
 		
		return id[0];
	}

 	public static bool operator == (BaseId left, BaseId right)
 	{
 		return left is null && right is null || left is not null && left.Equals((object)right); /// object cast is IMPORTANT!!
 	}
 
 	public static bool operator != (BaseId left, BaseId right)
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
	BaseId		Key { get; }
	//bool		New { get; set; }
	bool		Deleted { get; }

	void		Cleanup(Round lastInCommit);

	void		ReadMain(BinaryReader r);
	void		WriteMain(BinaryWriter r);
}
