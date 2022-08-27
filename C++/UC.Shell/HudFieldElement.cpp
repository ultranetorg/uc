#include "StdAfx.h"
#include "HudFieldElement.h"
#include "History.h"
#include "Board.h"
#include "ApplicationsMenu.h"

using namespace uc;

CHudFieldElement::CHudFieldElement(CShellLevel * l) : CFieldElement(l, GetClassName())
{
	EnableActiveBody = false;
	Level = l;
	
	auto style = l->Style->Document->One(L"Field/Item");

	ItemMargin = CFloat6(4);
	ItemPadding = CFloat6(5);
	IconSize = CSize(24, 24, 24);
	IconTitleMode	= ToAvatarTitleMode(style->Get<CString>(L"IconTitleMode"));
	WidgetTitleMode	= ECardTitleMode::No;
	Transform(0, 0, Z_STEP);
	///Express(L"W", [this]{ return Limits.Smax.W; });
	///Express(L"H", [this]{ return Limits.Smax.H; });

	Active->StateChanged += ThisHandler(OnStateModified);
	Active->MouseEvent[EListen::NormalAll] += ThisHandler(OnMouse);
	Level->World->ActionActivated += ThisHandler(OnWorldInputAction);
		
	Added += ThisHandler(OnAdded);
	Placing += ThisHandler(OnPlacing);

	auto s = new CFieldSurface(l->World, L"Surface");
	AMesh = new CSolidRectangleMesh(l->World->Engine->Level);
	VMesh = new CGridRectangleMesh(l->World->Engine->Level);
	s->Transform(0, 0, Z_STEP);
	s->Visual->SetMesh(VMesh);
	s->Active->SetMesh(AMesh);
	s->Express(L"W", [s]{ return s->Slimits.Smax.W; });
	s->Express(L"H", [s]{ return s->Slimits.Smax.H; });
	s->Visual->SetMaterial(Level->World->Materials->GetMaterial(L"0 0.15 0.15")); // grid
	s->Enable(false);
	SetSurface(s);
	s->Free();
		

/*
	Info = new CParagraph(l->World);
	//Info->SetView(l->World->MainOrthoView);
	
	Info->Add(l->Core->Product.ToString(L"NVSPB"));
	Info->AddBreak();
	Info->Add(L"Unstable build. For research and educational purposes only.");

	ProductInfo = Info->AddBreak();

	AddNode(Info);*/
}
	
CHudFieldElement::~CHudFieldElement()
{
	if(Menu)
	{
		Menu->Close();
		Menu->Free();
	}
	
	///Save();

	SetSurface(null);
	//RemoveNode(Info);

	AMesh->Free();
	VMesh->Free();

	//Info->Free();

	Level->World->ActionActivated -= ThisHandler(OnWorldInputAction);
}

void CHudFieldElement::SetEntity(CFieldServer * f)
{	
	if(Entity)
	{
		SetField(null);
	}

	Entity = f;

	if(Entity)
	{
		SetField(Entity);
		Load();
	}
}

CFieldItemElement * CHudFieldElement::AddItem(CFieldItem * fi)
{
	if(fi->Object.Object == SHELL_BOARD_1 && Level->World->Free3D)
		return null;
	else
		return __super::AddItem(fi);
}

void CHudFieldElement::OnAdded(CFieldItemElement * fie)
{
	auto hudmax = Level->World->Viewports.Has([](auto i){ return i->Tags.Contains(LAYOUT_HUD_MAXIMIZE); });

	if(fie->Entity->Object.Object == SHELL_HISTORY_1)
	{
		fie->Avatar->Express(L"W", [this, hudmax]{ return IW * 0.15f; });
		fie->Avatar->Express(L"H", [this, hudmax]{ return hudmax ? IH - (ItemMargin.TP + ItemMargin.BM + ItemPadding.TP + ItemPadding.BM) : 75.f; });
	}
					
	if(fie->Entity->Object.Object == SHELL_BOARD_1)
	{
		fie->Avatar->Express(L"W",	[this, hudmax]{ return IW * 0.7f; });
		fie->Avatar->Express(L"H",	[this, hudmax]{ return hudmax ? IH - (ItemMargin.TP + ItemMargin.BM + ItemPadding.TP + ItemPadding.BM) : 75.f; });
	}

	if(fie->Entity->Object.Object == SHELL_TRAY_1)
	{
		fie->Avatar->Express(L"W", [this, hudmax]{ return IW * 0.15f; });
		fie->Avatar->Express(L"H", [this, hudmax]{ return hudmax ? IH - (ItemMargin.TP + ItemMargin.BM + ItemPadding.TP + ItemPadding.BM) : 75.f; });
	}
}

void CHudFieldElement::OnPlacing(CFieldItemElement * fie)
{
	if(Level->World->Initializing)
	{
		if(fie->Entity->Object == CUol(CWorldEntity::Scheme, Level->Server->Instance, SHELL_HISTORY_1))
		{
			fie->Transform(0, 0, ItemZ);
		}
					
		if(fie->Entity->Object == CUol(CWorldEntity::Scheme, Level->Server->Instance, SHELL_BOARD_1))
		{
			fie->Transform(IW * 0.15f, 0, ItemZ);
		}

		if(fie->Entity->Object == CUol(CWorldEntity::Scheme, Level->Server->Instance, SHELL_TRAY_1))
		{
			fie->Transform(IW * 0.85f, 0, ItemZ);
		}

		if(fie->Entity->Object == CUol(CWorldEntity::Scheme, Level->Server->Instance, SHELL_APPLICATIONSMENU_1))
		{
			fie->Transform(0, 200, ItemZ);
		}
	}
}

void CHudFieldElement::UpdateLayout(CLimits const & l, bool apply)
{
	__super::UpdateLayout(l, apply);

/*
	if(apply)
	{
		AMesh->Generate(0, 0, Surface->W, Surface->H);
		VMesh->Generate(0, 0, 0, Surface->W, Surface->H, int(Surface->W/IconSize.W), int(Surface->H/IconSize.H), 1.f);
	
		Info->UpdateLayout({Size, Size}, apply);
		Info->Transform(0, Size.H - Info->H, Z_STEP * 2);
	}*/
}

void CHudFieldElement::OnProductInfoRetrieved(CUpdateData * d)
{
/*
	auto click  =	[this](auto, auto, auto a)
					{
						if(a.Class == EInputClass::Mouse && a.Sender == EInputSender::LeftButton && a.Type == EActiveEventType::Click)
						{
							Level->Nexus->Execute(CUrq(Level->WebInformer->Data.DownloadPageUrl), sh_new<CShowParameters>(a, Level->Style));
						}
					};

	if(d->Version > Level->Core->Product.Version)
	{
		auto r = new CRectangle(Level->World);
		r->EnableActiveBody = true;
		r->Active->ToggleInput += click;
	
		auto t = new CText(Level->World, Level->Style, L"New version is available! Click to download.");
		t->SetColor(Level->Style->Get<CFloat4>(L"Selection/Border/Material"));

		r->AddNode(t);

		Info->Insert(r, ProductInfo);

		Info->InsertBreak(r);

		r->Free();
		t->Free();
	}
	else if(d->Version < Level->Core->Product.Version)
	{
		auto t = Info->Insert(ProductInfo, CString::Format(L"Server version: %s   Ours: %s", d->Version.ToString(), Level->Core->Product.Version.ToString()), true);
		t->Active->ToggleInput += click;
		t->SetColor(Level->Style->Get<CFloat4>(L"Selection/Border/Material"));
		Info->InsertBreak(t);
	}

	UpdateLayout();*/
}

void CHudFieldElement::OnStateModified(CActive * r, CActive * s, CActiveStateArgs * arg)
{
	if(s == Active)
	{
		if(arg->Old == EActiveState::Active)
		{
			//Visual->SetMaterial(null);
			//Surface->Enable(false);
		}
	}
}

void CHudFieldElement::OnWorldInputAction(EWorldAction wa)
{
	///if(wa == EWorldAction::TopSlide)
	///{
	///	if(Active->Graph->Focus != Active)
	///	{
	///		if(Active->State == EActiveState::Normal)
	///		{
	///			Surface->Enable(true);
	///			Visual->SetMaterial(Level->World->Materials->GetMaterial(L"0 0 0 0.9"));
	///			Active->Graph->Activate(Active);
	///		}
	///		else
	///		{
	///			Surface->Enable(false);
	///			Visual->SetMaterial(null);
	///		}
	///	}
	///}
}

void CHudFieldElement::OnMouse(CActive * r, CActive * s, CMouseArgs * arg)
{
	if(arg->Control == EMouseControl::RightButton && arg->Event == EGraphEvent::Click)
	{
		if(s == Surface->Active)
		{
			auto p = Active->Transit(arg->Pick.Active, arg->Pick.Point);

			if(Menu == null)
			{
				Menu = new CRectangleMenu(Level->World, Level->Style);

				AddNewMenu(Menu, p);

				Menu->Section->AddSeparator();

				Menu->Section->AddItem(L"Hide Grid")->Clicked =	[this](auto, auto)
																{ 
																	Visual->SetMaterial(null);
																	Surface->Enable(false);																
																};
			}

			
			Menu->Open(arg->Pick);


			arg->StopPropagation = true;
		}
	}
}

