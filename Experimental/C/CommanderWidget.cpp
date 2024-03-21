#include "StdAfx.h"
#include "CommanderWidget.h"

using namespace uc;

CCommanderWidget::CCommanderWidget(CExperimentalLevel * l, CString const & name) : CWidgetWindow(l->World, l->Server, l->Style, name)
{
	Level = l;

	FileList = new CStandardFileList(Level);
	FileList->ApplyStyles(Style, {L"Widget"});
	SetFace(FileList);
}

CCommanderWidget::~CCommanderWidget()
{
	if(Menu)
	{
		Menu->Close();
		Menu->Free();
	}
	
	FileList->Free();
}

void CCommanderWidget::SetEntity(CUol & e)
{
	__super::SetEntity(e);
	Entity = __super::Entity->As<CCommander>();
	
	FileList->SetSource(Entity->Root);

	OnFileListPathChanged(Entity->Path);
	FileList->PathChanged += ThisHandler(OnFileListPathChanged);
}

void CCommanderWidget::OnFileListPathChanged(CString const  & o)
{
	Entity->SetPath(o);
}