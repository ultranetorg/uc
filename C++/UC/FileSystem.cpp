#include "StdAfx.h"
#include "FileSystem.h"
#include "Nexus.h"
#include "LocalFileSystemProvider.h"
#include "FileSystemDirectory.h"

using namespace uc;

CFileSystem::CFileSystem(CNexus * l, CServerRelease * info) : CServer(l, info)
{
	//Protocols[UOS_STORAGE_PROTOCOL] = null;
}

CFileSystem::~CFileSystem()
{
}

IInterface * CFileSystem::Connect(CString const & pr)
{
	return this;
}

void CFileSystem::Disconnect(IInterface * p)
{
	if(p == this)
	{
		for(auto & i : Streams)
		{
			i.second.Provider->Close(i.first);
		}
		Streams.clear();

		for(auto & i : Mounts)
		{
			Nexus->Disconnect(i.second);
		}
		Mounts.clear();
	}
}

void CFileSystem::Execute(CXon * command, CExecutionParameters * parameter)
{
	auto f = command->Nodes.First();

	if(f->Name == L"Open" && f->Any(L"Url"))
	{
		CUrl o(f->Get<CString>(L"Url"));

		if(o.Scheme == CFileSystemEntry::Scheme)
		{
			Nexus->Core->Execute(UniversalToNative(CUol(CUol(f->Get<CString>(L"Url"))).Object));
		}
	}
}

void CFileSystem::Mount(CString const & path, CServerAddress & provider, CXon * parameters)
{
	auto s = Nexus->CreateServer(provider, provider.Server + path, null, null);
	
	auto p = Nexus->Connect<IFileSystemProvider>(this, s->Instance, [&, path]
																	{
																		Mounts.Remove(path);
																	});
	if(p)
	{
		p->MountRoot(parameters);
		Mounts[path] = p;
	}
	else
		throw CException(HERE, L"Failed to connecting filesystem provider");
}

CString CFileSystem::UniversalToNative(CString const & path)
{
	auto m = FindMount(path);

	if(m.Provider)
	{
		if(auto a = dynamic_cast<CLocalFileSystemProvider *>(m.Provider))
		{
			return a->MapPath(m.Path);
		}
	}

	auto p = path;
	auto mount = L"/" + p.Substring(L'/', 1);
	auto & lpath = path.size() > mount.size() ? p.Substring(p.find(L'/', p.find(L'/') + 1) + 1) : L"";

	if(mount == IFileSystem::This)
	{
		if(lpath.empty())
			return L"\\";
		if(lpath.size() == 2 && lpath[1] == L':')
			return lpath + L"\\";
		else
			p = CPath::Nativize(lpath);
	}
	//else if(mount == UOS_MOUNT_PERSONAL)
	//{
	//	path = Level->Core->MapToGlobal(CPath::Nativize(lpath));
	//}
	//else if(mount == UOS_MOUNT_CORE)
	//{
	//	path = Resolve(Nexus->Core->GetPathTo(ESystemPath::Root, CPath::Nativize(lpath)));
	//}
	else if(mount == IFileSystem::Software)
	{
		//if(lpath.empty())
			p = Nexus->Core->Resolve(Nexus->Core->MapPath(ESystemPath::Software, CPath::Nativize(lpath)));
		//else
		//{
		//	auto & s = lpath.Substring(L'/', 0);
		//	auto & r = s.length() < lpath.length() ? lpath.Substring(s.length() + 1) : L"";
		//	path = Nexus->Resolve(Nexus->MapPathToRelease(Servers.Find([&](auto i){ return i->Name == s; })->Address, CPath::Nativize(r)));
		//}
	}
	else if(mount == IFileSystem::Servers)
	{
		auto sep = lpath.find(L'/');
		auto s = lpath.Substring(0, sep);

		if(s.empty())
			throw CMappingExcepion(HERE);

		auto & l = sep != CString::npos ? lpath.Substring(sep + 1) : L"";
		auto & r = Nexus->Servers.Find([&](auto i){ return i->Instance == s; })->Release->Address;
		p = Nexus->Core->Resolve(Nexus->Core->MapPath(ESystemPath::Software, CPath::Nativize(CPath::Join(r.Author + L"-" + r.Product + L"-" + r.Platform, r.Version.ToString(), l))));
	}
	else if(mount == IFileSystem::System)
	{
		p = Nexus->Core->MapPath(ESystemPath::System, CPath::Nativize(lpath));
	}
	else if(mount == IFileSystem::SystemTmp)
	{
		p = Nexus->Core->MapPath(ESystemPath::Tmp, CPath::Nativize(lpath));
	}
	else
		throw CMappingExcepion(HERE);
	///else if(mount == UOS_MOUNT_USER_LOCAL)
	///{
	///	path = Nexus->Core->MapToDatabase(CPath::Nativize(path));
	///}
	///else if(mount == UOS_MOUNT_USER_GLOBAL)
	///{
	///	path = Nexus->Core->MapToDatabase(CPath::Nativize(path));
	///}

	return p;	
}

CString CFileSystem::NativeToUniversal(CString const & path)
{
	return IFileSystem::This + L"/" + CPath::Universalize(path);
}

CUol CFileSystem::ToUol(CString const & path)
{
	return CUol(CFileSystemEntry::Scheme, Instance, path);
}

CString CFileSystem::GetType(CString const & path)
{
	auto m = FindMount(path);

	if(m.Provider == null)
		throw CException(HERE, L"Nothing is mounted");

	return m.Provider->GetType(m.Path);
}

CList<CFileSystemEntry> CFileSystem::Enumerate()
{
	CList<CFileSystemEntry> ooo;

	for(auto & i : Mounts)
	{
		ooo.push_back(CFileSystemEntry(i.first, CFileSystemEntry::Directory));
	}

	//if(CNativePath::MatchWildcards(UOS_MOUNT_THIS, mask, false))			ooo.push_back(CFileSystemEntry(CPath::Join(UOS_MOUNT_THIS, L""),		CDirectory::GetClassName()));
	//if(CNativePath::MatchWildcards(UOS_MOUNT_SERVERS, mask, false))		ooo.push_back(CFileSystemEntry(CPath::Join(UOS_MOUNT_SERVERS, L""),		CDirectory::GetClassName()));
	//if(CNativePath::MatchWildcards(UOS_MOUNT_USER_GLOBAL, mask, false))	ooo.push_back(CFileSystemEntry(CPath::Join(UOS_MOUNT_USER_GLOBAL, L""),	CDirectory::GetClassName()));
	//if(CNativePath::MatchWildcards(UOS_MOUNT_USER_LOCAL, mask, false))	ooo.push_back(CFileSystemEntry(CPath::Join(UOS_MOUNT_USER_LOCAL, L""),	CDirectory::GetClassName()));

	return ooo;
}

CFileSystem::CMapping CFileSystem::FindMount(CString const & path)
{
	auto p = path;

	while(!Mounts.Contains(p))
	{
		p = CPath::GetDirectoryPath(p);

		if(p == L"/")
		{
			return CMapping{L"", null};
		}
	}

	return CMapping{path.Substring(p.length() + 1), Mounts(p)};
}

CList<CFileSystemEntry>	CFileSystem::Enumerate(CString const & path, CString const & regex)
{
	if(path == L"/")
	{
		auto o = CList<CFileSystemEntry>{	{L"This", CFileSystemEntry::Directory},
											{L"System", CFileSystemEntry::Directory},
											{L"Servers", CFileSystemEntry::Directory},
											{L"Software", CFileSystemEntry::Directory}};


		for(auto & i : Mounts)
		{
			auto r = i.first.Substring(L"/", 1);

			if(!o.Has([&](auto i){ return i.Name == r; }))
			{
				o.push_back(CFileSystemEntry(r, CFileSystemEntry::Directory));
			}
		}

		return o;
	}

	auto m = FindMount(path);

	if(m.Provider)
		return m.Provider->Enumerate(m.Path, regex);
	else
		return CNativeDirectory::Enumerate(UniversalToNative(path), regex, SkipHidden);
}

void CFileSystem::CreateDirectory(CString const & path)
{
	auto m = FindMount(path);

	if(m.Provider)
		return m.Provider->CreateDirectory(m.Path);
	else
		return CNativeDirectory::CreateAll(UniversalToNative(path));
}

IDirectory * CFileSystem::OpenDirectory(CString const & path)
{
	return new CFileSystemDirectory(this, path);
}

CStream * CFileSystem::WriteFile(CString const & path)
{
	auto & m = FindMount(path);

	CStream * s = null;

	if(m.Provider)
		s = m.Provider->WriteFile(m.Path);
	else
		s = new CLocalFileStream(UniversalToNative(path), EFileMode::New);

	Streams[s] = m;

	return s;
}

CStream * CFileSystem::ReadFile(CString const & path)
{
	auto & m = FindMount(path);

	CStream * s = null;

	if(m.Provider)
		s = m.Provider->ReadFile(m.Path);
	else
		s = new CLocalFileStream(UniversalToNative(path), EFileMode::Open);

	Streams[s] = m;

	return s;
}

CAsyncFileStream * CFileSystem::ReadFileAsync(CString const & path)
{
	auto & m = FindMount(path);

	CAsyncFileStream * s = null;

	if(m.Provider)
		s = m.Provider->ReadFileAsync(m.Path);
	else
		s = new CAsyncFileStream(Nexus->Core, UniversalToNative(path), EFileMode::Open);

	Streams[s] = m;

	return s;
}

void CFileSystem::Close(CStream * stream)
{
	auto & m = Streams(stream);

	if(m.Provider)
		m.Provider->Close(stream);
	else
		delete stream;

	Streams.Remove(stream);
}

void CFileSystem::Close(IDirectory * directory)
{
	delete directory;
}

bool CFileSystem::Exists(CString const & path)
{
	auto m = FindMount(path);

	if(m.Provider)
		return m.Provider->Exists(m.Path); 
	else
		return CNativePath::IsFile(UniversalToNative(path)) || CNativePath::IsDirectory(UniversalToNative(path));
}

void CFileSystem::Delete(CString const & path)
{
	auto m = FindMount(path);

	if(m.Provider == null)
		throw CException(HERE, L"Nothing is mounted");

	return m.Provider->Delete(m.Path);
}


// void CStorage::CreateGlobalDirectory(CString const & path)
// {
// 	auto d = MapGlobalPath(path);
// 	CreateDirectory(d);
// }

void CFileSystem::CreateGlobalDirectory(CInterObject * o, CString const & path)
{
	auto d = o->MapGlobalPath(path);
	CreateDirectory(d);
}

void CFileSystem::CreateGlobalDirectory(CServer * s, CString const & path)
{
	auto d = s->MapUserGlobalPath(path);
	CreateDirectory(d);
}

void CFileSystem::CreateLocalDirectory(CInterObject * o, CString const & path)
{
	auto d = o->MapLocalPath(path);
	CreateDirectory(d);
}

void CFileSystem::CreateLocalDirectory(CServer * s, CString const & path)
{
	auto d = s->MapUserLocalPath(path);
	CreateDirectory(d);
}

