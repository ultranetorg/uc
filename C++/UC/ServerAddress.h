#pragma once
#include "ReleaseAddress.h"

namespace uc
{
	class UOS_LINKING CServerAddress : public CReleaseAddress
	{
		public:
			CString										Server;

			CServerAddress();
			CServerAddress(CString const & author, CString const & product, CString const & platform, CVersion & version, CString const & server);

			static CServerAddress						Parse(CString const & text);

			bool										operator==(const CServerAddress & a) const;
			bool										operator!=(const CServerAddress & a) const;
			virtual CString								ToString();
	};
}