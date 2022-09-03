#pragma once
#include "Server.h"
#include "IFileSystemProvider.h"

namespace uc
{
	class UOS_LINKING CLocalFileSystemProvider : public CServer, public IFileSystemProvider
	{	
		public:
			const static inline CString Name = L"LocalFileSystemProvider";

			CList<CStream *>		Streams;
			CString						Root;

			UOS_RTTI
			CLocalFileSystemProvider(CNexus * l, CServerInstance * info);
			~CLocalFileSystemProvider();

			IInterface *				Connect(CString const & pr) override;
			void						Disconnect(IInterface * c) override;

			void						MountRoot(CXon * parameters) override;

			CList<CFileSystemEntry>		Enumerate(CString const & dir, CString const & regex) override;
			void						CreateDirectory(CString const & path) override;
			CStream *				WriteFile(CString const & path) override;
			CStream *				ReadFile(CString const & path) override;
			CAsyncFileStream *			ReadFileAsync(CString const & path) override;
			void						Close(CStream *) override;
			void						Delete(CString const & path) override;
			bool						Exists(CString const & name) override;
			CString						GetType(CString const & path) override;

			CString						MapPath(CString const & path);
	};
}
