#include "StdAfx.h"
#include "NexusObject.h"
#include "StorableServer.h"
#include "Nexus.h"

using namespace uc;

CNexusObject::CNexusObject(CServer * s, CString const & name) : CBaseNexusObject(s, name)
{
}

CNexusObject::~CNexusObject()
{
	delete InfoDoc;
}

void CNexusObject::SaveInstance()
{
}

void CNexusObject::LoadInstance()
{
}

void CNexusObject::SetDirectories(CString const & path)
{
	GlobalDirectory = Server->Nexus->MapPath(UOS_MOUNT_USER_GLOBAL, path);
	LocalDirectory	= Server->Nexus->MapPath(UOS_MOUNT_USER_LOCAL, path);
}

void CNexusObject::SaveGlobal(CTonDocument & d, CString const & path)
{
	auto s = Server->As<CStorableServer>()->Storage->OpenWriteStream(CPath::Join(GlobalDirectory, path));
	d.Save(&CXonTextWriter(s));
	Server->As<CStorableServer>()->Storage->Close(s);
}

void CNexusObject::LoadGlobal(CTonDocument & d, CString const & path)
{
	auto s = Server->As<CStorableServer>()->Storage->OpenReadStream(CPath::Join(GlobalDirectory, path));
	d.Load(null, CXonTextReader(s));
	Server->As<CStorableServer>()->Storage->Close(s);
}

void CNexusObject::Load()
{
	if(Server->As<CStorableServer>()->Storage->Exists(GlobalDirectory) || Server->As<CStorableServer>()->Storage->Exists(LocalDirectory))
	{
		LoadInstance();
	}
}

void CNexusObject::Save()
{
	if(!GlobalDirectory.empty() || !LocalDirectory.empty())
	{
		SaveInstance();
	}
}

bool CNexusObject::IsSaved()
{
	return Server->As<CStorableServer>()->Storage->Exists(GlobalDirectory) || Server->As<CStorableServer>()->Storage->Exists(LocalDirectory);
}

void CNexusObject::Delete()
{
	Server->As<CStorableServer>()->Storage->DeleteDirectory(GlobalDirectory);
	Server->As<CStorableServer>()->Storage->DeleteDirectory(LocalDirectory);
}

CString CNexusObject::AddGlobalReference(CUol & r)
{
	if(!InfoDoc)
		InfoDoc = new CXonDocument();

	//if(Life == ELife::Free)
	{
		auto p = InfoDoc->Many(L"Ref").Find([&r](auto i){ return i->Get<CUol>() == r; });

		if(!p)
		{
			auto id = CGuid::Generate64();

			auto p = InfoDoc->Add(L"Ref");
			p->Set(r);
			p->Add(L"Token")->Set(id);

			return id;
		}
		else
			return p->One(L"Token")->Get<CString>();
	}

	//throw CException(HERE, L"Non referencable object");
}

void CNexusObject::RemoveGlobalReference(CUol & l, CString const & t)
{	
	auto p = InfoDoc->Many(L"Ref").Find([l, t](auto i) mutable { return i->Get<CUol>() == l && i->Get<CString>(L"Token") == t; });

	if(p)
	{
		InfoDoc->Remove(p);
	}
}

CXon * CNexusObject::GetInfo(CUol & owner)
{
	return InfoDoc ? InfoDoc->Many(L"Info").Find([owner](auto i) mutable { return i->Get<CUol>() == owner; }) : null;
}

CXon * CNexusObject::AddInfo(CUol & r)
{
	if(!InfoDoc)
		InfoDoc = new CXonDocument();

	auto p = InfoDoc->Add(L"Info");
	p->Set(r);
	return p;
}

void CNexusObject::SaveInfo(CStream * s)
{
	if(!InfoDoc)
		InfoDoc = new CXonDocument();

	InfoDoc->Save(&CXonTextWriter(s, true));
}

void CNexusObject::LoadInfo(CStream * s)
{
	if(!InfoDoc)
		InfoDoc = new CXonDocument();

	//if(CPath::IsFile(path))
	//{
	//	Name = CPath::GetFileNameBase(path);
	InfoDoc->Load(null, CXonTextReader(s));
	//}
}
