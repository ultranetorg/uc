using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;

namespace Uccs.Net
{
	[Flags]
	public enum ResourceFlags : byte
	{
		None			= 0, 
		Sealed			= 0b_______1, 
		Child			= 0b______10, 
		Data			= 0b_____100, 
	}

	public enum DataType : short
	{
		None		= 0,
		Redirect	= 1,
		IPAddress	= 2, 
		Uri			= 3,
				
		File		= 100, 
		Directory	= 101, 
		Package		= 102, 
		Consil		= 103, 
		Analysis	= 104, 

		FirstMime	= 1000, 
	}

	[Flags]
	public enum ResourceChanges : byte
	{
		None			= 0,
		Recursive		= 0b______________1,
		NotNullData		= 0b_____________10,
	}
 
 	public class Consil : IBinarySerializable
 	{
 		public Money			PerByteFee;
		public AccountAddress[]	Analyzers;

 		public byte[]			Raw {
										get
										{
											var s = new MemoryStream();
											var w = new BinaryWriter(s);
											
											Write(w);
											
											return s.ToArray();
										}
									}

		public void Read(BinaryReader reader)
		{
			PerByteFee	= reader.Read<Money>();
			Analyzers	= reader.ReadArray<AccountAddress>();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(PerByteFee);
			writer.Write(Analyzers);
		}

		public Consil Clone()
		{
			return new Consil {	PerByteFee	= PerByteFee, 
								Analyzers	= Analyzers.Clone() as AccountAddress[]};
		}
	}

	public enum AnalysisResult : byte
	{
		None,
		Negative,
		Positive,
		Vulnerable,
	}

	public struct AnalyzerResult
	{
		public byte				Analyzer { get; set; }
		public AnalysisResult	Result { get; set; }

		public override string ToString()
		{
			return $"Analyzer={Analyzer}, Result={Result}";
		}
	}


	public class Analysis : IBinarySerializable
	{
		public ReleaseAddress		Release { get; set; }
		public Money				Payment { get; set; }
		public ResourceAddress		Consil	{ get; set; }
		public AnalyzerResult[]		Results { get; set; }

 		public byte[]				Raw {
											get
											{
												var s = new MemoryStream();
												var w = new BinaryWriter(s);
												
												Write(w);
												
												return s.ToArray();
											}
										}


		public void Read(BinaryReader reader)
		{
			Release = reader.Read<ReleaseAddress>(ReleaseAddress.FromType);
			Consil	= reader.Read<ResourceAddress>();
			Payment	= reader.Read<Money>();
			Results	= reader.ReadArray(() => new AnalyzerResult { Analyzer = reader.ReadByte(), 
																  Result = (AnalysisResult)reader.ReadByte() });
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(Release);
			writer.Write(Consil);
			writer.Write(Payment);
			writer.Write(Results, i => { writer.Write(i.Analyzer);
										 writer.Write((byte)i.Result); });
		}

		public Analysis Clone()
		{
			return new Analysis {Release	= Release, 
								 Payment	= Payment, 
								 Consil		= Consil,	
								 Results	= Results.Clone() as AnalyzerResult[]};
		}
	}

	public class ResourceRelation : IBinarySerializable
	{
		public EntityId			Owner;
		public ResourceData		Data;

		public void Write(BinaryWriter writer)
		{
			writer.Write(Owner);
			writer.Write(Data);
		}

		public void Read(BinaryReader reader)
		{
			Owner	= reader.Read<EntityId>();
			Data	= reader.Read<ResourceData>();
		}

		public ResourceRelation Clone()
		{
			return new ResourceRelation {Owner = Owner, Data = Data.Clone()};
		}
	}

	public class ResourceData : IBinarySerializable, IEquatable<ResourceData>
	{
		public const short	LengthMax = 8192;

		public DataType		Type;
		byte[]				_Value;
		object				_Interpretation;

 		public byte[] Value
 		{
 			get
 			{
 				if(_Value == null)
 				{
 					var s = new MemoryStream();
 					var w = new BinaryWriter(s);
 			
 					WriteInterpetation(w);
 
 					_Value = s.ToArray();
 				}
 
 				return _Value;
 			}
 		}
		
 		public string Hex
 		{
 			get
 			{
 				var s = new MemoryStream();
 				var w = new BinaryWriter(s);
 			
				w.Write7BitEncodedInt((int)Type);
 				WriteInterpetation(w);
 
 				return s.ToArray().ToHex();
 			}
 		}

		public object Interpretation
		{
			get
			{
				if(_Interpretation == null && _Value != null)
				{
					ReadInterpetation(new BinaryReader(new MemoryStream(_Value)));
				}

				return _Interpretation;
			}
		}

		public ResourceData()
		{
		}

		//public ResourceData(byte[] data)
		//{
		//	Type = (DataType)new BinaryReader(new MemoryStream(data)).Read7BitEncodedInt();
		//	_Value = data;
		//}

		public ResourceData(BinaryReader reader)
		{
			Read(reader);
		}

		public ResourceData(DataType type, object interpretation)
		{
			Type = type;
			_Interpretation = interpretation;
		}

		public ResourceData(DataType type, byte[] value)
		{
			Type = type;
			_Value = value;
		}

// 		public static BinaryReader SkipHeader(byte[] data)
// 		{
// 			var r = new BinaryReader(new MemoryStream(data));
// 			r.Read7BitEncodedInt();
// 			return r;
// 		}

		public override string ToString()
		{
			return $"{Type}, {Value.Length}, {Interpretation}";
		}

		public void Read(BinaryReader reader)
		{
			Type = (DataType)reader.Read7BitEncodedInt();
			ReadInterpetation(reader);
		}

		public void ReadInterpetation(BinaryReader reader)
		{
			switch(Type)
			{
				case DataType.None:
					_Interpretation = reader.ReadBytes();
					break;

				case DataType.Redirect:
				case DataType.Uri:
					_Interpretation = reader.ReadUtf8();
					break;

				case DataType.IPAddress:
					_Interpretation = new IPAddress(reader.ReadBytes());
					break;

				case DataType.File:
				case DataType.Directory:
				case DataType.Package: 
					_Interpretation = reader.Read<ReleaseAddress>(ReleaseAddress.FromType);
					break;

				case DataType.Consil:
					_Interpretation = reader.Read<Consil>();
					break;

				case DataType.Analysis:
					_Interpretation = reader.Read<Analysis>();
					break;


				default:
					throw new ResourceException(ResourceError.UnknownDataType);
			}
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt((int)Type);
				
			if(_Value != null)
			{
				writer.Write(_Value);
			} 
			else
			{
				WriteInterpetation(writer);
			}
		}

		void WriteInterpetation(BinaryWriter writer)
		{
			switch(Type)
			{
				case DataType.None:
					writer.WriteBytes(Interpretation as byte[]);
					break;
	
				case DataType.Redirect:
				case DataType.Uri:
					writer.WriteUtf8(Interpretation as string);
					break;
				
				case DataType.IPAddress:
					writer.WriteBytes((Interpretation as IPAddress).GetAddressBytes());
					break;
				
				case DataType.File:
				case DataType.Directory:
				case DataType.Package: 
				case DataType.Consil:
				case DataType.Analysis:
					writer.Write(Interpretation as IBinarySerializable);
					break;

				default:
					throw new ResourceException(ResourceError.UnknownDataType);
			}
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Value);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as ResourceData);
		}

		public bool Equals(ResourceData other)
		{
			return other is not null && Type == other.Type && Value.SequenceEqual(other.Value);
		}

		public static bool operator ==(ResourceData left, ResourceData right)
		{
			return  left is null && right is null || 
					left is not null && left.Equals(right);
		}

		public static bool operator !=(ResourceData left, ResourceData right)
		{
			return !(left == right);
		}
		
		public ResourceData Clone()
		{
			object i = null;

			if(_Interpretation == null)
			{
				return new ResourceData {Type = Type, _Value = _Value};
			} 
			else
			{
				switch(_Interpretation)
				{
					case byte[] v:
						i = v.Clone();
						break;

					case string v:
						i = v.Clone();
						break;

					case IPAddress v:
						i = new IPAddress(v.GetAddressBytes());
						break;

					case ReleaseAddress v: 
						i = ReleaseAddress.FromRaw(v.Raw);
						break;

					case Consil v:
						i = v.Clone();
						break;

					case Analysis v:
						i = v.Clone();
						break;

					default:
						throw new ResourceException(ResourceError.UnknownDataType);
				}
				
				return new ResourceData {Type = Type, _Interpretation = i, _Value = _Value};
			}
		}
	}

	public class Resource : IBinarySerializable
	{
		public ResourceId			Id { get; set; }
		public ResourceAddress		Address { get; set; }
		public ResourceFlags		Flags { get; set; }
		public ResourceData			Data { get; set; }
		public Time					Updated { get; set; }
		public int[]				Resources { get; set; } = {};
		public ResourceRelation[]	Relations { get; set; }

		[JsonIgnore]
		public bool					New;
		List<ResourceRelation>		AffectedRelations = new();

		public override string ToString()
		{
			return $"{Id}, {Address}, [{Flags}], Data={{{Data}}}, Resources={{{Resources.Length}}}";
		}

		public Resource Clone()
		{
			return new() {	Id = Id,
							Address	= Address, 
							Flags = Flags,
							Data = Data,
							Updated = Updated,
							Resources = Resources,
							Relations = Relations?.ToArray(),
							};
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write((byte)Flags);
			writer.Write(Updated);
			
			if(Flags.HasFlag(ResourceFlags.Data))
				writer.Write(Data);
		
			writer.Write(Resources, i => writer.Write7BitEncodedInt(i));
			writer.Write(Relations);
		}

		public void Read(BinaryReader reader)
		{
			Flags = (ResourceFlags)reader.ReadByte();
			Updated	= reader.Read<Time>();

			if(Flags.HasFlag(ResourceFlags.Data))
				Data = reader.Read<ResourceData>();

			Resources = reader.ReadArray(() => reader.Read7BitEncodedInt());
			Relations = reader.ReadArray<ResourceRelation>();
		}

		public ResourceRelation AffectRelation(EntityId owner, ResourceData data)
		{
			var r = AffectedRelations.Find(i => i.Owner == owner && i.Data == data);
			
			if(r != null)
				return r;

			var i = Array.FindIndex(Relations, i => i.Owner == owner && i.Data == data);

			if(i != -1)
			{
				Relations = Relations.ToArray();

				r = Relations[i].Clone();
				Relations[i] = r;
			} 
			else
			{
				r = new ResourceRelation {Owner = owner, Data = data};
				Relations = Relations.Append(r).ToArray();
			}

			AffectedRelations.Add(r);

			return r;
		}
	}
}
