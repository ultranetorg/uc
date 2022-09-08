#pragma once
#include "ApplicationAddress.h"
#include "Manifest.h"

namespace uc
{
	class CNexus;
	class CServer;
	struct CServerInstance;
	class CClient;
	struct CClientInstance;

	typedef CServer *	(* FCreateUosServer)(CNexus * nexus, CServerInstance * info);
	typedef void		(* FDestroyUosServer)(CServer *);
	typedef CClient *	(* FCreateUosClient)(CNexus * nexus, CClientInstance * info);
	typedef void		(* FDestroyUosClient)(CClient *);

	struct CApplicationRelease
	{
		CApplicationAddress		Address;
		HINSTANCE				ServerModule;
		FCreateUosServer		CreateServer;
		HINSTANCE				ClientModule;
		FCreateUosClient		CreateClient;
		CManifest *				Manifest = null;
		CXonDocument *			Registry = null;

		~CApplicationRelease()
		{
			delete Registry;
		}
	};
}
