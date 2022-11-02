#pragma once
#include "IType.h"

namespace uc
{
	struct CLevel2;

	class CJsonClient : virtual public IType
	{
		public:
			CLevel2 *		Level;

			UOS_RTTI
			CJsonClient(CLevel2 * l);
			~CJsonClient();
			
			//void			FindReleases(const CString & product, CString const & platfrom, std::function<void(CArray<uint256> &)> ok);
			//void			GetRelease(CString const & product, uint256 id, std::function<void(CString, CVersion, CString)> ok);
	};
}