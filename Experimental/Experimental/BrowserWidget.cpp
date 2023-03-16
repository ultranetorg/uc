#include "StdAfx.h"
#include "BrowserWidget.h"

using namespace uc;

CBrowserWidget::CBrowserWidget(CExperimentalLevel * l, CString const & name) : CWidgetWindow(l->World, l->Server, l->Style, name)
{
	Level = l;

	CefElement = new CCefElement(Level,  Level->Style, L"cef");
	CefElement->ApplyStyles(Style, {L"Widget"});
	CefElement->Cef->TitleChanged	+= ThisHandler(OnCefTitleChanged);
	CefElement->Cef->AddressChanged	+= ThisHandler(OnCefAddressChanged);


	SetFace(CefElement);
}

CBrowserWidget::~CBrowserWidget()
{
	CefElement->Free();
}

void CBrowserWidget::SetEntity(CUol & e)
{
	__super::SetEntity(e);
	Entity = __super::Entity->As<CBrowser>();
	
	CefElement->Navigate(Entity->Address.ToString());
}

void CBrowserWidget::OnCefTitleChanged(CString const & t)
{
	Entity->SetTitle(t);
}

void CBrowserWidget::OnCefAddressChanged(CString & url)
{
	Entity->SetAddress(CUrl(url));
}

void CBrowserWidget::Place(CFieldWorld * f)
{
	__super::Place(f);
}

void CBrowserWidget::Open(CWorldCapabilities * caps, CUnit * a)
{
	__super::Open(caps, a);

	if(caps->Tight)
	{
		CefElement->Reset(L"P");
		UpdateLayout();
	}
}