#include "StdAfx.h"
#include "InterObject.h"
#include "Server.h"

using namespace uc;

CInterObject::CInterObject(CString const & scheme, CServer * s, CString const & name) : Url(scheme, s->Instance, name), Server(s)
{
}

CInterObject::~CInterObject()
{
}

CString CInterObject::MapRelative(CString const & path)
{
	return CPath::Join(Server->Instance, Url.Object, path);
}

CString CInterObject::MapGlobalPath(CString const & path)
{
	return Server->MapUserGlobalPath(CPath::Join(Url.Object, path));;
}

CString CInterObject::MapLocalPath(CString const & path)
{
	return Server->MapUserLocalPath(CPath::Join(Url.Object, path));;
}
