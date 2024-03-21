#pragma once
#include "String.h"

namespace uc
{
	class UOS_LINKING CProductAddress
	{
		public:
			CString					Author;
			CString					Product;
			const static wchar_t	SlashSeparator = L'/';
			const static wchar_t	DotSeparator = L'.';

			CProductAddress();
			CProductAddress(CString const & author, CString const & product);

			static CProductAddress	Parse(CString const & text);

			bool					operator==(const CProductAddress & a) const;
			bool					operator!=(const CProductAddress & a) const;
			CString					ToString();
	};
}