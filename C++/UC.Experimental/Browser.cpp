#include "stdafx.h"
#include "Browser.h"

using namespace uc;

CBrowser::CBrowser(CExperimentalLevel * l, CString const & name) : CWorldEntity(l->Server, name)
{
	Level = l;

	SetDirectories(MapRelative(L""));
	SetDefaultInteractiveMaster(AREA_MAIN);
	SetTitle(L"Browser");
}

CBrowser::~CBrowser()
{
	Save();
}

void CBrowser::SaveInstance()
{
	__super::SaveInstance();

	CTonDocument d;

	d.Add(L"Address")->Set(Address);
	d.Add(L"Title")->Set(Title);

	SaveGlobal(d, GetClassName() + L".xon");
}

void CBrowser::LoadInstance()
{
	CTonDocument d;
	LoadGlobal(d, GetClassName() + L".xon");

	Address = d.Get<CUrl>(L"Address");
	Title = d.Get<CString>(L"Title");
}

void CBrowser::SetAddress(CUrl & o)
{
	Address = o;
	AddressChanged();
}
