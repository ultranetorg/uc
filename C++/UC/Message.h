#pragma once
#include "String.h"
#include "BinaryReader.h"
#include "BinaryWriter.h"

namespace uc
{
	enum class EMessage : byte
	{
		Null, Stop
	};

	struct CMessage
	{
		//CString			Sender;

		virtual byte	GetCode()=0;

		virtual void Write(CBinaryWriter * writer)
		{
		}
		
		virtual void Read(CBinaryReader * reader)
		{
		
		}
	};

	struct CStopMessage : public CMessage
	{
		byte GetCode()
		{
			return (byte)EMessage::Stop;
		}
	};
}
