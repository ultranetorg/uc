#pragma once
#include "Storage.h"

namespace uc
{
	class CProductRelease
	{
		public:
			CString		Product;
			CVersion	Version;
			CString		Cid;
	};

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