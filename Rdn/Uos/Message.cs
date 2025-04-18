﻿using System;
using System.IO;
using System.Reflection;
using Uccs.Net;

namespace Uccs.Sun.Application
{
	internal enum MessageType
	{
		Null, Stop
	}

	internal class Message
	{
		public MessageType Type => Enum.Parse<MessageType>(GetType().Name);

		public static Message FromType(MessageType type)
		{
			try
			{
				return Assembly.GetExecutingAssembly().GetType(typeof(Message).Namespace + "." + type + "Message").GetConstructor(new Type[]{}).Invoke(new object[]{}) as Message;
			}
			catch(Exception ex)
			{
				throw new IntegrityException($"Wrong {nameof(Message)} type", ex);
			}
		}


		public virtual void Read(BinaryReader r)
		{
		}

		public virtual void Write(BinaryWriter w)
		{
		}
	}

	internal class StopMessage : Message
	{
	}
}
