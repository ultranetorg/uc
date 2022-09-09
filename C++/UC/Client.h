#pragma once
#include "ApplicationRelease.h"
#include "Interface.h"
#include "Identity.h"

namespace uc
{
	class CNexus;
	class CClient;

	struct CClientInstance
	{
		CString												Name;
		CApplicationRelease *								Release;
		CClient	*											Instance = null;
		CIdentity *											Identity = null;

		CMap<CString, IInterface *>							Interfaces;
		CMap<CString, CMap<IType *, std::function<void()>>>	Users;
	};

	class UOS_LINKING CClient
	{
		public:
 			CClientInstance *					Instance;
// 			CNexus *							Nexus;

			CClient(/*CNexus * nexus, */CClientInstance * instance);

			virtual IInterface *				Connect(CString const & iface)=0;
			virtual void						Disconnect(IInterface * iface)=0;
	};
}