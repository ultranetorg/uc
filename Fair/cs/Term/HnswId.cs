namespace Uccs.Fair;

public class HnswId : AutoId
{
	public byte				Level => BucketToLevel(B); /// 3 bit = 8 levels

	public static int		ToBucket(byte level, byte[] x) => level << 8+8+8-3 | (x[0] << 16 | x[1] << 8 | x[2]) & 0b1_1111_11111111_11111111;
	public static byte		BucketToLevel(int bucket) => (byte)(bucket >> 8+8+8-3);
	public static byte		ClusterToLevel(short cluster) => (byte)(cluster >> 1+8);

	public HnswId()
	{
	}

	public HnswId(int b, int e) : base(b, e)
	{
	}



// 	public override string ToString()
// 	{
// 		return $"{Level}-{E}";
// 	}
// 
// 	public override int GetHashCode()
// 	{
// 		return B;
// 	}
// 
// 	public static bool TryParse(string text, out HnswId entity)
// 	{
// 		var i = text.IndexOf('-');
// 
// 		entity = null;
// 
// 		if(i == -1)
// 			return false;
// 
// 		if(byte.TryParse(text.AsSpan(0, i), out var b) && int.TryParse(text.AsSpan(i + 1), out var e))
// 		{
// 			if(e < 0)
// 				return false;
// 
// 			entity = new HnswId(b, e);
// 			
// 			return true;
// 		}
// 		else
// 			return false;
// 	}
// 
// 	public static HnswId Parse(string t)
// 	{
// 		if(TryParse(t, out var e))
// 			return e;
// 		else
// 			throw new FormatException();
// 	}
// 
// 	public override void Read(BinaryReader reader)
// 	{
// 		Level	= reader.ReadByte();
// 		E		= reader.Read7BitEncodedInt();
// 	}
// 
// 	public override void Write(BinaryWriter writer)
// 	{
// 		writer.Write(Level);
// 		writer.Write7BitEncodedInt(E);
// 	}
// 
// 	public override bool Equals(object obj)
// 	{
// 		return obj is HnswId id && Equals(id);
// 	}
// 
// 	public override bool Equals(BaseId a)
// 	{
// 		return a is HnswId x && B == x.B && E == x.E;
// 	}
// 
// 	public override int CompareTo(BaseId a)
// 	{
// 		return CompareTo((HnswId)a);
// 	}
// 
// 	public int CompareTo(HnswId a)
// 	{
// 		var c = Level.CompareTo(a.Level);
// 		
// 		if(c != 0)
// 			return c;
// 		
// 		c = E.CompareTo(a.E);
// 		
// 		if(c != 0)
// 			return c;
// 
// 		return 0;
// 	}
// 
// 	public static bool operator == (HnswId left, HnswId right)
// 	{
// 		return left is null && right is null || left is not null && left.Equals((object)right); /// object cast is IMPORTANT!!
// 	}
// 
// 	public static bool operator != (HnswId left, HnswId right)
// 	{
// 		return !(left == right);
// 	}
}
