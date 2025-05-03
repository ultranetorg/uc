namespace Uccs.Fair;

public class HnswId : AutoId
{
	public byte				Level => BucketToLevel(B); /// 3 bit = 8 levels

	public const int		LevelBits = 3;

	public static int		ToBucket(byte level, byte[] x) => level << TableBase.BucketBase.Length-LevelBits | (x[0] << TableBase.BucketBase.Length-LevelBits-8 | x[1] << TableBase.BucketBase.Length-LevelBits-16 | 0x01&x[2]) & 0b_0001_11111111_11111111;
	public static byte		BucketToLevel(int bucket) => (byte)(bucket >> TableBase.BucketBase.Length-LevelBits);
	public static byte		ClusterToLevel(short cluster) => (byte)(cluster >> TableBase.ClusterBase.Length-LevelBits);

	public HnswId()
	{
	}

	public HnswId(int b, int e) : base(b, e)
	{
	}
}
