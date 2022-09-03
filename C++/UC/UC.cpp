#include "StdAfx.h"
#include "LocalFileSystemProvider.h"
#include "FileSystem.h"
#include "Nexus.h"

using namespace uc;

CServer * CreateUosServer(CNexus * l, CServerInstance * info)
{
	if(info->Release->Address.Server == CFileSystem::GetClassName())
	{
		return new CFileSystem(l, info);
	}
	if(info->Release->Address.Server == CLocalFileSystemProvider::GetClassName())
	{
		return new CLocalFileSystemProvider(l, info);
	}

	return null;
}

void DestroyUosServer(CServer * s)
{
	delete s;
}
