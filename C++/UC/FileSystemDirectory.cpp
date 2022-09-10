#include "StdAfx.h"
#include "FileSystemDirectory.h"
#include "FileSystemServer.h"

using namespace uc;

CFileSystemDirectory::CFileSystemDirectory(CFileSystemServer * filesystem, CString const & path)
{
	FileSystem = filesystem;
	Path = path;
}

CList<CFileSystemEntry> CFileSystemDirectory::Enumerate(CString const & regex)
{
	return FileSystem->Enumerate(Path, regex);
}

CStream * CFileSystemDirectory::WriteFile(CString const & path)
{
	return FileSystem->WriteFile(CPath::Join(Path, path));
}

CStream * CFileSystemDirectory::ReadFile(CString const & path)
{
	return FileSystem->ReadFile(CPath::Join(Path, path));
}

void CFileSystemDirectory::Close(CStream * s)
{
	return FileSystem->Close(s);
}

void CFileSystemDirectory::Delete(CString const & path)
{
	return FileSystem->Delete(CPath::Join(Path, path));
}
