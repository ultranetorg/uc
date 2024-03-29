#pragma once
#include "VwmElement.h"
#include "RectangleMenu.h"
#include "Fieldable.h"

namespace uc
{
	template<class E> class CIcon : public CAvatar, public CFieldable
	{
		public:
			CWorldProtocol *			Level;
			CServer *			Server;
			E *					Entity;
			CVwmImporter *		Importer = null;
			CRectangleMenu *	Menu = null;

			CIcon(CWorldProtocol * l, CServer * s, CString const & name) : CAvatar(l, s, name)
			{
				Level = l;
				Server = s;

				Active->MouseEvent[EListen::NormalAll] += ThisHandler(OnMouse);
			}

			~CIcon()
			{
				if(Menu)
				{
					Menu->Close();
					Menu->Free();
				}

				Active->MouseEvent -= ThisHandler(OnMouse);
			
				while(auto i = Nodes.First())
				{
					RemoveNode(i);
				}
			}
					
			virtual void SetEntity(CUol & e)
			{
				Entity = Server->FindObject<E>(e.Object);
			}

			CRectangle * UseRectangle()
			{
				CRectangle * r;

				if(Nodes.Empty())
				{
					r = new CRectangle(Level);
					///r->Visual->PixelPrecision = true;
					AddNode(r);
					r->Free();

					r->Express(L"W", [this]{ return Climits.Smax.W; });
					r->Express(L"H", [this]{ return Climits.Smax.H; });
					
					r->UpdateLayout(Climits, true);
				}
				else
					r = Nodes.First()->As<CRectangle>();

				return r;
			}

			void SetContentFromMaterial(CMaterial * t)
			{
				auto r = UseRectangle();

				r->Visual->SetMaterial(t);
			}

			void SetContentFromImage(CString & l)
			{
				auto r = UseRectangle();
				
				auto m = new CMaterial(Level->Engine->Level, Level->Engine->PipelineFactory->DiffuseTextureShader);
				m->AlphaBlending = true;
				r->Visual->SetMaterial(m);
				
				auto t = Level->Engine->TextureFactory->CreateTexture();
				
				auto s = Level->Storage->ReadFile(l);
				t->Load(s);
				Level->Storage->Close(s);
				
				r->Visual->Material->Textures[L"DiffuseTexture"] = t;
				r->Visual->Material->Samplers[L"DiffuseSampler"].SetAddressMode(ETextureAddressMode::Clamp, ETextureAddressMode::Clamp);
				
				t->Free();
				m->Free();
			}

			void CIcon::SetContentFromVWM(CString & l)
			{
				throw CException(HERE, L"Need update");

				///auto d = Level->Storage->OpenDirectory(l);
				///
				///auto e = new CVwmElement(Level, d, l[L"node"]);
				///		
				///Layout->SetFace(e);
				///e->Free();
				///
				///Level->Storage->Close(d);
			}

			virtual void OnMouse(CActive * r, CActive * s, CMouseArgs * a)
			{
				if(a->Control == EMouseControl::RightButton && a->Event == EGraphEvent::Click)
				{
					if(!Menu)
					{
						Menu = new CRectangleMenu(Level, Level->Style, L"Menu");

						Menu->Section->AddItem(L"Open")->Clicked = [this](auto a, auto *){ /*Activate(CShowFeatures(a));*/ };
						Menu->Section->AddSeparator();
		
						Field->AddIconMenu(Menu->Section, this);
						Field->AddTitleMenu(Menu->Section, this);
						Menu->Section->AddSeparator();
		
						Menu->Section->AddItem(L"Delete")->Clicked = [this](auto, auto){ Field->DeleteAvatar(this); };
						Menu->Section->AddSeparator();
							
						Menu->Section->AddItem(L"Properties...");
					}
			
	
					Menu->Open(a->Pick);

					a->StopPropagation = true;
				}
			}

			void DetermineSize(CSize & smax, CSize & s) override
			{
				Express(L"W", [s]{ return s.W; });
				Express(L"H", [s]{ return s.H; });

				UpdateLayout(CLimits::Empty, false);
			}
	};
}