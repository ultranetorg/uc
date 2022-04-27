#include "StdAfx.h"
#include "CommanderEnvironment.h"

using namespace uc;

CCommanderEnvironment::CCommanderEnvironment(CExperimentalLevel * l, const CString & name) : CEnvironmentWindow(l->World, l->Server, l->Style, name)
{
	Level = l;
		
	FileList = new CColumnFileList(Level);
	FileList->Active->Index = 0;
	FileList->ApplyStyles(Style, {L"Environment"});
	FileList->Express(L"P", [this]{ return CFloat6(5); });
	//FileList->Express(L"M", [this]{ return CFloat6(5); });
	FileList->Express(L"W", [this]{ return Size.W; });
	FileList->Express(L"H", [this]{ return Size.H; });

	SetContent(FileList);
	UseSizer(this, FileList);
}

CCommanderEnvironment::~CCommanderEnvironment()
{
	FileList->Clear();
	FileList->Free();
}

void CCommanderEnvironment::SetEntity(CUol & e)
{
	__super::SetEntity(e);
	Entity = __super::Entity->As<CCommander>();

	FileList->SetRoot(Entity->Root);
	FileList->SetPath(Entity->Path);

	OnFileListPathChanged(Entity->Path);
	FileList->PathChanged += ThisHandler(OnFileListPathChanged);
}

void CCommanderEnvironment::DetermineSize(CSize & smax, CSize & s)
{
	//if(!Size)
	{	
		if(s)
		{
			Express(L"W", [s]{ return s.W; });
			Express(L"H", [s]{ return s.H; });
		}
		else
		{
			Express(L"W", [smax]{ return smax.H * 0.8f * 3.f/4.f; });
			Express(L"H", [smax]{ return smax.H * 0.8f; });
		}
	}
	UpdateLayout({smax, smax}, false);
}

void CCommanderEnvironment::OnFileListPathChanged(CString const  & o)
{
	Entity->SetPath(o);
}

void CCommanderEnvironment::SaveInstance()
{
	__super::SaveInstance();

	CTonDocument d;

	FileList->Save(&d);

	SaveGlobal(d, GetClassName() + L".xon");
}

void CCommanderEnvironment::LoadInstance()
{
	__super::LoadInstance();

	CTonDocument d;
	LoadGlobal(d, GetClassName() + L".xon");

	FileList->Load(&d);
}

