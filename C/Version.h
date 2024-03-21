#pragma once
#include "String.h"

namespace uc
{
	class UOS_LINKING CVersion
	{
		public:
		
			CVersion static								FromModule(HMODULE m);
			CVersion static								FromFile(const CString & path);
			CString	static								GetStringInfo(const CString & path, const CString & v);
		
			CString										ToString();
		
			bool										IsGreaterER(const CVersion &);
			bool										IsGreaterOrEqER(const CVersion &);

			uint										Era;
			uint										Release;
			uint										Build;
			
			bool 										operator == (CVersion const &) const;
			bool 										operator != (CVersion const &) const;
			bool 										operator < (CVersion &);
			bool 										operator > (CVersion &);
						
			CVersion static								GetFrameworkVerision();

			CVersion();
			CVersion(const CString & v);
			CVersion(uint e, uint r, uint b);
			~CVersion();
	};
}

