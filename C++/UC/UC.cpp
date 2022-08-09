#include "StdAfx.h"
#include "LocalStorage.h"
#include "Nexus.h"

using namespace uc;

static CLocalStorage * Storage = null;

CServer * StartUosServer(CNexus * l, CServerInfo * info)
{
	Storage = new CLocalStorage(l, info);
	return Storage;
}

void StopUosServer()
{
	delete Storage;
}
