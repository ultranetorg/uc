#include "StdAfx.h"
#include "NotepadWidget.h"

using namespace uc;

CNotepadWidget::CNotepadWidget(CShellLevel * l, CString const & name) : CWidgetWindow(l->World, l->Server, l->Style, name)
{
	Level = l;

	auto f = new CTextEdit(Level->World, Level->Style);
	f->ApplyStyles(Style, {L"Widget"});
	f->SetMultiline(true);
	SetFace(f);
	f->Free();
}

CNotepadWidget::~CNotepadWidget()
{
	if(Menu)
	{
		Menu->Close();
		Menu->Free();
	}
}

void CNotepadWidget::SetEntity(CUol & o)
{
	__super::SetEntity(o);
	Entity = __super::Entity->As<CNotepad>();

	if(auto s = Level->Storage->OpenReadStream(Entity->File))
	{
		CTextReader r(s, ETextEncoding::UTF8);
	
		Face->As<CTextEdit>()->SetText(r.Text);
	
		Level->Storage->Close(s);
	}
}

void CNotepadWidget::SaveInstance()
{
	__super::SaveInstance();
}

void CNotepadWidget::LoadInstance()
{
	__super::LoadInstance();
}
