#pragma once
#include "Tile.h"
#include "GeoStore.h"

namespace uc
{
	class CGlobe : public CRectangle
	{
		public:
			CExperimentalLevel *		Level;

			int							T; // tiles dimention
			int							S = 32; // segments per Lg and Lt
			float						ZoomLevel = 0;
			float						TouchZoomLevel = 0; 
			CTile *						Root = null;
			float						R;
			bool						IsRotating = false;

			float						Latitude = 0.f;
			float						Longitude = 0.f;

			CMaterial *					Notloaded = null;

			CString						MapStyle = L"satellite-streets-v11";

			CAnimated<float>			ZoomAnimation;
			CCamera *					ZoomCamera = null;

			CDiagnostic *				Diagnostic;
			CList<CString>				Report;

			CUnit *						Unit = null;

			CFloat2						LastTouch;
			CGeoStore *					Store;

			CViewport *					Viewport;
			CCamera *					Camera;

			UOS_RTTI
			CGlobe(CExperimentalLevel * l, CGeoStore * s) : CRectangle(l->World, GetClassName())
			{
				Level = l;
				Store = s;

				Active->MouseEvent[EListen::Normal] += ThisHandler(OnMouse);
				Active->TouchEvent[EListen::Normal] += ThisHandler(OnTouch);

				UseClipping(EClipping::Inherit, true);

				Notloaded = new CMaterial(Level->Engine->Level, Level->Engine->PipelineFactory->DiffuseColorShader);
				Notloaded->Float4s[L"DiffuseColor"] = CFloat4(0.f, 0.f, 0.1f, 1.f);
			
				auto uol = Level->Server->MapSystemPath(L"Earth/Cache");
				Level->Storage->CreateDirectory(uol);

				Diagnostic	= Level->Core->Supervisor->CreateDiagnostics(L"Globe");
				Diagnostic->Updating += ThisHandler(OnDiagnosticUpdateing);

				Viewport = new CViewport(Level->Engine->Level);
				Camera = new CCamera(Level->Engine->Level, Viewport, L"Static", CFloat::PI/2, 100, 10000);
				Camera->UseAffine();
			}

			~CGlobe()
			{
				Diagnostic->Updating -= ThisHandler(OnDiagnosticUpdateing);

				delete Root;
				Notloaded->Free();

				delete Camera;
				delete Viewport;
			}


			void OnDiagnosticUpdateing(CDiagnosticUpdate & u)
			{
				for(auto i : Report)
				{
					Diagnostic->Add(i);
				}
			}

			void CGlobe::OnMouse(CActive * r, CActive * s, CMouseArgs * arg)
			{
				if(arg->Event == EGraphEvent::Input)
				{
					if(arg->Action == EMouseAction::Rotation/* && !ZoomAnimation.Animation.Running*/)
					{
						Zoom(ZoomLevel + arg->RotationDelta.y, arg->Pick.Camera);
					}
				}

				if(arg->Action == EMouseAction::Move)
				{
					if(arg->Event == EGraphEvent::Captured && arg->Capture->InputAs<CMouseInput>()->Control == EMouseControl::LeftButton)
					{
						IsRotating = true;
					}
		
					if(IsRotating)
					{
						if(arg->Event == EGraphEvent::Hover || arg->Event == EGraphEvent::Roaming)
						{
							auto lt = Latitude + arg->PositionDelta.y * (+0.002f / pow(2.f, ZoomLevel + 1));
							auto lg = Longitude + arg->PositionDelta.x * (-0.002f / pow(2.f, ZoomLevel + 1));
		
							Rotate(lt, lg, arg->Pick.Camera);
						}
					}
				}

				if(arg->Action == EMouseAction::Off)
				{
					if(IsRotating)
					{
						if(arg->Control == EMouseControl::LeftButton && arg->Event == EGraphEvent::Released)
						{
							IsRotating = false;
						}
					}
				}
			
			}

			void CGlobe::OnTouch(CActive * r, CActive * s, CTouchArgs * arg)
			{
				if(arg->Picks.size() == 2)
				{
					if(arg->Input->Action == ETouchAction::Added)
					{
						TouchZoomLevel = ZoomLevel;
					}

					if(arg->Input->Action == ETouchAction::Movement)
					{
						auto a = *arg->Input->Touches.begin();
						auto b = *++arg->Input->Touches.begin();

						auto c = *arg->Input->Touches.begin();
						auto d = *++arg->Input->Touches.begin();
	
						auto s = (a.Position - b.Position).GetLength() - (c.Origin - d.Origin).GetLength();
	
						Zoom(TouchZoomLevel + s/100.f, arg->GetPick().Camera);
					}
				}

				if(arg->Picks.size() == 1)
				{
					if(arg->Event == EGraphEvent::Captured)
					{
						IsRotating = true;
						LastTouch = arg->Input->Touch.Position;
					}

					if(IsRotating)
					{
						if(arg->Event == EGraphEvent::Hover || arg->Event == EGraphEvent::Roaming)
						{
							auto d = arg->Input->Touch.Position - LastTouch;
						
							LastTouch = arg->Input->Touch.Position;

							auto lt = Latitude + d.y * (+0.002f / pow(2.f, ZoomLevel + 1));
							auto lg = Longitude + d.x * (-0.002f / pow(2.f, ZoomLevel + 1));
	
							Rotate(lt, lg, arg->GetPick().Camera);
						}

						if(arg->Event == EGraphEvent::Input && arg->Input->Action == ETouchAction::Removed)
						{
							IsRotating = false;
						}
					}
				}
			}

			void UpdateLayout(CLimits const & l, bool apply) override
			{
				__super::UpdateLayout(l, apply);

				if(apply && !Root && IW > 0 && IH > 0)
				{ 
					Viewport->W = IW;
					Viewport->H = IH;

					if(Level->World->FullScreen)
					{
						auto a = IW/IH;
						Camera->Fov = 2.f * a * atan((IW*0.5f)/(a * 1000));
					} 
					else
					{
						Camera->Fov = Level->World->MainView->PrimaryCamera->Fov;
					}
					Camera->UpdateViewProjection();

					Report.clear();

					R = min(IW, IH)/2 * 0.8f;
					Root = new CTile(Level, this, null, Notloaded, R, 0, 0, 0, &Report);
					
					Build();

					if(!Root->Visual->Parent)
						Visual->AddNode(Root->Visual);

				}
			}

			void Zoom(float zoom, CCamera * c)
			{
				zoom = CFloat::Clamp(zoom, 0.f, 15.f);
				
		
				ZoomCamera = c;
				ZoomAnimation = CAnimated<float>(ZoomLevel, zoom,  Level->Style->GetAnimation(L"Animation"));

				Level->Core->DoUrgent(this, L"Zooming",	[this]
														{
															ZoomLevel = ZoomAnimation.GetNext();

															Build();

															return !ZoomAnimation.Animation.Running;
														});
			}

			void Rotate(float lt, float lg, CCamera * c)
			{
				Latitude = CFloat::Clamp(lt, -CFloat::PI/2, CFloat::PI/2);
				Longitude = lg;
		
				Build();
			}

			void Build()
			{
				CTransformation t;

				t.Scale = float(pow(2, ZoomLevel));
				t.Position = O + CFloat3(IW/2, IH/2, R * t.Scale.z - R);
				t.Rotation = CQuaternion(0, Longitude, 0)  * CQuaternion(Latitude, 0, 0);

				auto  m = CMatrix(t);

				Root->Visual->SetMatrix(m);

				auto n = Root->Build(Camera, int(ZoomLevel) + 2, CList<CMatrix>{m * Visual->FinalMatrix * CMatrix(0, 0, 1000) });
			}

			void Save(CXon * n)
			{
				n->Add(L"ZoomLevel")->Set(ZoomLevel);
				n->Add(L"Longitude")->Set(Longitude);
				n->Add(L"Latitude")->Set(Latitude);
			}

			void Load(CXon * n)
			{
				ZoomLevel = n->Get<CFloat>(L"ZoomLevel");
				Longitude = n->Get<CFloat>(L"Longitude");
				Latitude = n->Get<CFloat>(L"Latitude");
			}
	};
}