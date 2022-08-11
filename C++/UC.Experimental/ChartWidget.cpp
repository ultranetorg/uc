#include "StdAfx.h"
#include "ChartWidget.h"

using namespace uc;

CChartWidget::CChartWidget(CExperimentalLevel * l, CString const & name) : CWidgetWindow(l->World, l->Server, l->Style, name)
{
	Level = l;

	Element = new CChartElement(Level, Level->Style);
	Element->ApplyStyles(Style, {L"Widget"});

	//Visual->Clipping = EClipping::No;
	//Active->Clipping = EClipping::No;
	//
	//Express(L"Size", [this]{ return Limits.Smax; });

	//Element->Visual->SetMaterial(Level->World->Materials->GetMaterial(L"0 0 0"));
	//AddNode(ChartElement);
	
	SetFace(Element);
}

CChartWidget::~CChartWidget()
{
	if(Menu)
	{
		Menu->Close();
		Menu->Free();
	}

	OnDependencyDestroying(Entity);

	Element->Free();
}

void CChartWidget::SetEntity(CUol & e)
{
	__super::SetEntity(e);

	Entity = __super::Entity->As<CTradeHistory>();
	Entity->Changed += ThisHandler(OnChanged);

	Element->SetEntity(Entity);
}

void CChartWidget::Place(CFieldWorld * fo)
{
	__super::Place(fo);

	//Element->ApplyStyles(Style, {L"Widget"});
}

void CChartWidget::OnChanged()
{
}

void CChartWidget::OnDependencyDestroying(CStorableObject * o)
{
	if(o == Entity)
	{
		Entity->Changed -= ThisHandler(OnChanged);
		Entity = null;
	}
}

void CChartWidget::OnMouse(CActive * r, CActive * s, CMouseArgs * arg)
{
	if(arg->Control == EMouseControl::RightButton && arg->Event == EGraphEvent::Click)
	{
		if(!Menu)
		{
			Menu = new CRectangleMenu(World, Level->Style, L"Menu");
	
			Element->AddMenus(Menu->Section);
			Menu->Section->AddSeparator();

			Field->AddTitleMenu(Menu->Section, this);
			Menu->Section->AddSeparator();
		
			Menu->Section->AddItem(L"Delete")->Clicked = [this](auto, auto){ Field->DeleteAvatar(this); };
			Menu->Section->AddSeparator();
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