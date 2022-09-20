#include "stdafx.h"
#include "Commander.h"

using namespace uc;

CCommander::CCommander(CExperimentalLevel * l, CString const & name) : CWorldEntity(l->Server, name)
{
	Level = l;

	SetDirectories(MapRelative(L""));
	SetDefaultInteractiveMaster(CArea::Main);
}

CCommander::~CCommander()
{
	Save();
}

void CCommander::SaveInstance()
{
	CTonDocument d;

	d.Add(L"Root")->Set(Root);
	d.Add(L"Path")->Set(Path);

	SaveGlobal(d, GetClassName() + L".xon");
}

void CCommander::LoadInstance()
{
	CTonDocument d; 
	
	LoadGlobal(d, GetClassName() + L".xon");

	SetRoot(d.Get<CString>(L"Root"));
	SetPath(d.Get<CString>(L"Path"));
}

void CCommander::SetRoot(CString const & u)
{
	Root = u;
	SetPath(u);
}

void CCommander::SetPath(CString const & o)
{
	Path = o;

	SetTitle(GetClassName() + L" - " + Path);

	PathChanged();
}
