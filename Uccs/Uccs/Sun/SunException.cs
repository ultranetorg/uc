﻿using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net
{

	public enum ExceptionClass : byte
	{
		None, NodeException, RequestException, EntityException, ResourceException
	}

	public enum NodeError : byte
	{
		None,
		Unknown,
		Connectivity,
		Integrity,
		Internal,
		Timeout,
		NoIP,
		NotBase,
		NotChain,
		NotSeed,
		NotMember,
		NotSynchronized,
		TooEearly,
		AllNodesFailed,
		NotOnlineYet,
		CircularRoute,
	}

	public enum EntityError : byte
	{
		None,
		NotFound,
		RoundNotAvailable,
	}

	public enum RequestError : byte
	{
		None,
	}

	public enum ResourceError : byte
	{
		None,
		UnknownDataType,
		BothResourceAndReleaseNotFound,
		RequiredPackagesNotFound,
		AlreadyExists,
		DataTypeNotSupported
	}

	public abstract class SunException : Exception, ITypedBinarySerializable, IBinarySerializable 
	{
		public byte				TypeCode => (byte)Class;
		public abstract int		ErrorCode {get; set;}
		public ExceptionClass	Class => Enum.Parse<ExceptionClass>(GetType().Name);	

		public SunException()
		{
		}

		public SunException(string message) : base(message)
		{
		}

		public static SunException FromType(ExceptionClass type)
		{
			return Assembly.GetExecutingAssembly().GetType(typeof(SunException).Namespace + "." + type).GetConstructor(new System.Type[]{}).Invoke(null) as SunException;
		}

		public void Read(BinaryReader reader)
		{
			ErrorCode = reader.Read7BitEncodedInt();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt(ErrorCode);
		}
	}

 	public class NodeException : SunException
 	{
		public override int				ErrorCode { get => (int)Error; set => Error = (NodeError)value; }
		public NodeError				Error { get; protected set; }
		public override string			Message => Error.ToString();

		public NodeException()
		{
		}

		public NodeException(NodeError erorr) : base(erorr.ToString())
		{
			Error = erorr;
		}

	}

	public class RequestException : SunException
	{
		public override int				ErrorCode { get => (int)Error; set => Error = (RequestError)value; }
		public RequestError				Error { get; protected set; }
		public override string			Message => Error.ToString();

		public RequestException()
		{
		}

		public RequestException(RequestError erorr) : base(erorr.ToString())
		{
			Error = erorr;
		}
	}

 	public class EntityException : SunException
 	{
		public override int				ErrorCode { get => (int)Error; set => Error = (EntityError)value; }
		public EntityError				Error { get; protected set; }
		public override string			Message => Error.ToString();

		public EntityException()
		{
		}

		public EntityException(EntityError erorr) : base(erorr.ToString())
		{
			Error = erorr;
		}
 	}

	public class ResourceException : SunException
	{
		public override int				ErrorCode { get => (int)Error; set => Error = (ResourceError)value; }
		public ResourceError			Error { get; protected set; }
		public override string			Message => Error.ToString();

		public ResourceException()
		{
		}

		public ResourceException(ResourceError erorr) : base(erorr.ToString())
		{
			Error = erorr;
		}
	}

	public class SunExceptionJsonConverter : JsonConverter<SunException>
	{
		public override SunException Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var s = reader.GetString().Split(':');
			var o = SunException.FromType(Enum.Parse<ExceptionClass>(s[0]));
 			
			o.Read(new BinaryReader(new MemoryStream(s[1].FromHex()))); 

			return o;
		}

		public override void Write(Utf8JsonWriter writer, SunException value, JsonSerializerOptions options)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			
			value.Write(w);
			
			writer.WriteStringValue(value.Class.ToString() + ":" + s.ToArray().ToHex());
		}
	}

}