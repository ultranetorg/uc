#pragma once
#include "ReleaseAddress.h"

namespace uc
{
	class UOS_LINKING CApplicationReleaseAddress : public CReleaseAddress
	{
		public:
			CString								Application;

			CApplicationReleaseAddress();
			CApplicationReleaseAddress(CString const & author, CString const & product, CString const & platform, CVersion & version, CString const & application);

			bool								Empty();
			static CApplicationReleaseAddress	Parse(CString const & text);

			bool								operator==(const CApplicationReleaseAddress & a) const;
			bool								operator!=(const CApplicationReleaseAddress & a) const;
			CString								ToString();
	};
}