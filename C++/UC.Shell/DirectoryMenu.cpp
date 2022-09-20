#include "stdafx.h"
#include "DirectoryMenu.h"

using namespace uc;

CDirectoryMenu::CDirectoryMenu(CShellLevel * l, CString const & name) : CWorldEntity(l->Server, name)
{
	Level = l;

	SetDirectories(MapRelative(L""));
	SetDefaultInteractiveMaster(CArea::Main);
}

CDirectoryMenu::~CDirectoryMenu()
{
	Save();
}

void CDirectoryMenu::AddPath(CString & o)
{
	Paths.push_back(o);
}

void CDirectoryMenu::SaveInstance()
{
	CTonDocument d;

	for(auto i : Paths)
	{
		d.Add(L"Item")->Set(i);
	}

	SaveGlobal(d, GetClassName() + L".xon");
}

void CDirectoryMenu::LoadInstance()
{
	CTonDocument d;
	LoadGlobal(d, GetClassName() + L".xon");

	for(auto i : d.Many(L"Item"))
	{
		Paths.push_back(i->Get<CString>());
	}
}
