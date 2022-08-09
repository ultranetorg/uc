#pragma once
#include "Server.h"
#include "IStorageProtocol.h"
#include "Connection.h"

namespace uc
{
	class UOS_LINKING CStorableServer : public CServer
	{
		public:
			CProtocolConnection<IStorageProtocol>	Storage;

			UOS_RTTI
			CStorableServer(CNexus * l, CServerInfo * info);
			~CStorableServer();

			using									CServer::DestroyObject;
			using									CServer::FindObject;

			CBaseNexusObject *						FindObject(CString const & name) override;

			bool									Exists(CString const & name);
			void									DeleteObject(CBaseNexusObject * r);
			void									DestroyObject(CBaseNexusObject * o, bool save);
			void									LoadObject(CNexusObject * o);

			CTonDocument *							LoadServerDocument(CString const & path);
			CTonDocument *							LoadGlobalDocument(CString const & path);


	};
}

