#include "StdAfx.h"
#include "LocalFileSystemProvider.h"
#include "FileSystem.h"
#include "Nexus.h"

using namespace uc;

CServer * StartUosServer(CNexus * l, CServerRelease * info, CXon * command)
{
	if(info->Address.Server == CFileSystem::GetClassName())
	{
		return new CFileSystem(l, info);
	}
	if(info->Address.Server == CLocalFileSystemProvider::GetClassName())
	{
		return new CLocalFileSystemProvider(l, info);
	}

	return null;
}

void StopUosServer(CServer * s)
{
	delete s;
}
