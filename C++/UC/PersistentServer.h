#pragma once
#include "Server.h"
#include "FileSystemProtocol.h"
#include "Connection.h"

namespace uc
{
	class UOS_LINKING CPersistentServer : public CServer
	{
		public:
			CConnection<CFileSystemProtocol>	Storage;

			UOS_RTTI
			CPersistentServer(CNexus * l, CServerInstance * info);
			~CPersistentServer();

			using								CServer::FindObject;

			CInterObject *						FindObject(CString const & name) override;

			bool								Exists(CString const & name);
			void								DeleteObject(CInterObject * r);
			void								DestroyObject(CInterObject * o, bool save);
			void								LoadObject(CPersistentObject * o);

			CTonDocument *						LoadReleaseDocument(CString const & path);
			CTonDocument *						LoadGlobalDocument(CString const & path);

		protected:
			using								CServer::DestroyObject;

	};
}

