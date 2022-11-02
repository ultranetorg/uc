#pragma once
#include "String.h"

namespace uc
{
	class CHex
	{
		public:
			static CArray<byte>			ToBytes(CString const & text);
			static CString				ToString(CArray<byte> & bytes);
	};
}

