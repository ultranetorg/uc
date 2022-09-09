#include "StdAfx.h"
#include "LocalFileSystemProvider.h"
#include "FileSystem.h"
#include "Nexus.h"
#include "Fun.h"

using namespace uc;

CList<CServer *>	Servers;
CList<CClient *>	Clients;

CServer * CreateUosServer(CNexus * l, CServerInstance * info)
{
	CServer * s = null;

	if(info->Release->Address.Application == CFileSystem::GetClassName())
	{
		s = new CFileSystem(l, info);
	}
	if(info->Release->Address.Application == CLocalFileSystemProvider::GetClassName())
	{
		s = new CLocalFileSystemProvider(l, info);
	}

	if(s)
	{
		Servers.push_back(s);
	}

	return s;
}

void DestroyUosServer(CServer * s)
{
	delete s;
}

CClient * CreateUosClient(CNexus * nexus, CClientInstance * info)
{
	CClient * c = null;

	if(info->Release->Address.Application == CFileSystem::GetClassName())
	{
		auto s = Servers.Find([&](auto i){ return i->Instance->Name == info->Name; });
		c = new CFileSystemClient(nexus, info, dynamic_cast<CFileSystem *>(s));
	}
	else if(info->Release->Address.Application == CLocalFileSystemProvider::GetClassName())
	{
		auto s = Servers.Find([&](auto i){ return i->Instance->Name == info->Name; });
		c = new CLocalFileSystemProviderClient(nexus, info, dynamic_cast<CLocalFileSystemProvider *>(s));
	}
	else if(info->Release->Address.Application == L"Fun")
	{
		c = new CFunClient(nexus, info);
	}

	if(c)
	{
		Clients.push_back(c);
	}

	return c;

}

void DestroyUosClient(CClient * client)
{
	delete client;
}