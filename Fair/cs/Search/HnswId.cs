namespace Uccs.Fair;

public class HnswId : AutoId
{
	public byte						Level => BucketToLevel(B); /// 3 bit = 8 levels

	public const int				LevelBits = 4;

	public static readonly HnswId	Entry = new HnswId(0, 0);
	public static byte				BucketToLevel(int bucket) => (byte)(bucket >> TableBase.BucketBase.Length-LevelBits);
	public static byte				ClusterToLevel(short cluster) => (byte)(cluster >> TableBase.ClusterBase.Length-LevelBits);

	public HnswId()
	{
	}

	public HnswId(int b, int e) : base(b, e)
	{
	}

	public static int ToBucket(byte level, byte[] x)
	{
		/// LLLL---------------- 
		/// ----00000000--------
		/// ------------11111111
		
		var r = level << TableBase.BucketBase.Length-LevelBits;
		
		r |= x[0] << TableBase.BucketBase.Length-LevelBits-8 & 0b_0000_11111111_0000000;

		if(x.Length > 1)
			r |= x[1];
			
		return r;
	}
}
