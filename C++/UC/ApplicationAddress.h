#pragma once
#include "ReleaseAddress.h"

namespace uc
{
	class UOS_LINKING CApplicationAddress : public CReleaseAddress
	{
		public:
			CString						Application;

			CApplicationAddress();
			CApplicationAddress(CString const & author, CString const & product, CString const & platform, CVersion & version, CString const & application);

			bool						Empty();
			static CApplicationAddress	Parse(CString const & text);

			bool						operator==(const CApplicationAddress & a) const;
			bool						operator!=(const CApplicationAddress & a) const;
			CString						ToString();
	};
}