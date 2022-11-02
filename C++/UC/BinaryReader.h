#pragma once
#include "Stream.h"

namespace uc
{
	class CBinaryWriter
	{
		public:
			CStream * Stream;

			CBinaryWriter(CStream * stream)
			{
				Stream = stream;
			}

			void Write(byte val)
			{
				Stream->Write(&val, sizeof(byte));
			}

			void Write(int val)
			{
				Stream->Write(&val, sizeof(int));
			}
	};
}

