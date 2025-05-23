#pragma once

namespace uc
{
	class UOS_WORLD_LINKING CSphere : public CModel
	{
		public:
			CWorldProtocol * 									Level;

			UOS_RTTI
			CSphere(CWorldProtocol * l) : CModel(l, l->Server, ELifespan::Session, GetClassName())
			{
				Level = l;

				Visual->Enable(false);

				auto m = new CSphereMesh(l->Engine->Level);
				//w = MainPerspView->PrimaryCamera->GetX(1000000, MainViewport->Width/2);
				//h = MainPerspView->PrimaryCamera->GetY(1000000, MainViewport->Height/2);
				m->Generate(0, 0, 0, 1e6, 16);
				
				Transform(0, 0, 0);
				Active->SetMesh(m);
				Active->Clipping = EClipping::No;

				m->Free();
			}

			~CSphere()
			{
			}

			virtual void DetermineSize(CSize & smax, CSize & s) override
			{
				//if(ms.W < Size.W)
				//{
				//	auto a = ms.W/Size.W;
				//	Size = Size * a * 0.8f;
				//	Visual->Mesh->As<CsectMesh>()->Generate(0, 0, 0, Size.W, Size.H);
				//}
			}

			virtual CTransformation DetermineTransformation(CPositioning * ps, CPick & pk, CTransformation & t) override
			{
				return Transformation;
			}
	};
}
