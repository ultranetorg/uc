#pragma once
#include "ApplicationReleaseAddress.h"
#include "Manifest.h"

namespace uc
{
	class CNexus;
	class CServer;
	class CClient;
	struct CServerInstance;
	struct CClientInstance;

	typedef CServer *	(* FCreateUosServer)(CNexus * nexus, CServerInstance * info);
	typedef void		(* FDestroyUosServer)(CServer *);
	typedef CClient *	(* FCreateUosClient)(CNexus * nexus, CClientInstance * info);
	typedef void		(* FDestroyUosClient)(CClient *);

	struct CApplicationRelease
	{
		CApplicationReleaseAddress		Address;
		HINSTANCE						Module;
		CCompiledManifest *				Manifest = null;
		CXonDocument *					Registry = null;

		~CApplicationRelease()
		{
			delete Registry;
		}
	};
}
