#pragma once
#include "WorldServer.h"

namespace uc
{
	class CMobileWorld : public CWorldServer
	{
		public:
			CScreenViewport *			SkinViewport = null;
			CMobileSkinModel *			Skin = null;
			CWorldView *				SkinView = null;

			CPolygonalPositioning *		Positioning;	

			bool						ExternalTouch = false;

			CUnit *						History = null;
			CUnit *						Board = null;
			CUnit *						Tray = null;

			CMobileWorld(CNexus * l, CServerInfo * si) : CWorldServer(l, si) 
			{
				Name		= WORLD_MOBILE_EMULATION;
				Complexity	= AVATAR_WIDGET;
				Free3D		= false;
				FullScreen	= true;
				Tight		= true;
			}

			~CMobileWorld()
			{
				Skin->Free();
				Positioning->Free();
				delete SkinView;
			}

			void InitializeViewports() override
			{
				auto s = Engine->ScreenEngine->PrimaryScreen;
				auto t = Targets.Find([s](auto i){ return i->Screen = s; });
				
				SkinViewport = new CScreenViewport(Engine->Level, t,	t->Size.W / Engine->ScreenEngine->Scaling.x, t->Size.H / Engine->ScreenEngine->Scaling.y, 
																			0, 0,
																			t->Size.W, t->Size.H,
																			t->Screen->Rect.W, t->Screen->Rect.H);

				float x, y, w, h;
				
				if(Layout != L"PhoneB")
				{
					w = 370.f;
					h = 628.f;
					x = 115.f;
					y = 84.f;
				} 
				else
				{
					w = 370.f;
					h = 777.f;
					x = 115.f;
					y = 84.f;
				}

				MainViewport = new CScreenViewport(Engine->Level, t,	w, h,
																			x * Engine->ScreenEngine->Scaling.x,	y * Engine->ScreenEngine->Scaling.y,
																			w * Engine->ScreenEngine->Scaling.x,	h * Engine->ScreenEngine->Scaling.y,
																			w * Engine->ScreenEngine->DpiScaling.x,	h * Engine->ScreenEngine->DpiScaling.y);
				SkinViewport->Tags = {L"Skin"};
				MainViewport->Tags = {L"Apps", AREA_SERVICE_BACK, AREA_SERVICE_FRONT};

				Viewports.push_back(SkinViewport);
				Viewports.push_back(MainViewport);
			}
			
			void InitializeView() override
			{
				Z = 1000;

				auto ma = MainViewport->W/MainViewport->H;
				auto fovm = 2.f * ma * atan((MainViewport->W * 0.5f)/(ma * Z));

				auto sa = SkinViewport->W/SkinViewport->H;
				auto fovs = 2.f * sa * atan((SkinViewport->W * 0.5f)/(sa * Z));

				MainView	= new CWorldView(this, L"Main");
				HudView		= new CWorldView(this, L"Hud");
				ThemeView	= new CWorldView(this, L"Theme");
				SkinView	= new CWorldView(this, L"Skin");

				HudView->	AddCamera(MainViewport, fovm, 100, 1e4)->UseAffine();
				MainView->	AddCamera(MainViewport, fovm, 100, 15000)->UseAffine();
				ThemeView->	AddCamera(MainViewport, fovm, 10, 1e6)->UseAffine();
				SkinView->	AddCamera(SkinViewport, fovs, 10, 10000)->UseAffine();
			}

			void InitializeGraphs() override
			{
				ActiveGraph->Root->TouchEvent[EListen::NormalAll] += ThisHandler(OnTouch);
				ActiveGraph->Root->KeyboardEvent[EListen::NormalAll] += ThisHandler(OnKeyboard);
			}

			void InitializeAreas() override
			{
				
				auto w = MainViewport->W;
				auto h = MainViewport->H;

				Positioning = new CPolygonalPositioning(false);

				Positioning->GetView				= [this]			{ return MainView; };
				Positioning->Transformation[null]	= [this, w, h](auto){ return CTransformation(0, 0, Z); };
				Positioning->Bounds[null]			= [w, h]			{ return CArray<CFloat2>{{-w/2, -h/2}, {-w/2, h/2}, {w/2, h/2}, {w/2, -h/2}}; };

				FieldArea->	SetPositioning(Positioning);
				MainArea->	SetPositioning(Positioning);
				HudArea->	SetPositioning(Positioning);
				TopArea->	SetPositioning(Positioning);

				ServiceBackArea->As<CPositioningArea>()->SetPositioning(Positioning);
				ServiceFrontArea->As<CPositioningArea>()->SetPositioning(Positioning);
				
				Area->Match(AREA_SKIN)->As<CPositioningArea>()->SetPositioning(Positioning);
				Area->Match(AREA_SKIN)->As<CPositioningArea>()->SetView(SkinView);

				for(auto i : Area->Areas)
				{
					if(auto p = i->Area->As<CPositioningArea>())
					{
						p->PlaceNewDefault			= Positioning->PlaceCenter; 
						p->PlaceNewConvenient		= Positioning->PlaceCenter;
						p->PlaceNewExact			= Positioning->PlaceCenter;
					}
				}

			}

			void InitializeModels() override
			{
				Skin = new CMobileSkinModel(this);

				auto a = AllocateUnit(Skin);
				Show(a, AREA_SKIN, null);
			}

			CList<CUnit *> CollectHidings(CArea * a, CArea * master) override
			{
				CList<CUnit *> hidings;

				bool hidefs = false; 
				bool hidems = false; 
				bool hidehs = false; 
				
				if(master == FieldArea)
				{
					hidefs = hidems = hidehs = true;
				}
				if(master == MainArea)
				{
					hidems = hidehs = true;
				}
				if(master == HudArea)
				{
					hidehs = true;
				}
				
				for(auto b : Units.Where([&](CUnit * i){ return	(	(i != a) &&
																	(hidefs && i->Parent && FieldArea->ContainsDescedant(i) || 
																	hidems && i->Parent && MainArea->ContainsDescedant(i) || 
																	hidehs && i->Parent && HudArea->ContainsDescedant(i))); }))
				{
					hidings.push_back(b);
				}

				return hidings;
			}

			void OnKeyboard(CActive * r, CActive * s, CKeyboardArgs * arg)
			{
				if(arg->Action == EKeyboardAction::On)
				{
					if(GlobalHotKeys.Contains(arg->Control))
					{
						GlobalHotKeys[arg->Control](arg->Control);
						arg->StopPropagation = true;
					}
				}
			}

			void StartShowAnimation(CArea * a, CShowParameters * f, CTransformation & from, CTransformation & to) override
			{
				auto newfrom = CTransformation::Nan;

				if(auto arg = dynamic_cast<CTouchArgs *>(f->Args))
				{
					if(arg->Capture && arg->GetPick().Camera->Viewport != arg->Capture->Pick.Camera->Viewport)
					{
						auto d = arg->Input->Touch.Position - arg->Input->Touch.Origin;
	
						if(abs(d.x) > abs(d.y) && d.x > 0)	newfrom = CTransformation(-MainViewport->W, 0.f, to.Position.z); else
						if(abs(d.x) > abs(d.y) && d.x < 0)	newfrom = CTransformation( MainViewport->W, 0.f, to.Position.z); else
						if(abs(d.x) < abs(d.y) && d.y > 0)	newfrom = CTransformation(0.f, -MainViewport->H, to.Position.z); else
						if(abs(d.x) < abs(d.y) && d.y < 0)	newfrom = CTransformation(0.f,	MainViewport->H, to.Position.z);
					}
				}

				if(newfrom.IsReal())
				{
					a->Transform(newfrom);
					RunAnimation(a, CAnimated<CTransformation>(newfrom, to, f->Animation));
				}
				else
					__super::StartShowAnimation(a, f, from, to);
			}

			void StartHideAnimation(CArea * a, CHideParameters * f, CTransformation & from, CTransformation & to, std::function<void()> hide) override
			{
				auto newto = CTransformation::Nan;

				if(f && f->Args)
				{
					if(auto arg = f->Args->As<CTouchArgs>())
					{
						if(arg->GetPick().Camera->Viewport != arg->Capture->Pick.Camera->Viewport)
						{
							auto d = arg->Input->Touch.Position - arg->Input->Touch.Origin;
		
							if(abs(d.x) > abs(d.y) && d.x < 0)	newto = CTransformation(-MainViewport->W, 0.f, from.Position.z); else 
							if(abs(d.x) > abs(d.y) && d.x > 0)	newto = CTransformation( MainViewport->W, 0.f, from.Position.z); else 
							if(abs(d.x) < abs(d.y) && d.y > 0)	newto = CTransformation(0.f,  MainViewport->H, from.Position.z); else 
							if(abs(d.x) < abs(d.y) && d.y < 0)	newto = CTransformation(0.f, -MainViewport->H, from.Position.z);
	
							auto ani = CAnimated<CTransformation>(from, newto, f->Animation);
	
							Core->DoUrgent(	this,
											L"Hide animation",
											[this, a, ani, hide]() mutable
											{	
												if(ani.Animation.Running)
												{
													a->Transform(ani.GetNext());
													Engine->Update();
													return false;
												} 
												else
												{
													hide();
													return true;
												}
											});
							return;
						}
					}
				}

				__super::StartHideAnimation(a, f, from, to, hide);
			}


			void OnTouch(CActive *, CActive * s, CTouchArgs * arg)
			{
				if(arg->Event == EGraphEvent::Input && arg->Input->Action == ETouchAction::Added)
				{
					if(!ExternalTouch && s->HasAncestor(Skin->Active))
					{
						ExternalTouch = true;
					}
				}

				if(arg->Event == EGraphEvent::Enter)
				{
					if(ExternalTouch)
					{
						if(arg->GetPick().Camera->Viewport == MainViewport)
						{
							if(arg->GetPick().Camera->Viewport != arg->Capture->Pick.Camera->Viewport)
							{
								ExternalTouch = false;

								auto sf = new CShowParameters(arg, Style);

								auto d = arg->Input->Touch.Position - arg->Input->Touch.Origin;

								float x = 0;
								float y = 0;

								if(abs(d.x) > abs(d.y))
								{
									x = d.x;
								}
								if(abs(d.x) < abs(d.y))
								{
									y = d.y;
								}

								if(History && x < 0)
								{
									Hide(History, sh_new<CHideParameters>(arg, Style));
									History = null;
								}
								else if(Board && y < 0)
								{
									Hide(Board, sh_new<CHideParameters>(arg, Style));
									Board = null;
								}
								else if(Tray && y > 0)
								{
									Hide(Tray, sh_new<CHideParameters>(arg, Style));
									Tray = null;
								}
								else if(x > 0 && Components.Contains(WORLD_HISTORY))
								{
									History = OpenEntity(Components(WORLD_HISTORY), AREA_HUD, sf);
								}
								else if(y > 0 && Components.Contains(WORLD_BOARD))
								{
									Board = OpenEntity(Components(WORLD_BOARD), AREA_HUD, sf);
								}
								else if(y < 0 && Components.Contains(WORLD_TRAY))
								{
									Tray = OpenEntity(Components(WORLD_TRAY), AREA_HUD, sf);
								}

								sf->Free();
							}
						}
					}
				}

				if(arg->Event == EGraphEvent::Input && arg->Input->Action == ETouchAction::Removed)
				{
					if(ExternalTouch)
					{
						ExternalTouch = false;
					}
				
				//	ActiveGraph->Root->IsPropagator = true;
				}

			}

	};
}

