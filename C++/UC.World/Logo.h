#pragma once

namespace uc
{
	class UOS_WORLD_LINKING CLogo : public CModel
	{
		public:
			CWorld * 	Level;
			CText *		Mode;

			UOS_RTTI
			CLogo(CWorld * l) : CModel(l, l->Server, ELifespan::Visibility, GetClassName())
			{
				Level = l;
				Tags = {AREA_SERVICE_FRONT, L"Apps"};

				auto t = Level->Engine->TextureFactory->CreateTexture();
				t->Load(Level->Server->Info->HInstance, IDB_UOS_PNG);

				auto mtl = new CMaterial(Level->Engine->Level, Level->Engine->PipelineFactory->DiffuseTextureShader);
				mtl->AlphaBlending = true;
				mtl->Textures[L"DiffuseTexture"] = t;	
				mtl->Samplers[L"DiffuseSampler"].SetFilter(ETextureFilter::Point, ETextureFilter::Point, ETextureFilter::Point);


				auto msh = new CSolidRectangleMesh(Level->Engine->Level);
				msh->Generate(0, 0, float(t->W), float(t->H));
				Visual->SetMaterial(mtl);
				Visual->SetMesh(msh);

				Size = CSize(float(t->W), float(t->H), 0);

				t->Free();
				msh->Free();
				mtl->Free();


				Mode = new CText(l, l->Style);
				Mode->SetFont(l->Engine->FontFactory->GetFont(L"Verdana", 18, true, false));
				
				Visual->AddNode(Mode->Visual);

			}

			~CLogo()
			{
				Mode->Free();
			}

			void SetText(CString const & t)
			{
				Mode->SetText(t);
			}

			virtual void DetermineSize(CSize & smax, CSize & s) override
			{
				if(smax.W < Size.W)
				{
					auto a = smax.W/Size.W;
					Size = Size * a * 0.75f;
					Visual->Mesh->As<CSolidRectangleMesh>()->Generate(0, 0, Size.W, Size.H);

				}

				Mode->UpdateLayout({smax, smax}, true);
				Mode->Transform((Size.W - Mode->W)/2, -smax.H/2 + Size.H, 0);
			}

			virtual CTransformation DetermineTransformation(CPositioning * ps, CPick & pk, CTransformation & t) override
			{
				return CTransformation(-Size.W/2, -Size.H/2, Unit->GetActualView()->PrimaryCamera->UnprojectZ(1, 1));;
			}
	};
}
