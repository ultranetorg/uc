using System.IO;

namespace Uccs
{
	public interface IBinarySerializable
	{
		void			Write(BinaryWriter writer);
		void			Read(BinaryReader reader);

 		public byte[]	Raw {
								get
								{
									var s = new MemoryStream();
									var w = new BinaryWriter(s);
									
									Write(w);
									
									return s.ToArray();
								}
							}
	}

	public interface ITypeCode
	{
		//public static Dictionary<Type, byte>								Codes = [];
		//public static Dictionary<Type, Dictionary<byte, ConstructorInfo>>	Contructors = [];

		//public static T Contruct<T>(byte code) => (T)Contructors[typeof(T)][code].Invoke(null);
// 
// 		public static void Register(Assembly assembly, Type type, Type @enum, byte code)
// 		{
// 			assembly.GetType(type.Namespace + "." + Enum.GetName(@enum, code)).GetConstructor([])
// 		}

	//	byte TypeCode { get; }
	}
}
