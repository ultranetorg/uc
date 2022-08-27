#pragma once
#include "Guid.h"
#include "FileSystemEntry.h"
#include "NativePath.h"

namespace uc
{
	enum EDirectoryFlag
	{
		SkipHidden = 4, DirectoriesOnly = 8, FilesOnly = 16
	};


	class UOS_LINKING CNativeDirectory
	{
		public:
			static CList<CFileSystemEntry>				Enumerate(CString const & folder, const CString & mask, EDirectoryFlag f);

			static void									Delete(CString const & src, bool premove = true);
			static void									Copy(CString const & src, CString const & dst);
			static void									Create(CString const & src);
			static void									CreateAll(const CString & path);
			static void									Clear(CString const & src);
			static bool									Exists(CString const & l);

	};
}