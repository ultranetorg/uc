#pragma once
#include "Server.h"
#include "Client.h"
#include "FileSystemProtocol.h"
#include "ExecutorProtocol.h"
#include "Connection.h"

namespace uc
{
	class UOS_LINKING CFileSystem : public CServer, public CFileSystemProtocol, public CExecutorProtocol
	{	
		public:
			struct CMapping
			{
				CString							Path;
				CFileSystemProviderProtocol	*	Provider;
			};

			CMap<CString, CConnection<CFileSystemProviderProtocol>>		Mounts;
			CMap<CStream *, CMapping>									Streams;

			UOS_RTTI
			CFileSystem(CNexus * l, CServerInstance * info);
			~CFileSystem();

			void								UserStart() override;
			IProtocol *							Accept(CString const & pr) override;
			void								Break(IProtocol * c) override;

			void								Execute(CXon * arguments, CExecutionParameters * parameter) override;

			CMapping							FindMount(CString const & path);

			virtual CUol						ToUol(CString const & path) override;
			virtual CList<CFileSystemEntry>		Enumerate();
			
			void								Mount(CString const & path, CApplicationReleaseAddress & provider, CXon * parameters) override;
			CString								UniversalToNative(CString const & path) override;
			CString								NativeToUniversal(CString const & path) override;
			CList<CFileSystemEntry>				Enumerate(CString const & path, CString const & regex) override;
			void								CreateDirectory(CString const & path) override;
			IDirectory *						OpenDirectory(CString const & path) override;
			CStream *							WriteFile(CString const & path) override;
			CStream *							ReadFile(CString const & path) override;
			CAsyncFileStream *					ReadFileAsync(CString const & path) override;
			void								Close(CStream *) override;
			void								Close(IDirectory *) override;
			void								Delete(CString const & path) override;
			bool								Exists(CString const & name) override;
			CString								GetType(CString const & path) override;

			void								CreateGlobalDirectory(CInterObject * o, CString const & path = CString());
			void								CreateLocalDirectory(CInterObject * o, CString const & path = CString());
			void								CreateGlobalDirectory(CServer * s, CString const & path = CString());
			void 								CreateLocalDirectory(CServer * s, CString const & path = CString());


	};

	class UOS_LINKING CFileSystemClient : public CClient, public virtual IType
	{
		public:
			CFileSystem * Server;

			UOS_RTTI
			CFileSystemClient(CNexus * nexus, CClientInstance * instance, CFileSystem * server) : CClient(instance)
			{
				Server = server;
			}

			virtual ~CFileSystemClient()
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
