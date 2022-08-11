#include "StdAfx.h"
#include "Field.h"

using namespace uc;

CFieldServer::CFieldServer(CShellLevel * l, CString const & name) : CField(l, name), CWorldEntity(l->Server, name)
{
	Level = l;

	SetDirectories(MapRelative(L""));
	SetDefaultInteractiveMaster(AREA_FIELDS);
}

CFieldServer::~CFieldServer()
{
	Save();

	Items.Do([this](auto i){ Level->Server->DestroyObject(i, true); });
}

void CFieldServer::SetTitle(CString const & name)
{
	Title = name;
}

CObject<CFieldItem> CFieldServer::FindByObject(CUol & o)
{
	auto fi = Items.Find([&o](auto i){ return i->Object == o; });
	if(fi)
		return Level->Server->FindObject(fi->Url);
	else
		return {};
}

CList<CFieldItem *> CFieldServer::Add(CList<CUol> & o, CString const & complexity)
{
	CList<CFieldItem *> fis;

	o.Do([this, &complexity, &fis](auto i)
		{
			auto fi = Add(i, complexity);
			fis.push_back(fi);
		});
	return fis;
}

CObject<CFieldItem> CFieldServer::Add(CUol & e, CString const & complexity)
{
	auto fi = new CFieldItem(Level->Server, complexity);
	fi->Object = e;
	Items.push_back(fi);

	///CObject<CStorableObject> o = Level->Nexus->GetObject(e);
	///if(!GlobalDirectory.IsEmpty() || !LocalDirectory.IsEmpty())
	///{
	///	fi->SetDirectories(CFileSystem::Join(CDirectory::GetClassName(), GlobalDirectory, fi->Url.Object), CFileSystem::Join(CDirectory::GetClassName(), LocalDirectory, fi->Url.Object));
	///}

	Level->Server->RegisterObject(fi, false);
	fi->Free();

	Added(fi);

	return fi;
}

void CFieldServer::Remove(CFieldItem * fi)
{
	Items.remove(fi);

	Level->Server->DeleteObject(fi);
	///CObject<CStorableObject> o = Level->Nexus->GetObject(e);
	///if(!GlobalDirectory.IsEmpty() || !LocalDirectory.IsEmpty())
	///{
	///	fi->SetDirectories(CFileSystem::Join(CDirectory::GetClassName(), GlobalDirectory, fi->Url.Object), CFileSystem::Join(CDirectory::GetClassName(), LocalDirectory, fi->Url.Object));
	///}
}

CObject<CFieldItem> CFieldServer::Find(CUol & o)
{
	return Level->Server->FindObject(o);
}

void CFieldServer::SaveInstance()
{
	CTonDocument d;

	d.Add(L"Title")->Set(Title);

	for(auto i : Items)
	{
		auto p = d.Add(L"Item");
		i->Save(p);
	}

	SaveGlobal(d, GetClassName() + L".xon");
}

void CFieldServer::LoadInstance()
{
	Items.Do([](auto i){ delete i; });
	Items.clear();

	CTonDocument d;
	LoadGlobal(d, GetClassName() + L".xon");

	Title = d.Get<CString>(L"Title");

	for(auto i : d.Many(L"Item"))
	{
		auto fi = new CFieldItem(Level->Server, L"", i->AsString());
		fi->Load(i);

		Items.push_back(fi);
		Level->Server->RegisterObject(fi, false);
		fi->Free();
	}
}
