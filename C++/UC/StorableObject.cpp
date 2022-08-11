#include "StdAfx.h"
#include "StorableObject.h"
#include "StorableServer.h"
#include "Nexus.h"

using namespace uc;

CStorableObject::CStorableObject(CServer * s, CString const & name) : CInterObject(s, name)
{
}

CStorableObject::~CStorableObject()
{
	delete Info;
}

void CStorableObject::SaveInstance()
{
}

void CStorableObject::LoadInstance()
{
}

void CStorableObject::SetDirectories(CString const & path)
{
	GlobalDirectory = Server->Nexus->MapPath(UOS_MOUNT_USER_GLOBAL, path);
	LocalDirectory	= Server->Nexus->MapPath(UOS_MOUNT_USER_LOCAL, path);
}

void CStorableObject::SaveGlobal(CTonDocument & d, CString const & path)
{
	auto s = Server->As<CStorableServer>()->Storage->OpenWriteStream(CPath::Join(GlobalDirectory, path));
	d.Save(&CXonTextWriter(s));
	Server->As<CStorableServer>()->Storage->Close(s);
}

void CStorableObject::LoadGlobal(CTonDocument & d, CString const & path)
{
	auto s = Server->As<CStorableServer>()->Storage->OpenReadStream(CPath::Join(GlobalDirectory, path));
	d.Load(null, CXonTextReader(s));
	Server->As<CStorableServer>()->Storage->Close(s);
}

void CStorableObject::Load()
{
	if(Server->As<CStorableServer>()->Storage->Exists(GlobalDirectory) || Server->As<CStorableServer>()->Storage->Exists(LocalDirectory))
	{
		LoadInstance();
	}
}

void CStorableObject::Save()
{
	if(!GlobalDirectory.empty() || !LocalDirectory.empty())
	{
		SaveInstance();
	}
}

bool CStorableObject::IsSaved()
{
	return Server->As<CStorableServer>()->Storage->Exists(GlobalDirectory) || Server->As<CStorableServer>()->Storage->Exists(LocalDirectory);
}

void CStorableObject::Delete()
{
	Server->As<CStorableServer>()->Storage->DeleteDirectory(GlobalDirectory);
	Server->As<CStorableServer>()->Storage->DeleteDirectory(LocalDirectory);
}

CString CStorableObject::AddGlobalReference(CUol & r)
{
	if(!Info)
		Info = new CXonDocument();

	//if(Life == ELife::Free)
	{
		auto p = Info->Many(L"Ref").Find([&r](auto i){ return i->Get<CUol>() == r; });

		if(!p)
		{
			auto id = CGuid::Generate64();

			auto p = Info->Add(L"Ref");
			p->Set(r);
			p->Add(L"Token")->Set(id);

			return id;
		}
		else
			return p->One(L"Token")->Get<CString>();
	}

	//throw CException(HERE, L"Non referencable object");
}

void CStorableObject::RemoveGlobalReference(CUol & l, CString const & t)
{	
	auto p = Info->Many(L"Ref").Find([l, t](auto i) mutable { return i->Get<CUol>() == l && i->Get<CString>(L"Token") == t; });

	if(p)
	{
		Info->Remove(p);
	}
}

CXon * CStorableObject::GetInfo(CUol & owner)
{
	return Info ? Info->Many(L"Info").Find([owner](auto i) mutable { return i->Get<CUol>() == owner; }) : null;
}

CXon * CStorableObject::AddInfo(CUol & r)
{
	if(!Info)
		Info = new CXonDocument();

	auto p = Info->Add(L"Info");
	p->Set(r);
	return p;
}

void CStorableObject::SaveInfo(CStream * s)
{
	if(!Info)
		Info = new CXonDocument();

	Info->Save(&CXonTextWriter(s, true));
}

void CStorableObject::LoadInfo(CStream * s)
{
	if(!Info)
		Info = new CXonDocument();

	//if(CPath::IsFile(path))
	//{
	//	Name = CPath::GetFileNameBase(path);
	Info->Load(null, CXonTextReader(s));
	//}
}
