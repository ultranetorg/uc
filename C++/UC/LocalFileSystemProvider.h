#pragma once
#include "Server.h"
#include "FileSystemProviderProtocol.h"
#include "Client.h"

namespace uc
{
	class UOS_LINKING CLocalFileSystemProvider : public CServer, public CFileSystemProviderProtocol
	{	
		public:
			const static inline CString Name = L"LocalFileSystemProvider";

			CList<CStream *>			Streams;
			CString						Root;

			UOS_RTTI
			CLocalFileSystemProvider(CNexus * l, CServerInstance * info);
			~CLocalFileSystemProvider();

			IProtocol *					Accept(CString const & pr) override;
			void						Break(IProtocol * c) override;

			void						MountRoot(CXon * parameters) override;

			CList<CFileSystemEntry>		Enumerate(CString const & dir, CString const & regex) override;
			void						CreateDirectory(CString const & path) override;
			CStream *					WriteFile(CString const & path) override;
			CStream *					ReadFile(CString const & path) override;
			CAsyncFileStream *			ReadFileAsync(CString const & path) override;
			void						Close(CStream *) override;
			void						Delete(CString const & path) override;
			bool						Exists(CString const & name) override;
			CString						GetType(CString const & path) override;

			CString						MapPath(CString const & path);
	};

	class UOS_LINKING CLocalFileSystemProviderClient : public CClient, public virtual IType
	{
		public:
			CLocalFileSystemProvider * Server;

			UOS_RTTI
			CLocalFileSystemProviderClient(CNexus * nexus, CClientInstance * instance, CLocalFileSystemProvider * server) : CClient(instance)
			{
				Server = server;
			}

			virtual ~CLocalFileSystemProviderClient()
			{
			}

			IProtocol * Connect(CString const & iface) override
			{
				return Server->Accept(iface);
			}

			void Disconnect(IProtocol * iface) override
			{
				Server->Break(iface);
			}
	};
}
