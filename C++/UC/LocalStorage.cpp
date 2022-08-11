#include "StdAfx.h"
#include "LocalStorage.h"
#include "Nexus.h"

using namespace uc;

CLocalStorage::CLocalStorage(CNexus * l, CServerInfo * info) : CServer(l, info), CLocalDirectory(this, L"/", L"")
{
	//Protocols[UOS_STORAGE_PROTOCOL] = null;
}

CLocalStorage::~CLocalStorage()
{
	while(auto i = Objects.Find([](auto j){ return j->Shared; }))
	{
		DestroyObject(i);
	}
}

IProtocol * CLocalStorage::Connect(CString const & pr)
{
	if(pr == UOS_STORAGE_PROTOCOL)
	{
		return this;
	}

	return null;
}

void CLocalStorage::Disconnect(IProtocol * p)
{
	if(p == this)
	{
		//for(auto i : Streams)
		//{
		//	Destroy(i);
		//}

		for(auto i : Directories)
		{
			DestroyObject(i);
		}

		WriteStreams.clear();
		ReadStreams.clear();
		Directories.clear();

	}
}

void CLocalStorage::Start(EStartMode sm)
{
	//CNativeDirectory::Create(Level->Core->MapToDatabase(UOS_MOUNT_GLOBAL));
	//CNativeDirectory::Create(Level->Core->MapToDatabase(UOS_MOUNT_SYSTEM_GLOBAL));
	//CNativeDirectory::Create(Level->Core->MapToDatabase(UOS_MOUNT_SYSTEM_LOCAL));
	//CNativeDirectory::Create(Level->Core->TmpFolder);
}


CObject<CDirectory> CLocalStorage::OpenDirectory(CString const & addr)
{
	auto d = Directories.Find([addr](auto i){ return i->Url.GetId() == addr; });

	if(d)
	{
		d->Take();
		return d;
	}

	auto mount = addr.Substring(L'/', 1);
	auto native = Nexus->UniversalToNative(addr);
	
	if(CNativeDirectory::Exists(native))
	{
		d = new CLocalDirectory(this, addr, native);

		Directories.push_back(d);
		RegisterObject(d, false);
		d->Free();
		return d;
	}
	if(addr == L"/")
	{
		RegisterObject(this, false);
		return this;
	}
	
	return null;
}

CStream * CLocalStorage::OpenWriteStream(CString const & addr)
{
	auto dir = CPath::GetDirectoryPath(addr);

	if(!Exists(dir))
	{
		CreateDirectory(dir);
	}
	
	CString path = addr;

	CFileStream * s = null;

	//if(path.StartsWith(UOS_MOUNT_LOCAL) || path.StartsWith(UOS_MOUNT_GLOBAL) || path.StartsWith(UOS_MOUNT_SYSTEM) || path.StartsWith(UOS_MOUNT_SYSTEM_LOCAL) || path.StartsWith(UOS_MOUNT_SYSTEM_GLOBAL))
	{
		path = Nexus->UniversalToNative(addr);
		s = new CFileStream(path, EFileMode::New);
		WriteStreams.push_back(s);
	}

	return s;
}

CStream * CLocalStorage::OpenReadStream(CString const & u)
{
	CFileStream * s = null;

	auto n = Nexus->UniversalToNative(u);

	try
	{
		s = new CFileStream(n, EFileMode::Open);
	}
	catch(CFileException & )
	{
	}

	if(s)
	{
		ReadStreams.push_back(s);
	}

	return s;
}

CAsyncFileStream * CLocalStorage::OpenAsyncReadStream(CString const & addr)
{
	auto path = Nexus->UniversalToNative(addr);
	auto s = new CAsyncFileStream(Nexus->Core, path, EFileMode::Open);
	ReadStreams.push_back(s);

	return s;
}

void CLocalStorage::Close(CStream * f)
{
	if(WriteStreams.Contains(f))
		WriteStreams.Remove(f);

	if(ReadStreams.Contains(f))
		ReadStreams.Remove(f);
	
	delete f;
}

void CLocalStorage::Close(CDirectory * d)
{
	if(d == this)
	{
		DestroyObject(d);
		return;
	}

	if(d->GetRefs() == 1)
	{
		Directories.Remove(d->As<CLocalDirectory>());
		DestroyObject(d);
	}
	else
		d->Free();
}

bool CLocalStorage::Exists(CString const & u)
{
	auto  n =  Nexus->UniversalToNative(u);

	return (CNativePath::IsDirectory(n) || CNativePath::IsFile(n));
}

void CLocalStorage::CreateDirectory(CString const & o)
{
	CNativeDirectory::CreateAll(Nexus->UniversalToNative(o), true);
}

// void CStorage::CreateGlobalDirectory(CString const & path)
// {
// 	auto d = MapGlobalPath(path);
// 	CreateDirectory(d);
// }

void CLocalStorage::CreateGlobalDirectory(CStorableObject * o, CString const & path)
{
	auto d = o->MapGlobalPath(path);
	CreateDirectory(d);
}

void CLocalStorage::CreateGlobalDirectory(CServer * s, CString const & path)
{
	auto d = s->MapUserGlobalPath(path);
	CreateDirectory(d);
}

void CLocalStorage::CreateLocalDirectory(CString const & path)
{
	auto d = MapLocalPath(path);
	CreateDirectory(d);
}

void CLocalStorage::CreateLocalDirectory(CStorableObject * o, CString const & path)
{
	auto d = o->MapLocalPath(path);
	CreateDirectory(d);
}

void CLocalStorage::CreateLocalDirectory(CServer * s, CString const & path)
{
	auto d = s->MapUserLocalPath(path);
	CreateDirectory(d);
}

CList<CStorageEntry> CLocalStorage::Enumerate(CString const & mask)
{
	CList<CStorageEntry> ooo;

	if(CNativePath::MatchWildcards(UOS_MOUNT_LOCAL, mask, false))		ooo.push_back(CStorageEntry(Nexus->MapPath(UOS_MOUNT_LOCAL, L""),		CDirectory::GetClassName()));
	if(CNativePath::MatchWildcards(UOS_MOUNT_SERVER, mask, false))		ooo.push_back(CStorageEntry(Nexus->MapPath(UOS_MOUNT_SERVER, L""),		CDirectory::GetClassName()	));
	if(CNativePath::MatchWildcards(UOS_MOUNT_USER_GLOBAL, mask, false))	ooo.push_back(CStorageEntry(Nexus->MapPath(UOS_MOUNT_USER_GLOBAL, L""),	CDirectory::GetClassName()	));
	if(CNativePath::MatchWildcards(UOS_MOUNT_USER_LOCAL, mask, false))	ooo.push_back(CStorageEntry(Nexus->MapPath(UOS_MOUNT_USER_LOCAL, L""),	CDirectory::GetClassName()	));

	return ooo;
}

void CLocalStorage::DeleteFile(CString const & f)
{
	auto path = Nexus->UniversalToNative(f);

	CLocalFile::Delete(path);
}

void CLocalStorage::DeleteDirectory(CString const & f)
{
	auto path = Nexus->UniversalToNative(f);

	CNativeDirectory::Delete(path);
}

CList<CStorageEntry> CLocalStorage::Enumerate(CString const & dir, CString const & mask)
{
	auto d = OpenDirectory(dir);
	
	if(d)
	{
		auto & o = d->Enumerate(mask);
		Close(d);
		return o;
	}
	else
		return {};
}

CUol CLocalStorage::ToUol(CString const & type, CString const & path)
{
	return CUol(CServer::Url, type + L'-' + path);
}

CString CLocalStorage::GetType(CString const & path)
{
	auto p = Nexus->UniversalToNative(path);

	if(CNativePath::IsDirectory(p))
	{
		return CDirectory::GetClassName();
	}
	if(CNativePath::IsFile(p))
	{
		return CFile::GetClassName();
	}
	else
		throw CException(HERE, L"Unknown type");
}

bool CLocalStorage::CanExecute(const CUrq & request)
{
	if(request.Protocol == CUol::Protocol)
	{
		return CUsl(request) == CServer::Url;
	}
	return false;
}

void CLocalStorage::Execute(const CUrq & u, CExecutionParameters * ep)
{
	ShellExecute(null, L"open", Nexus->UniversalToNative(u.GetObject()).data(), NULL, NULL, SW_SHOWNORMAL);
}
