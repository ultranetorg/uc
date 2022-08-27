#pragma once
#include "Path.h"
#include "AsyncFileStream.h"
#include "Interface.h"
#include "FileSystemEntry.h"

namespace uc
{
#undef CreateDirectory

	class IFileSystemProvider : public virtual IInterface
	{
		public:
			inline static const CString   		InterfaceName = L"IFileSystemProvider";

			virtual void						MountRoot(CXon * parameters)=0;

			virtual CList<CFileSystemEntry>		Enumerate(CString const & dir, CString const & mask)=0;
			virtual void						CreateDirectory(CString const & path)=0;
			virtual CStream *					WriteFile(CString const & path)=0;
			virtual CStream *					ReadFile(CString const & path)=0;
			virtual CAsyncFileStream *			ReadFileAsync(CString const & path)=0;
			virtual void						Close(CStream *)=0;
			virtual void						Delete(CString const & path)=0;
			virtual bool						Exists(CString const & name)=0;
			virtual CString						GetType(CString const & path)=0;

			virtual								~IFileSystemProvider(){}
	};
}


