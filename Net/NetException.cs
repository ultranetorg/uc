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
		AlreadyRunning,
		Timeout,
		NoIP,
		NotFound,
		NotBase,
		NotChain,
		NotSeed,
		NotMember,
		NotSynchronized,
		NotUnlocked,
		NoMcv,
		TooEearly,
		AllNodesFailed,
		NotOnlineYet,
		CircularRoute,
	}

	public enum EntityError : byte
	{
		None,
		NotFound,
		ExcutionFailed,
		EmissionFailed,
		NoMembers
	}

	public enum RequestError : byte
	{
		None,
		IncorrectRequest,
		OutOfRange,
	}

	public enum ResourceError : byte
	{
		None,
		UnknownDataType,
		UnknownAddressType,
		BothResourceAndReleaseNotFound,
		RequiredPackagesNotFound,
		AlreadyExists,
		NotSupportedDataType,
		Busy,
		NotFound,
		HashMismatch,
	}

	[JsonDerivedType(typeof(NodeException), typeDiscriminator: "Node")]
	[JsonDerivedType(typeof(RequestException), typeDiscriminator: "Request")]
	[JsonDerivedType(typeof(EntityException), typeDiscriminator: "Entity")]
	[JsonDerivedType(typeof(ResourceException), typeDiscriminator: "Resource")]
	public abstract class NetException : Exception, ITypeCode, IBinarySerializable 
	{
		public byte				TypeCode => (byte)Class;
		public abstract int		ErrorCode {get; set;}
		public ExceptionClass	Class => Enum.Parse<ExceptionClass>(GetType().Name);	

		public NetException()
		{
		}

		public NetException(string message) : base(message)
		{
		}

		public static NetException FromType(ExceptionClass type)
		{
			return Assembly.GetExecutingAssembly().GetType(typeof(NetException).Namespace + "." + type).GetConstructor(new System.Type[]{}).Invoke(null) as NetException;
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

 	public class NodeException : NetException
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

	public class RequestException : NetException
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

 	public class EntityException : NetException
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

	public class ResourceException : NetException
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
}