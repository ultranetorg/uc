#include "stdafx.h"
#include "Server.h"
#include "Nexus.h"

using namespace uc;

CServer::CServer(CNexus * l, CServerRelease * info)
{
	Nexus = l;
	Release = info;
}

CServer::~CServer()
{
}

CInterObject * CServer::CreateObject(CString const & name)
{
	return null;
}

void CServer::RegisterObject(CInterObject * o, bool shared)
{
	if(Objects.Has([o](auto i){ return i->Url.Object == o->Url.Object; }))
	{	
		throw CException(HERE, L"Already registered");
	}

	o->Take();

	o->Shared = shared;
	Objects.push_back(o);
}

void CServer::DestroyObject(CInterObject * o)
{
	if(Objects.Contains(o))
		Objects.Remove(o);
	else
		return;

	o->Destroying(o);
	o->Free();
}

CInterObject * CServer::FindObject(CUol const & u)
{
// 	if(!u.IsEmpty() && (CUsl)u != Url)
// 	{
// 		throw CException(HERE, L"Alien system object");
// 	}

	return FindObject(u.Object);
}

CInterObject * CServer::FindObject(CString const & name)
{
	return Objects.Find([&name](auto i){ return i->Url.Object == name; });
}

CString CServer::MapReleasePath(CString const & path)
{
	//auto & r = Release->Address;

	return CPath::Join(IFileSystem::Servers, CPath::Join(Instance, path));
}

CString CServer::MapSystemPath(CString const & path)
{
	return CPath::Join(IFileSystem::System, Instance, path);
}

CString CServer::MapSystemTmpPath(CString const & path)
{
	return CPath::Join(IFileSystem::SystemTmp, Instance, path);
}

CString CServer::MapUserLocalPath(CString const & path)
{
	return CPath::Join(IFileSystem::UserLocal, Instance, path);
}

CString CServer::MapUserGlobalPath(CString const & path)
{
	return CPath::Join(IFileSystem::UserGlobal, Instance, path);
}

CString CServer::MapUserTmpPath(CString const & path)
{
	return CPath::Join(IFileSystem::UserTmp, Instance, path);
}
