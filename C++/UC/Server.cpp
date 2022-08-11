#include "stdafx.h"
#include "Server.h"
#include "Nexus.h"

using namespace uc;

CServer::CServer(CNexus * l, CServerInfo * info)
{
	Nexus = l;
	Url = info->Url;
	Info = info;
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
	if(!u.IsEmpty() && ((CUsl)u != Url))
	{
		throw CException(HERE, L"Alien system object");
	}

	return FindObject(u.Object);
}

CInterObject * CServer::FindObject(CString const & name)
{
	return Objects.Find([&name](auto i){ return i->Url.Object == name; });
}

CString CServer::MapRelative(CString const & path)
{
	return CPath::Join(Url.Server, path);
}

CString CServer::MapUserLocalPath(CString const & path)
{
	return Nexus->MapPath(UOS_MOUNT_USER_LOCAL, MapRelative(path));
}

CString CServer::MapUserGlobalPath(CString const & path)
{
	return Nexus->MapPath(UOS_MOUNT_USER_GLOBAL, MapRelative(path));
}

CString CServer::MapTmpPath(CString const & path)
{
	return Nexus->MapPath(UOS_MOUNT_SERVER_TMP, MapRelative(path));
}

CString CServer::MapPath(CString const & path)
{
	return Nexus->MapPath(UOS_MOUNT_SERVER, MapRelative(path));
}
