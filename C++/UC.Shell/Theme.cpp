#include "StdAfx.h"
#include "Theme.h"

using namespace uc;

CTheme::CTheme(CShellLevel * l, CString const & name) : CWorldEntity(l->Server, name)
{
	Level = l;

	SetDirectories(MapRelative(L""));
	SetDefaultInteractiveMaster(AREA_THEME);
}

CTheme::~CTheme()
{
	if(Directory)
	{
		if(CPath::GetExtension(Source).ToLower() == L"vwm")
			delete Directory;
		else
			Level->Storage->Close(dynamic_cast<CDirectory *>(Directory));
	}


	Save();

	delete Importer;
}

void CTheme::SaveInstance()
{
	CTonDocument d;
	d.Add(L"Source")->Set(Source);

	SaveGlobal(d, GetClassName() + L".xon");
}

void CTheme::LoadInstance()
{
	CTonDocument d;
	LoadGlobal(d, GetClassName() + L".xon");

	SetSource(d.Get<CString>(L"Source"));
}

void CTheme::SetSource(CString & o)
{
	SetTitle(GetClassName() + L": " + CPath::GetNameBase(o));

	Source = o;

	if(Level->Storage->Exists(Source))
	{
		if(CPath::GetExtension(o).ToLower() == L"vwm")
		{
			auto s = Level->Storage->OpenReadStream(o);
			Directory = new CZipDirectory(s, EFileMode::Open);
			Level->Storage->Close(s);
		}
		else
		{
			Directory = Level->Storage->OpenDirectory(o);
		}

		if(Directory)
		{
			Importer = new CVwmImporter(Level->World);
			Importer->Import(Directory);
		}
	}
}

