#pragma once
#include "ProductAddress.h"

namespace uc
{
	class UOS_LINKING CRealizationAddress : public CProductAddress
	{
		public:
			CString										Platform;

			CRealizationAddress();
			CRealizationAddress(CString const & author, CString const & product, CString const & platform);

			static CRealizationAddress					Parse(CString const & text);

			bool										operator==(const CRealizationAddress & a) const;
			bool										operator!=(const CRealizationAddress & a) const;
			CString										ToString();
	};
}
