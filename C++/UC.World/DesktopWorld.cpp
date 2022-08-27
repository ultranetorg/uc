#include "stdafx.h"
#include "DesktopWorld.h"

using namespace uc;

CDesktopWorld::CDesktopWorld(CNexus * l, CServerRelease * si) : CWorldServer(l, si)
{
	CWorldCapabilities::Name = WORLD_DESKTOP;
	Complexity = AVATAR_ENVIRONMENT;
	Free3D = false;
	FullScreen = false;
	Tight = false;
}

CDesktopWorld::~CDesktopWorld()
{
	if(InteractivePositioning)
		InteractivePositioning->Free();

	if(BackPositioning)
		BackPositioning->Free();
}

void CDesktopWorld::InitializeViewports()
{
	for(auto i : Targets)
	{
		Viewports.push_back(new CScreenViewport(Engine->Level, i,	i->Size.W / Engine->ScreenEngine->Scaling.x, i->Size.H / Engine->ScreenEngine->Scaling.y, 
																	0, 0,
																	i->Size.W, i->Size.H,
																	i->Screen->Rect.W, i->Screen->Rect.H));
	}

	MainViewport = Viewports.front();
}

void CDesktopWorld::InitializeGraphs()
{
	ActiveGraph->Root->MouseEvent[EListen::NormalAll] += ThisHandler(OnMouse);
	ActiveGraph->Root->TouchEvent[EListen::NormalAll] += ThisHandler(OnTouch);
	ActiveGraph->Root->KeyboardEvent[EListen::NormalAll] += ThisHandler(OnKeyboard);
	ActiveGraph->Root->StateChanged += ThisHandler(OnStateChanged);
}

void CDesktopWorld::InitializeView()
{
	MainView = new CWorldView(this, L"Main");
	HudView = new CWorldView(this, L"Hud");
	ThemeView = new CWorldView(this, L"Theme");

	//Z = MainView->GetCamera(MainViewport)->UnprojectZ(1, 1);
	Z = 1000;

	for(auto i : Viewports)
	{
		auto a = i->W/i->H;
		auto fov0 = 2.f * a * atan((i->W*0.5f)/(a * Z));

		HudView->AddCamera(i, fov0, 100, 1e4)->UseAffine();
		MainView->AddCamera(i, fov0, 100, 10000)->UseAffine();
		ThemeView->AddCamera(i, fov0, 10, 1e6)->UseAffine();
	}
}

void CDesktopWorld::InitializeAreas()
{
	InteractivePositioning = new CPolygonalPositioning(false);
	BackPositioning = new CPolygonalPositioning(false);

	for(auto i : Viewports)
	{
		auto w = i->W;
		auto h = i->H;

		auto bw = MainView->GetCamera(i)->UnprojectX(Z + 7000, w/2);
		auto bh = MainView->GetCamera(i)->UnprojectY(Z + 7000, h/2);

		InteractivePositioning->GetView				= [this]		{ return MainView; };
		InteractivePositioning->Transformation[i]	= [this](auto)	{ return CTransformation(0, 0, Z); };
		InteractivePositioning->Bounds[i]			= []			{ return CArray<CFloat2>{{-FLT_MAX, -FLT_MAX}, {-FLT_MAX, FLT_MAX}, {FLT_MAX, FLT_MAX}, {FLT_MAX, -FLT_MAX}}; };

		BackPositioning->GetView			= [this]				{ return MainView; };
		BackPositioning->Transformation[i]	= [this, bw, bh](auto)	{ return CTransformation(0, 0, Z + 7000); };
		BackPositioning->Bounds[i]			= [bw, bh]				{ return CArray<CFloat2>{{-bw, -bh}, {-bw, bh}, {bw, bh}, {bw, -bh}}; };

		if(i->Tags.Contains(LAYOUT_HUD_PRIMARY) && i->Tags.Contains(LAYOUT_HUD_MAXIMIZE))
		{
			InteractivePositioning->Transformation[i] = [this](auto){ return CTransformation(0, -Viewports.front()->H, Z); };

			BackPositioning->Transformation[i] = [this](auto){ return CTransformation(0, -Viewports.front()->H, Z + 7000); };

			HudView->GetCamera(i)->SetPosition(CFloat3(0, -Viewports.front()->H, 0));
			MainView->GetCamera(i)->SetPosition(CFloat3(0, -Viewports.front()->H, 0));
			ThemeView->GetCamera(i)->SetRotation(CFloat3(CFloat::PI/4, 0, 0));
		}
	}

	TopArea->SetPositioning(InteractivePositioning);
	HudArea->SetPositioning(InteractivePositioning);
	MainArea->SetPositioning(InteractivePositioning);
	FieldArea->SetPositioning(InteractivePositioning);
	BackArea->SetPositioning(BackPositioning);

	TopArea->PlaceNewDefault	= HudArea->PlaceNewDefault		= MainArea->PlaceNewDefault		= FieldArea->PlaceNewDefault	= InteractivePositioning->PlaceCenter;
	TopArea->PlaceNewConvenient = HudArea->PlaceNewConvenient	= MainArea->PlaceNewConvenient	= FieldArea->PlaceNewConvenient = InteractivePositioning->PlaceExact;
	TopArea->PlaceNewExact		= HudArea->PlaceNewExact		= MainArea->PlaceNewExact		= FieldArea->PlaceNewExact		= InteractivePositioning->PlaceExact;

	BackArea->PlaceNewDefault		= BackPositioning->PlaceExact;
	BackArea->PlaceNewConvenient	= BackPositioning->PlaceExact;
	BackArea->PlaceNewExact			= BackPositioning->PlaceExact;
}

CList<CUnit *> CDesktopWorld::CollectHidings(CArea * a, CArea * master)
{
	CList<CUnit *> hidings;

	if(FieldArea == master)
	{
		for(auto g : FieldArea->Areas.Where([this](auto i){ return i->Area && i->Area->Parent; }).Select<CUnit *>([](auto i){ return i->Area->As<CUnit>(); }))
		{
			hidings.push_back(g);
		}
	}
	if(ThemeArea == master)
	{
		for(auto g : ThemeArea->Areas.Where([this](auto i){ return i->Area && i->Area->Parent; }).Select<CUnit *>([](auto i){ return i->Area->As<CUnit>(); }))
		{
			hidings.push_back(g);
		}
	}

	return hidings;
}

void CDesktopWorld::OnMouse(CActive * r, CActive * s, CMouseArgs * arg)
{
	auto u = GetUnit(s);


	if(Drags.empty())
	{
		if(arg->Event == EGraphEvent::Captured && arg->Capture->InputAs<CMouseInput>()->Control == EMouseControl::MiddleButton)
		{
			StartMovement(arg->Capture->Pick, arg);
		}

		if(!Movings.empty())
		{
			if(arg->Event == EGraphEvent::Hover || arg->Event == EGraphEvent::Roaming)
			{
				ProcessMovement(arg->Pick, arg);
			}

			if(arg->Event == EGraphEvent::Input && arg->Action == EMouseAction::Off)
			{
				FinishMovement(arg);
			}
		}
		else
		{
			if(arg->Event == EGraphEvent::Input && arg->Action == EMouseAction::On)
			{
				if(u)
				{
					auto pa = u->AncestorOf<CPositioningArea>();
					
					if(pa)
					{
						pa->Activate(u, arg->Pick.Camera->Viewport, arg->Pick, u->Transformation);
					}
				}

				if(arg->Control == EMouseControl::LeftButton)
				{
					if(arg->Pick.Camera->Viewport->H - arg->Pick.Vpp.y < 5)
					{
						///ActionActivated(EWorldAction::OpenHud);
					}
				}
			}
			else if(u && arg->Event == EGraphEvent::Click)
			{
				auto p = new CShowParameters(arg, Style);

				if(arg->Control == EMouseControl::MiddleButton)
				{
					if(u->IsUnder(MainArea) || u->IsUnder(FieldArea))
					{
						p->Activate = true;
						Show(u, CArea::Background, p);
					}
				}
				if(arg->Control == EMouseControl::LeftButton)
				{
					if(BackArea->ContainsDescedant(u))
					{
						Show(u, CArea::LastInteractive, p);
					}
				}

				p->Free();
			}
			
			if(s == Sphere->Active)
			{
				ServiceSurfaceButtonEvent(arg);
			}
		}
	}
	else
	{
		if(arg->Control == EMouseControl::RightButton && arg->Action == EMouseAction::Off)
		{
			if(DropTarget)
			{
				auto dt = DropTarget->As<IDropTarget>();

				if(dt->Test(Drags, DragCurrentAvatar))
				{
					dt->Drop(Drags, arg->Pick);
				}
			}

			CancelDragDrop();
			arg->StopPropagation = true;
			arg->StopRelatedPropagation = true;
		}

		if(arg->Action == EMouseAction::Move)
		{
			if(arg->Event == EGraphEvent::Feedback)
			{
				auto d = arg->Device;

				if(DropTarget)
				{
					if(DropTarget->As<IDropTarget>()->Test(Drags, DragCurrentAvatar))
					{
						d->SetImage(LoadCursor(null, IDC_ARROW));
					}
					else
					{
						d->SetImage(LoadCursor(null, IDC_NO));
					}
				}
				else
				{
					d->SetImage(LoadCursor(null, IDC_NO));
				}
			}

			if(!InSpecialArea && arg->Pick.Camera->Viewport->H - arg->Pick.Vpp.y < 5)
			{
				///ActionActivated(EWorldAction::OpenHud);
				InSpecialArea = true;
			}
			else if(InSpecialArea && arg->Pick.Camera->Viewport->H - arg->Pick.Vpp.y > 5)
			{
				InSpecialArea = false;
			}

			auto dt = arg->Pick.Active->AncestorOwnerOf<IDropTarget>();

			if(dt != dynamic_cast<IDropTarget *>(DropTarget))
			{
				if(DropTarget)
				{
					DropTarget->As<IDropTarget>()->Leave(Drags, DragCurrentAvatar);
					DropTarget->Free();
					DropTarget = null;
				}

				DropTarget = dynamic_cast<CElement *>(dt);

				if(DropTarget)
				{
					dt->Enter(Drags, DragDefaultAvatar);
					DropTarget->Take();
				}

			}

			if(DragCurrentAvatar)
			{
				auto w = arg->Pick.Camera->Viewport->W;
				auto h = arg->Pick.Camera->Viewport->H;
				auto z = arg->Pick.Camera->UnprojectZ(w/2, w/2);

				DragCurrentAvatar->Transform(arg->Pick.Vpp.x - w/2 - DragCurrentAvatar->Size.W, arg->Pick.Vpp.y - h/2, z);
			}
		}
	}
}

void CDesktopWorld::OnTouch(CActive * r, CActive * s, CTouchArgs * arg)
{
	auto u = GetUnit(s);

	if(Movings.empty())
	{
		if(arg->Event == EGraphEvent::Input && arg->Input->Action == ETouchAction::Added)
		{
			if(TouchStage != EWorldAction::One && arg->Picks.size() == 1)
			{
				TouchStage = EWorldAction::One;
			}

			if(TouchStage != EWorldAction::Three && arg->Picks.size() == 3)
			{
				TouchStage = EWorldAction::Three;
			}
		}

		if(arg->Event == EGraphEvent::Captured &&	(u->IsUnder(BackArea)							&& TouchStage == EWorldAction::One || 
													(u->IsUnder(MainArea) || u->IsUnder(FieldArea)) && TouchStage == EWorldAction::Three))
		{
			if(arg->Input->Touch.Primary)
			{
				TouchStage = EWorldAction::Move;
				StartMovement(arg->Capture->Pick, arg);
			}
		}

		if(arg->Event == EGraphEvent::Input && arg->Input->Action == ETouchAction::Removed)
		{
			auto p = new CShowParameters(arg, Style);
	
			if(arg->Input->Touch.Primary)
			{
				if((u->IsUnder(MainArea) || u->IsUnder(FieldArea)) && TouchStage == EWorldAction::Three)
				{
					p->Activate = true;
					Show(u, CArea::Background, p);
				}
				else if(u->IsUnder(BackArea) && TouchStage == EWorldAction::One)
				{
					Show(u, CArea::LastInteractive, p);
				}
			
				TouchStage = EWorldAction::Null;
			}

			p->Free();
		}
	}
	else
	{
		if(arg->Input->Touch.Primary)
		{
			if(arg->Event == EGraphEvent::Input)
			{
				if(TouchStage == EWorldAction::Move)
				{
					if(arg->Input->Action == ETouchAction::Movement)
					{
						ProcessMovement(arg->Picks(arg->Input->Touch.Id), arg);
					}
			
					if(arg->Input->Action == ETouchAction::Removed)
					{
						FinishMovement(arg);
						TouchStage = EWorldAction::Null;
					}
				}
			}
		}
	}
}

void CDesktopWorld::OnKeyboard(CActive *, CActive * s, CKeyboardArgs * arg)
{
	if(!Drags.empty() && arg->Action == EKeyboardAction::On && arg->Control == EKeyboardControl::Escape)
	{
		CancelDragDrop();
		arg->StopPropagation = true;
	}

	if(GlobalHotKeys.Contains(arg->Control) && arg->Action == EKeyboardAction::On)
	{
		GlobalHotKeys[arg->Control](arg->Control);
		arg->StopPropagation = true;
	}
}

void CDesktopWorld::OnStateChanged(CActive * r, CActive * s, CActiveStateArgs * arg)
{
	if(arg->New == EActiveState::Active)
	{
	}
}

void CDesktopWorld::StartMovement(CPick & pk, CInputArgs * arg)
{
	auto u = GetUnit(pk.Active);

	//#ifdef _DEBUG
	//Level->Log->ReportDebug(this, L"%s", u->Name);
	//#endif 

	auto pa = u->AncestorOf<CPositioningArea>();

	if(pa && pa->Positioning)
	{
		Capture = pa->Positioning->Capture(pk, u->Measure(), pk.Space->Matrix);

		Movings.push_back(u);

		ActiveGraph->Root->IsPropagator = false;
		arg->StopPropagation = true;
		arg->StopRelatedPropagation = true;
	}
}

void CDesktopWorld::ProcessMovement(CPick & pk, CInputArgs * arg)
{
	auto pa = Movings[0]->AncestorOf<CPositioningArea>();

	if(pa)
	{
		auto t = pa->Positioning->Move(Capture, pk);
		t = t * pa->Positioning->GetMatrix(pk.Camera->Viewport).Decompose();
		Movings[0]->Transform(t);
	}
}

void CDesktopWorld::FinishMovement(CInputArgs * arg)
{
	Movings.clear();

	ActiveGraph->Root->IsPropagator = true;
	arg->StopPropagation = true;
	arg->StopRelatedPropagation = true;
}