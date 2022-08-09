#pragma once
#include "File.h"
#include "Directory.h"
#include "Path.h"
#include "AsyncFileStream.h"

namespace uc
{
	#define UOS_STORAGE_PROTOCOL							L"Uos.Storage"
	//#define UOS_FILE_SYSTEM								L"uc-storage"
	//#define UOS_FILE_SYSTEM_ORIGIN						L"uc/storage"

	class CStorableServer;

	class IStorageProtocol : public virtual IProtocol
	{
		public:
			virtual void								CreateDirectory(CString const & path)=0;

			virtual void								CreateLocalDirectory(CServer * s, CString const & path = CString())=0;
			virtual void								CreateGlobalDirectory(CServer * s, CString const & path = CString())=0;

			virtual CStream *							OpenWriteStream(CString const & path)=0;
			virtual CStream *							OpenReadStream(CString const & path)=0;
			virtual CObject<CDirectory>					OpenDirectory(CString const & path)=0;
			virtual void								Close(CStream *)=0;
			virtual void								Close(CDirectory *)=0;
			virtual void								DeleteFile(CString const & path)=0;
			virtual void								DeleteDirectory(CString const & path)=0;
			virtual bool								Exists(CString const & name)=0;

			virtual CAsyncFileStream *					OpenAsyncReadStream(CString const & path)=0;

			virtual CList<CStorageEntry>				Enumerate(CString const & dir, CString const & mask)=0;

			virtual CUol								ToUol(CString const & type, CString const & path)=0;
			virtual CString								GetType(CString const & path)=0;

			virtual										~IStorageProtocol(){}
	};
}


