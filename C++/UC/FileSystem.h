#pragma once
#include "Server.h"
#include "Client.h"
#include "IFileSystem.h"
#include "IExecutor.h"
#include "Connection.h"

namespace uc
{
	class UOS_LINKING CFileSystem : public CServer, public IFileSystem, public IExecutor
	{	
		public:
			struct CMapping
			{
				CString					Path;
				IFileSystemProvider	*	Provider;
			};

			CMap<CString, CProtocolConnection<IFileSystemProvider>>	Mounts;
			CMap<CStream *, CMapping>								Streams;

			UOS_RTTI
			CFileSystem(CNexus * l, CServerInstance * info);
			~CFileSystem();

			IInterface *						Connect(CString const & pr);
			void								Disconnect(IInterface * c);

			void								Execute(CXon * arguments, CExecutionParameters * parameter) override;

			CMapping							FindMount(CString const & path);

			virtual CUol						ToUol(CString const & path) override;
			virtual CList<CFileSystemEntry>		Enumerate();
			
			void								Mount(CString const & path, CApplicationAddress & provider, CXon * parameters) override;
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

			IInterface * Connect(CString const & iface) override
			{
				return Server->Connect(iface);
			}

			void Disconnect(IInterface * iface) override
			{
				Server->Disconnect(iface);
			}
	};
}
