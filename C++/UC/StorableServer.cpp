#include "StdAfx.h"
#include "StorableServer.h"
#include "Nexus.h"

using namespace uc;

CStorableServer::CStorableServer(CNexus * l, CServerInfo * info) : CServer(l, info)
{
	Url = info->Url;
	Info = info;
}
	
CStorableServer::~CStorableServer()
{
}

CBaseNexusObject * CStorableServer::FindObject(CString const & name)
{
	auto o = CServer::FindObject(name);

	if(!o && !name.empty() && Exists(name))
	{
		o = CreateObject(name);

		auto so = dynamic_cast<CNexusObject *>(o);
		
		if(so)
		{
			LoadObject(so);
		}

		RegisterObject(o, o->Shared);
		o->Free();
	}

	return o;
}

bool CStorableServer::Exists(CString const & name)
{
	auto f = MapUserGlobalPath(name + L".object");
	auto g = MapUserGlobalPath(name);
	auto l = MapUserLocalPath(name);

	return Storage->Exists(f) || Storage->Exists(g) || Storage->Exists(l);
}

void CStorableServer::LoadObject(CNexusObject * o)
{
	auto f = MapUserGlobalPath(o->Url.Object + L".object");
	auto g = MapUserGlobalPath(o->Url.Object);
	auto l = MapUserLocalPath(o->Url.Object);

	if(Storage->Exists(f))
	{
		o->Shared = true;
				
		auto s = Storage->OpenReadStream(f);
		o->LoadInfo(s);
		Storage->Close(s);
	}

	if(Storage->Exists(g) || Storage->Exists(l))
	{
		o->Load();
	}
}

void CStorableServer::DeleteObject(CBaseNexusObject * r)
{
	auto name = r->Url.Object;
	auto shared = r->Shared;


	if(shared)
	{
		if(CUol::GetObjectID(name).Has([](auto c){ return !iswalnum(c); }))
		{
			throw CException(HERE, L"Incorrect permanent object name");
		}

		Storage->DeleteFile(Nexus->MapPath(UOS_MOUNT_USER_GLOBAL, name + L".object"));
		Storage->DeleteFile(Nexus->MapPath(UOS_MOUNT_USER_LOCAL, name + L".object"));
	}

	DestroyObject(r, false);
	//r->Free();
}

void CStorableServer::DestroyObject(CBaseNexusObject * o, bool save)
{
	if(save && o->Shared)
	{
		auto so = dynamic_cast<CNexusObject *>(o);

		if(so)
		{
			auto s = Storage->OpenWriteStream(MapUserGlobalPath(o->Url.Object + L".object"));
			so->SaveInfo(s);
			Storage->Close(s);
		}
	}

	DestroyObject(o);
}

CTonDocument * CStorableServer::LoadServerDocument(CString const & path)
{
	if(auto s = Storage->OpenReadStream(MapPath(path)))
	{
		auto d = new CTonDocument(CXonTextReader(s));
		Storage->Close(s);
		return d;
	}
	else
		return null;
}

CTonDocument * CStorableServer::LoadGlobalDocument(CString const & path)
{
	if(auto s = Storage->OpenReadStream(MapUserGlobalPath(path)))
	{
		auto d = new CTonDocument(CXonTextReader(s));
		Storage->Close(s);
		return d;
	}
	else
		return null;
}
