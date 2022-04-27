#include "StdAfx.h"
#include "EarthWidget.h"

using namespace uc;

CEarthWidget::CEarthWidget(CExperimentalLevel * l, CGeoStore * s, CString const & name) : CWidgetWindow(l->World, l->Server, l->Style, name)
{
	Level = l;

	Globe = new CGlobe(Level, s);
	Globe->ApplyStyles(Style, {L"Widget"});
	//Globe->Express(L"P", [this]{ return CFloat6(30); }); // hack: to fit shere properly
		
	SetFace(Globe);
}

CEarthWidget::~CEarthWidget()
{
	if(Menu)
	{
		Menu->Close();
		Menu->Free();
	}

	Globe->Free();
}

void CEarthWidget::SetEntity(CUol & e)
{
	__super::SetEntity(e);
	Entity = __super::Entity->As<CEarth>();
}

void CEarthWidget::Open(CWorldCapabilities * caps, CUnit * a)
{
	__super::Open(caps, a);

	Globe->Unit = Unit;
}
