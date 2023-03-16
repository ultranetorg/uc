#pragma once
#include "Array.h"
#include "Stream.h"
#include "String.h"

namespace uc
{
	struct CFileSystemEntry
	{
		inline static const CString   	File = L"File";
		inline static const CString   	Directory = L"Directory";
		inline static const CString   	Scheme = L"filesystementry";

		CString							Name;
		CString							Type;
		CString							NameOverride;

		CFileSystemEntry(){}
		CFileSystemEntry(CString const & name) : Name(name){}
		CFileSystemEntry(CString const & name, CString const & t) : Name(name), Type(t) {} 
	};

	class IDirectory
	{
		public:
			virtual CList<CFileSystemEntry> 	Enumerate(CString const & regex) = 0;
			virtual CStream *					WriteFile(CString const & path) = 0;
			virtual CStream * 					ReadFile(CString const & path) = 0;
			virtual void						Close(CStream * s) = 0;
			virtual void						Delete(CString const & path) = 0;

			virtual ~IDirectory(){}
	};
}