#include "StdAfx.h"
#include "PersistentObject.h"
#include "PersistentServer.h"
#include "Nexus.h"

using namespace uc;

CPersistentObject::CPersistentObject(CString const & scheme, CServer * server, CString const & name) : CInterObject(scheme, server, name)
{
}

CPersistentObject::~CPersistentObject()
{
	delete Info;
}

void CPersistentObject::SaveInstance()
{
}

void CPersistentObject::LoadInstance()
{
}

void CPersistentObject::SetDirectories(CString const & path)
{
	GlobalDirectory = CPath::Join(CFileSystemProtocol::UserGlobal, path);
	LocalDirectory	= CPath::Join(CFileSystemProtocol::UserLocal, path);
}

void CPersistentObject::SaveGlobal(CTonDocument & d, CString const & path)
{
	auto s = Server->As<CPersistentServer>()->Storage->WriteFile(CPath::Join(GlobalDirectory, path));
	d.Save(&CXonTextWriter(s));
	Server->As<CPersistentServer>()->Storage->Close(s);
}

void CPersistentObject::LoadGlobal(CTonDocument & d, CString const & path)
{
	auto s = Server->As<CPersistentServer>()->Storage->ReadFile(CPath::Join(GlobalDirectory, path));
	d.Load(null, CXonTextReader(s));
	Server->As<CPersistentServer>()->Storage->Close(s);
}

void CPersistentObject::Load()
{
	if(Server->As<CPersistentServer>()->Storage->Exists(GlobalDirectory) || Server->As<CPersistentServer>()->Storage->Exists(LocalDirectory))
	{
		LoadInstance();
	}
}

void CPersistentObject::Save()
{
	if(!GlobalDirectory.empty() || !LocalDirectory.empty())
	{
		SaveInstance();
	}
}

bool CPersistentObject::IsSaved()
{
	return Server->As<CPersistentServer>()->Storage->Exists(GlobalDirectory) || Server->As<CPersistentServer>()->Storage->Exists(LocalDirectory);
}

void CPersistentObject::Delete()
{
	Server->As<CPersistentServer>()->Storage->Delete(GlobalDirectory);
	Server->As<CPersistentServer>()->Storage->Delete(LocalDirectory);
}

CString CPersistentObject::AddGlobalReference(CUol & r)
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

void CPersistentObject::RemoveGlobalReference(CUol & l, CString const & t)
{	
	auto p = Info->Many(L"Ref").Find([l, t](auto i) mutable { return i->Get<CUol>() == l && i->Get<CString>(L"Token") == t; });

	if(p)
	{
		Info->Remove(p);
	}
}

CXon * CPersistentObject::GetInfo(CUol & owner)
{
	return Info ? Info->Many(L"Info").Find([owner](auto i) mutable { return i->Get<CUol>() == owner; }) : null;
}

CXon * CPersistentObject::AddInfo(CUol & r)
{
	if(!Info)
		Info = new CXonDocument();

	auto p = Info->Add(L"Info");
	p->Set(r);
	return p;
}

void CPersistentObject::SaveInfo(CStream * s)
{
	if(!Info)
		Info = new CXonDocument();

	Info->Save(&CXonTextWriter(s, true));
}

void CPersistentObject::LoadInfo(CStream * s)
{
	if(!Info)
		Info = new CXonDocument();

	//if(CPath::IsFile(path))
	//{
	//	Name = CPath::GetFileNameBase(path);
	Info->Load(null, CXonTextReader(s));
	//}
}
