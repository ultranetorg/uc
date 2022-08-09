#include "StdAfx.h"
#include "EmailWidget.h"

using namespace uc;

CEmailWidget::CEmailWidget(CExperimentalLevel * l, CString const & name) : CWidgetWindow(l->World, l->Server, l->Style, name)
{
	Level = l;
	
	Grid = new CGrid(Level->World, Level->Style);
	Grid->ApplyStyles(Style, {L"Widget"});
	Grid->SetSpacing(CFloat2(10, 0));
	Grid->AddColumn(L"From", 100);
	Grid->AddColumn(L"Subject", 200);
	Grid->AddColumn(L"Date", 150, EOrder::Down);

	SetFace(Grid);
}

CEmailWidget::~CEmailWidget()
{
	OnDependencyDestroying(Entity);

	Grid->Free();
}

void CEmailWidget::SetEntity(CUol & e)
{
	__super::SetEntity(e);
	Entity = __super::Entity->As<CEmail>();
	Entity->Destroying += ThisHandler(OnDependencyDestroying);

	for(auto i : Entity->Messages)
	{
		OnMessageRecieved(i);
	}

	
	Entity->MessageRecieved += ThisHandler(OnMessageRecieved);
}

void CEmailWidget::OnDependencyDestroying(CBaseNexusObject * o)
{
	if(o == Entity && Entity)
	{
		Entity->MessageRecieved -= ThisHandler(OnMessageRecieved);
		Entity->Destroying -= ThisHandler(OnDependencyDestroying);
		Entity = null;
	}
}

void CEmailWidget::OnMessageRecieved(CEmailMessage * m)
{
	auto r = Grid->AddRow();
	r->AddCell(&m->From);
	r->AddCell(&m->Subject);
	r->AddCell(&m->Date);

	Grid->UpdateLayout();
	//Grid->Arrange();
}

void CEmailWidget::Place(CFieldWorld * f)
{
	__super::Place(f);

	Grid->ApplyStyles(Style, {L"Widget"});
}

void CEmailWidget::OnMouse(CActive * r, CActive * s, CMouseArgs * arg)
{
	if(arg->Control == EMouseControl::RightButton && arg->Event == EGraphEvent::Click)
	{
		if(!Menu)
		{
			Menu = new CRectangleMenu(World, Level->Style, L"Menu");

			Menu->Section->AddItem(L"Account...")->Clicked =[this](auto a, auto *)
															{
																if(Entity->Account.Url.IsEmpty())
																{
																	auto ea = new CEmailAccount(Level);
																	Level->Server->RegisterObject(ea, true);
																	ea->Free();
	
																	Entity->SetEntity(ea->Url);
																}
															
																Level->World->OpenEntity(Entity->Account.Url, AREA_MAIN, sh_new<CShowParameters>(a, Level->Style));
	
															};
			Menu->Section->AddSeparator();
	
			Menu->Section->AddItem(L"Get New Mail")->Clicked	= [this](auto, auto *){ Entity->Retrieve(); };
	
			Menu->Section->AddSeparator();

			Field->AddTitleMenu(Menu->Section, this);
			Menu->Section->AddSeparator();
		
			Menu->Section->AddItem(L"Delete")->Clicked = [this](auto, auto *){ Field->DeleteAvatar(this); };
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
