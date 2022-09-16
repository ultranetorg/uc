#pragma once
#include "ApplicationRelease.h"
#include "Interface.h"
#include "Identity.h"
#include "ApplicationAddress.h"
#include "Connection.h"

namespace uc
{
	class CNexus;
	class CClient;

	struct CClientInstance
	{
		CString										Name;
		//CApplicationAddress									Address;
		CApplicationRelease *						Release;
		CClient	*									Instance = null;
		CIdentity *									Identity = null;

		CMap<CString, IProtocol *>					Interfaces;
		CMap<CString, CList<CClientConnection *>>	Users;
	};

	class UOS_LINKING CClient
	{
		public:
 			CClientInstance *					Instance;
// 			CNexus *							Nexus;

			CClient(/*CNexus * nexus, */CClientInstance * instance);

			virtual IProtocol *				Connect(CString const & iface)=0;
			virtual void						Disconnect(IProtocol * iface)=0;
	};
}