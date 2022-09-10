#pragma once
#include "FileSystemEntry.h"

namespace uc
{
	class CFileSystemServer;

	class CFileSystemDirectory : public IDirectory
	{
		public:
			CFileSystemServer	*			FileSystem;
			CString					Path;

			CFileSystemDirectory(CFileSystemServer * filesystem, CString const & path);

			CList<CFileSystemEntry> Enumerate(CString const & regex) override;
			CStream *			WriteFile(CString const & path) override;
			CStream *			ReadFile(CString const & path) override;
			void					Close(CStream * s) override;
			void					Delete(CString const & path) override;
	};
}