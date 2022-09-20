#include "StdAfx.h"
#include "TradingviewWidget.h"

using namespace uc;

CTradingviewWidget::CTradingviewWidget(CExperimentalLevel * l, CString const & name) : CWidgetWindow(l->World, l->Server, l->Style, name)
{
	Level = l;

	Element = new CTradingviewElement(Level, Level->Style);
	Element->ApplyStyles(Style, {L"Widget"});
	SetFace(Element);
}

CTradingviewWidget::~CTradingviewWidget()
{
	Element->Free();
}

void CTradingviewWidget::SetEntity(CUol & e)
{
	__super::SetEntity(e);
	Entity = __super::Entity->As<CTradingview>();
	Entity->Changed += ThisHandler(OnChanged);
	
	Element->SetEntity(Entity);
}

void CTradingviewWidget::Open(CWorldCapabilities * caps, CUnit * a)
{
	__super::Open(caps, a);

	if(!Confugured)
	{
		Element->Express(L"P", [this]{ return CFloat6(5.f); });
		Element->Express(L"W", [this]{ return IW; });
		Element->Express(L"H", [this]{ return IH; });
		Element->Visual->SetMaterial(Level->World->Materials->GetMaterial(L"0 0 0"));

		Confugured = true;
	}
}

void CTradingviewWidget::Place(CFieldWorld * fo)
{
	__super::Place(fo);

	Element->ApplyStyles(Style, {L"Widget"});
}

void CTradingviewWidget::OnChanged()
{
}

void CTradingviewWidget::OnDependencyDestroying(CPersistentObject * o)
{
	if(o == Entity)
	{
		Entity->Changed -= ThisHandler(OnChanged);
		Entity = null;
	}
}

void CTradingviewWidget::OnMouse(CActive * r, CActive * s, CMouseArgs * arg)
{
	if(arg->Control == EMouseControl::RightButton && arg->Event == EGraphEvent::Click)
	{
		if(!Menu)
		{
			Menu = new CRectangleMenu(World, Level->Style, L"Menu");
	
			Menu->Section->AddItem(L"Refresh")->Clicked = [this](auto, auto){ Element->Refresh(); };
			Menu->Section->AddSeparator();

			Element->AddMenus(Menu->Section);
			Menu->Section->AddSeparator();

			if(Field)
			{
				Field->AddTitleMenu(Menu->Section, this);
				Menu->Section->AddSeparator();
			
				Menu->Section->AddItem(L"Delete")->Clicked = [this](auto, auto){ Field->DeleteAvatar(this); };
				Menu->Section->AddSeparator();
			}

			Menu->Section->AddItem(L"Properties...");
		}
		
		Menu->Open(arg->Pick);
	
		arg->StopPropagation = true;
	}

	if(arg->Control == EMouseControl::LeftButton && arg->Event == EGraphEvent::Click)
	{
		arg->StopPropagation = true;
	}
}