#pragma once
#include "Version.h"

namespace uc
{
	class UOS_LINKING CReleaseAddress
	{
		public:
			CString										Author;
			CString										Product;
			CString										Platform;
			CVersion									Version;

			CReleaseAddress();
			CReleaseAddress(CString const & author, CString const & product, CString const & platform, CVersion & version);

			static CReleaseAddress						Parse(CString const & text);
			
			bool										operator==(const CReleaseAddress & a) const;
			bool										operator!=(const CReleaseAddress & a) const;
			virtual CString								ToString();
	};
}