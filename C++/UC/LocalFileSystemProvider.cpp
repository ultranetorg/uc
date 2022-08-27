#include "StdAfx.h"
#include "LocalFileSystemProvider.h"
#include "Nexus.h"

using namespace uc;

CLocalFileSystemProvider::CLocalFileSystemProvider(CNexus * l, CServerRelease * info) : CServer(l, info)
{
	//Protocols[UOS_STORAGE_PROTOCOL] = null;
}

CLocalFileSystemProvider::~CLocalFileSystemProvider()
{
}

IInterface * CLocalFileSystemProvider::Connect(CString const & pr)
{
	if(pr == InterfaceName)
	{
		return this;
	}

	return null;
}

void CLocalFileSystemProvider::Disconnect(IInterface * p)
{
	if(p == this)
	{
		for(auto i : Streams)
		{
			delete i;
		}
		Streams.clear();
	}
}

void CLocalFileSystemProvider::MountRoot(CXon * parameters)
{
	Root = parameters->Get<CString>(L"To");
}

// CObject<CDirectory> CLocalFileSystemProvider::OpenDirectory(CString const & addr)
// {
// 	auto d = Directories.Find([addr](auto i){ return i->Url.GetObjectId() == addr; });
// 
// 	if(d)
// 	{
// 		d->Take();
// 		return d;
// 	}
// 
// 	auto mount = addr.Substring(L'/', 1);
// 	auto native = Nexus->UniversalToNative(addr);
// 	
// 	if(CNativeDirectory::Exists(native))
// 	{
// 		d = new CNativeDirectory(this, addr, native);
// 
// 		Directories.push_back(d);
// 		RegisterObject(d, false);
// 		d->Free();
// 		return d;
// 	}
// 	if(addr == L"/")
// 	{
// 		RegisterObject(this, false);
// 		return this;
// 	}
// 	
// 	return null;
// }

CString CLocalFileSystemProvider::MapPath(CString const & path)
{
	auto p = path;

// 	auto known = path.Substring(L'/', 1);
// 	auto lpath = p.size() > known.size() + 1 ? path.Substring(path.find(L'/', path.find(L'/') + 1) + 1) : L"";
// 
// 	if(known == UOS_DIRECTORY_SERVERS)
// 		p = Nexus->Core->MapToDatabase(CPath::Nativize(p));
// 	else if(known == UOS_DIRECTORY_USER)
// 		p = Nexus->Core->MapToDatabase(CPath::Nativize(p));

	return CNativePath::Join(Root, CPath::Nativize(path));
}

CStream * CLocalFileSystemProvider::ReadFile(CString const & path)
{
	//auto dir = CPath::GetDirectoryPath(addr);
	//
	//if(!Exists(dir))
	//{
	//	CreateDirectory(dir);
	//}

	auto s = new CLocalFileStream(MapPath(path), EFileMode::Open);
	Streams.push_back(s);

	return s;
}

CStream * CLocalFileSystemProvider::WriteFile(CString const & path)
{
	auto f = MapPath(path);

	auto d = CNativePath::GetDirectoryPath(f);

	if(!CNativePath::IsDirectory(d))
		CNativeDirectory::CreateAll(d);

	auto s = new CLocalFileStream(f, EFileMode::New);
	Streams.push_back(s);

	return s;
}

CAsyncFileStream * CLocalFileSystemProvider::ReadFileAsync(CString const & path)
{
	auto s = new CAsyncFileStream(Nexus->Core, MapPath(path), EFileMode::Open);
	Streams.push_back(s);

	return s;
}

void CLocalFileSystemProvider::Close(CStream * f)
{
	if(Streams.Contains(f))
		Streams.Remove(f);
	
	delete f;
}

bool CLocalFileSystemProvider::Exists(CString const & path)
{
	auto n = MapPath(path);

	return CNativePath::IsDirectory(n) || CNativePath::IsFile(n);
}

void CLocalFileSystemProvider::CreateDirectory(CString const & path)
{
	CNativeDirectory::CreateAll(MapPath(path));
}

// void CStorage::CreateGlobalDirectory(CString const & path)
// {
// 	auto d = MapGlobalPath(path);
// 	CreateDirectory(d);
// }

void CLocalFileSystemProvider::Delete(CString const & path)
{
	auto p = MapPath(path);

	if(CNativePath::IsDirectory(p))
		CNativeDirectory::Delete(p);
	else if(CNativePath::IsFile(p))
		DeleteFile((L"\\\\?\\" + p).data());;
}

CList<CFileSystemEntry> CLocalFileSystemProvider::Enumerate(CString const & directory, CString const & regex)
{
	auto d = MapPath(directory);

	return CNativeDirectory::Enumerate(d, regex, SkipHidden);
}

CString CLocalFileSystemProvider::GetType(CString const & path)
{
	auto p = MapPath(path);

	if(CNativePath::IsDirectory(p))
	{
		return CFileSystemEntry::Directory;
	}
	if(CNativePath::IsFile(p))
	{
		return CFileSystemEntry::File;
	}
	else
		throw CException(HERE, L"Unknown type");
}
