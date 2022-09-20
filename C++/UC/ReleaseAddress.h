#pragma once
#include "RealizationAddress.h"
#include "Version.h"

namespace uc
{
	class UOS_LINKING CReleaseAddress : public CRealizationAddress
	{
		public:
			CVersion									Version;

			CReleaseAddress();
			CReleaseAddress(CString const & author, CString const & product, CString const & platform, CVersion & version);

			static CReleaseAddress						Parse(CString const & text);
			
			bool										operator==(const CReleaseAddress & a) const;
			bool										operator!=(const CReleaseAddress & a) const;
			CString										ToString();
	};
}