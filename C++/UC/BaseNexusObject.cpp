#include "StdAfx.h"
#include "BaseNexusObject.h"
#include "Server.h"

using namespace uc;

CBaseNexusObject::CBaseNexusObject(CServer * s, CString const & name) : Url(s->Url, name), Server(s)
{
}

CBaseNexusObject::~CBaseNexusObject()
{
}

CString CBaseNexusObject::MapRelative(CString const & path)
{
	return Server->MapRelative(CPath::Join(Url.Object, path));
}

CString CBaseNexusObject::MapGlobalPath(CString const & path)
{
	return Server->MapUserGlobalPath(CPath::Join(Url.Object, path));;
}

CString CBaseNexusObject::MapLocalPath(CString const & path)
{
	return Server->MapUserLocalPath(CPath::Join(Url.Object, path));;
}
