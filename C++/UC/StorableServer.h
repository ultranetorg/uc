#pragma once
#include "Server.h"
#include "IFileSystem.h"
#include "Connection.h"

namespace uc
{
	class UOS_LINKING CStorableServer : public CServer
	{
		public:
			CProtocolConnection<IFileSystem>		Storage;

			UOS_RTTI
			CStorableServer(CNexus * l, CServerRelease * info);
			~CStorableServer();

			using									CServer::FindObject;

			CInterObject *							FindObject(CString const & name) override;

			bool									Exists(CString const & name);
			void									DeleteObject(CInterObject * r);
			void									DestroyObject(CInterObject * o, bool save);
			void									LoadObject(CStorableObject * o);

			CTonDocument *							LoadReleaseDocument(CString const & path);
			CTonDocument *							LoadGlobalDocument(CString const & path);

		protected:
			using									CServer::DestroyObject;

	};
}

