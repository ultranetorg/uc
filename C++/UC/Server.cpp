#include "stdafx.h"
#include "Server.h"
#include "Nexus.h"

using namespace uc;

CServer::CServer(CNexus * l, CServerInstance * info)
{
	Nexus = l;
	Instance = info;
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

	return CPath::Join(CFileSystem::Servers, CPath::Join(Instance->Name, path));
}

CString CServer::MapSystemPath(CString const & path)
{
	return CPath::Join(CFileSystem::System, Instance->Name, path);
}

CString CServer::MapSystemTmpPath(CString const & path)
{
	return CPath::Join(CFileSystem::SystemTmp, Instance->Name, path);
}

CString CServer::MapUserLocalPath(CString const & path)
{
	return CPath::Join(CFileSystem::UserLocal, Instance->Name, path);
}

CString CServer::MapUserGlobalPath(CString const & path)
{
	return CPath::Join(CFileSystem::UserGlobal, Instance->Name, path);
}

CString CServer::MapUserTmpPath(CString const & path)
{
	return CPath::Join(CFileSystem::UserTmp, Instance->Name, path);
}
