#pragma once
#include "Text.h"

namespace uc
{
	class CMobileSkinModel : public CModel
	{
		public:
			CWorld *									Level;
			
			CText *										SwipeLeft;
			CText *										SwipeRight;
			CText *										SwipeDown;
			CText *										SwipeUp;
			
			CSolidRectangleMesh *						Frame;

			CList<CTouch *> Touches;

			UOS_RTTI
			CMobileSkinModel(CWorld * l) : CModel(l, l->Server, ELifespan::Visibility, GetClassName())
			{
				Level = l;
				Tags = {L"Skin"};

				auto path = Level->Server->MapReleasePath(Level->Layout == L"PhoneB" ? L"PhoneB.png" : L"Phone.png");
				auto f = Level->Storage->ReadFile(path);
				auto t = Level->Engine->TextureFactory->CreateTexture();
				t->Load(f);
				Level->Storage->Close(f);

				auto m = new CMaterial(Level->Engine->Level, Level->Engine->PipelineFactory->DiffuseTextureShader);
				m->AlphaBlending = true;
				m->Textures[L"DiffuseTexture"] = t;
				m->Samplers[L"DiffuseSampler"].SetFilter(ETextureFilter::Point, ETextureFilter::Point, ETextureFilter::Point);
				Visual->SetMaterial(m);

				Frame = new CSolidRectangleMesh(l->Engine->Level);
				Visual->SetMesh(Frame);
				Active->SetMesh(Frame);
				Frame->Free();

				m->Free();
				t->Free();

				Active->MouseEvent[EListen::Normal]	+= ThisHandler(OnMouse);
				Active->TouchEvent[EListen::Normal]	+= ThisHandler(OnTouch);

				auto lables = true;
				auto placeholder = L"                         ";
				
				if(auto a = Level->Server->Command->One(L"Skin/ShowLabels"))
				{
					lables = a->Get<CBool>();
				}

				SwipeLeft	= new CText(l, l->Style, L"SwipeLeft",	true);
				SwipeRight	= new CText(l, l->Style, L"SwipeRight",	true);
				SwipeDown	= new CText(l, l->Style, L"SwipeDown",	true);
				SwipeUp		= new CText(l, l->Style, L"SwipeUp",	true);

				SwipeLeft	->SetText(lables ? L"Click to Swipe Left"	: placeholder);
				SwipeRight	->SetText(lables ? L"Click to Swipe Right"	: placeholder);
				SwipeDown	->SetText(lables ? L"Click to Swipe Down"	: placeholder);
				SwipeUp		->SetText(lables ? L"Click to Swipe Up"		: placeholder);

/*				float w = l->MainViewport->As<CScreenViewport>()->Target->Screen->Rect.W;// 370.f-1; 
				float h = l->MainViewport->As<CScreenViewport>()->Target->Screen->Rect.H; //628.f-1;

				SwipeRight->Active->MouseEvent[EListen::Normal] +=	[this, w, h](auto, auto, CMouseArgs * a) mutable
																	{
																		if(a->Event == EGraphEvent::Click)
																		{
																			Swipe(a->Screen, a->Pick.ScreenPoint.x, a->Pick.ScreenPoint.y, w/2, 0);
																		}
																	};

				SwipeLeft->Active->MouseEvent[EListen::Normal] +=	[this, w, h](auto, auto, auto a) mutable
																	{
																		if(a->Event == EGraphEvent::Click)
																		{
																			Swipe(a->Screen, a->Pick.ScreenPoint.x, a->Pick.ScreenPoint.y, -w/2, 0);
																		}
																	};

				SwipeUp->Active->MouseEvent[EListen::Normal] +=	[this,  w, h](auto, auto, auto a) mutable
																{
																	if(a->Event == EGraphEvent::Click)
																	{
																		Swipe(a->Screen, a->Pick.ScreenPoint.x, a->Pick.ScreenPoint.y, 0, h/2);
																	}
																};

				SwipeDown->Active->MouseEvent[EListen::Normal] +=	[this, w, h](auto, auto, auto a) mutable
																	{
																		if(a->Event == EGraphEvent::Click)
																		{
																			Swipe(a->Screen, a->Pick.ScreenPoint.x, a->Pick.ScreenPoint.y, 0, -h/2);
																		}
																	};*/

				AddNode(SwipeLeft);
				AddNode(SwipeRight);
				AddNode(SwipeDown);
				AddNode(SwipeUp);
			}

			~CMobileSkinModel()
			{
				for(auto i : Touches)
				{
					delete i;
				}

				RemoveNode(SwipeLeft);
				RemoveNode(SwipeRight);
				RemoveNode(SwipeDown);
				RemoveNode(SwipeUp);

				SwipeLeft->Free();
				SwipeRight->Free();
				SwipeDown->Free();
				SwipeUp->Free();
			}

			virtual void DetermineSize(CSize & smax, CSize & s) override
			{
				Express(L"W", [smax]{ return smax.W; });
				Express(L"H", [smax]{ return smax.H; });
				UpdateLayout(CLimits(smax, smax), false);
			}

			virtual CTransformation DetermineTransformation(CPositioning * ps, CPick & pk, CTransformation & t) override
			{
				return t;
			}

			void virtual UpdateLayout(CLimits const & l, bool apply) override
			{
				__super::UpdateLayout(l, apply);

				Frame->Generate(0, 0, Size.W, Size.H);

				SwipeRight->Transform(CTransformation(CFloat3(1, (Size.H + SwipeLeft->W)/2.f, Z_STEP), CQuaternion(0, 0, float(-M_PI_2)), 1.f));
				SwipeLeft->Transform(CTransformation(CFloat3(Size.W - SwipeRight->H, (Size.H + SwipeRight->W)/2.f, Z_STEP), CQuaternion(0.f, 0.f, -float(M_PI_2)), 1.f));
				SwipeDown->Transform(ceil((Size.W - SwipeDown->W)/2.f), Size.H - SwipeDown->H - 64, Z_STEP);
				SwipeUp->Transform(ceil((Size.W - SwipeUp->W)/2.f), 66, Z_STEP);
			}

			void OnMouse(CActive *, CActive *, CMouseArgs * a)
			{
				if(a->Event == EGraphEvent::Click)
				{
					OnClick(a->Screen, a->Pick.Vpp);
				}
			}

			void OnTouch(CActive *, CActive *, CTouchArgs * a)
			{
				if(a->Event == EGraphEvent::Click)
				{
					OnClick(a->GetPick().Camera->Viewport->As<CScreenViewport>()->Target->Screen, a->GetPick().Vpp);
				}
			}

			void OnClick(CScreen * s, CFloat2 & p)
			{
				float w = (float)Level->MainViewport->As<CScreenViewport>()->Target->Screen->Rect.W;// 370.f-1; 
				float h = (float)Level->MainViewport->As<CScreenViewport>()->Target->Screen->Rect.H; //628.f-1;

				if(CRect(250, 17, 106, 45).Contains(p))
				{
					GoHome();
				}
				else if(p.x < 115)
				{
					Swipe(s, p.x, p.y, w/2, 0);
				}
				else if(p.x > 487)
				{
					Swipe(s, p.x, p.y, -w/2, 0);
				}
				else if(p.y < 84)
				{
					Swipe(s, p.x, p.y, 0, h/2);
				}
				else if(Level->Layout == L"PhoneB" ? p.y > 777 : p.y > 712)
				{
					Swipe(s, p.x, p.y, 0, -h/2);
				}
			}

			void Swipe(CScreen * sc, float x, float y, float dx, float dy)
			{
				auto move = [this, sc](float x, float y, float dx, float dy, ETouchAction a)
							{
								auto m = new CTouchInput();
								m->Device	= null;
								m->Action	= a;
								m->Screen	= sc;
								m->Id		= Level->Engine->InputSystem->GetNextID();
								
								auto s = Level->Engine->ScreenEngine->Scaling;

								CTouch t;
								t.Id = 1;
								t.Primary = true;
								t.Origin = CFloat2(x, y) * s;
								t.Position = t.Origin + CFloat2(dx, dy) * s;

								m->Touch	= t;
								m->Touches	= {t};

								Level->Engine->InputSystem->SendInput(m);

								m->Free();
							};
						
				move(x, y, 0, 0, ETouchAction::Added);

				for(float i=0; i<1; i+=0.1f)
				{
					move(x, y, dx * i, dy * i, ETouchAction::Movement);
				}

				move(x, y, dx, dy, ETouchAction::Removed);
			};

			void GoHome()
			{
				auto im = new CKeyboardInput();
				im->Control	= EKeyboardControl::GlobalHome;
				im->Action	= EKeyboardAction::On;

				Level->Engine->InputSystem->SendInput(im);
				im->Free();

				im = new CKeyboardInput();
				im->Control	= EKeyboardControl::GlobalHome;
				im->Action	= EKeyboardAction::Off;

				Level->Engine->InputSystem->SendInput(im);
				im->Free();
			}
	};
}
