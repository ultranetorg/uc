#pragma once
#include "Server.h"
#include "IStorageProtocol.h"

namespace uc
{
	class UOS_LINKING CLocalStorage : public CServer, public IStorageProtocol, protected CLocalDirectory
	{	
		public:
			CMap<CString, CString>		Mounts;

			CList<CStream *>			WriteStreams;
			CList<CStream *>			ReadStreams;
			CList<CLocalDirectory *>	Directories;

			UOS_RTTI
			CLocalStorage(CNexus * l, CServerInfo * info);
			~CLocalStorage();

			void						Start(EStartMode sm) override;
			IProtocol * 				Connect(CString const & pr) override;
			void						Disconnect(IProtocol * c) override;

			void						CreateDirectory(CString const & path) override;

			CStream *					OpenWriteStream(CString const & path) override;
			CStream *					OpenReadStream(CString const & path) override;
			CAsyncFileStream *			OpenAsyncReadStream(CString const & path) override;
			CObject<CDirectory>			OpenDirectory(CString const & path) override;
			void						Close(CDirectory *) override;
			void						Close(CStream *) override;
			void						DeleteFile(CString const & path) override;
			void						DeleteDirectory(CString const & path) override;
			bool						Exists(CString const & name) override;

			void						Execute(const CUrq & o, CExecutionParameters * ep);
			bool						CanExecute(const CUrq & o);


			//void						CreateGlobalDirectory(CString const & path);
			void						CreateGlobalDirectory(CStorableObject * o, CString const & path = CString());
			void						CreateGlobalDirectory(CServer * s, CString const & path = CString()) override;
			void						CreateLocalDirectory(CString const & path);
			void						CreateLocalDirectory(CStorableObject * o, CString const & path = CString());
			void 						CreateLocalDirectory(CServer * s, CString const & path = CString()) override;

			CList<CStorageEntry>		Enumerate(CString const & dir, CString const & mask) override;
			CList<CStorageEntry>		Enumerate(CString const & mask);

			CUol						ToUol(CString const & type, CString const & path) override;
			CString						GetType(CString const & path) override;
	};
}
