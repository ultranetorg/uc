#include "StdAfx.h"
#include "PersistentServer.h"
#include "Nexus.h"

using namespace uc;

CPersistentServer::CPersistentServer(CNexus * l, CServerInstance * info) : CServer(l, info)
{
}
	
CPersistentServer::~CPersistentServer()
{
}

CInterObject * CPersistentServer::FindObject(CString const & name)
{
	auto o = CServer::FindObject(name);

	if(!o && !name.empty() && Exists(name))
	{
		o = CreateObject(name);

		auto so = dynamic_cast<CPersistentObject *>(o);
		
		if(so)
		{
			LoadObject(so);
		}

		RegisterObject(o, o->Shared);
		o->Free();
	}

	return o;
}

bool CPersistentServer::Exists(CString const & name)
{
	auto f = MapUserGlobalPath(name + L".object");
	auto g = MapUserGlobalPath(name);
	auto l = MapUserLocalPath(name);

	return Storage->Exists(f) || Storage->Exists(g) || Storage->Exists(l);
}

void CPersistentServer::LoadObject(CPersistentObject * o)
{
	auto f = MapUserGlobalPath(o->Url.Object + L".object");
	auto g = MapUserGlobalPath(o->Url.Object);
	auto l = MapUserLocalPath(o->Url.Object);

	if(Storage->Exists(f))
	{
		o->Shared = true;
				
		auto s = Storage->ReadFile(f);
		o->LoadInfo(s);
		Storage->Close(s);
	}

	if(Storage->Exists(g) || Storage->Exists(l))
	{
		o->Load();
	}
}

void CPersistentServer::DeleteObject(CInterObject * r)
{
	auto name = r->Url.Object;
	auto shared = r->Shared;


	if(shared)
	{
		if(CUol::GetObjectId(name).Has([](auto c){ return !iswalnum(c); }))
		{
			throw CException(HERE, L"Incorrect permanent object name");
		}

		Storage->Delete(CPath::Join(CFileSystem::UserGlobal, name + L".object"));
		Storage->Delete(CPath::Join(CFileSystem::UserLocal, name + L".object"));
	}

	DestroyObject(r, false);
	//r->Free();
}

void CPersistentServer::DestroyObject(CInterObject * o, bool save)
{
	if(save && o->Shared)
	{
		auto so = dynamic_cast<CPersistentObject *>(o);

		if(so)
		{
			auto s = Storage->WriteFile(MapUserGlobalPath(o->Url.Object + L".object"));
			so->SaveInfo(s);
			Storage->Close(s);
		}
	}

	DestroyObject(o);
}

CTonDocument * CPersistentServer::LoadReleaseDocument(CString const & path)
{
	if(auto s = Storage->ReadFile(MapReleasePath(path)))
	{
		auto d = new CTonDocument(CXonTextReader(s));
		Storage->Close(s);
		return d;
	}
	else
		return null;
}

CTonDocument * CPersistentServer::LoadGlobalDocument(CString const & path)
{
	try
	{
		auto s = Storage->ReadFile(MapUserGlobalPath(path));
		auto d = new CTonDocument(CXonTextReader(s));
		Storage->Close(s);
		return d;
	}
	catch(CFileException &)
	{
		return null;
	}
}
