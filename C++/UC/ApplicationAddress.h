#pragma once
#include "RealizationAddress.h"

namespace uc
{
	class UOS_LINKING CApplicationAddress : public CRealizationAddress
	{
		public:
			CString						Application;

			CApplicationAddress();
			CApplicationAddress(CString const & author, CString const & product, CString const & platform, CString const & application);

			bool						Empty();
			static CApplicationAddress	Parse(CString const & text);

			bool						operator==(const CApplicationAddress & a) const;
			bool						operator!=(const CApplicationAddress & a) const;
			CString						ToString();
	};
}