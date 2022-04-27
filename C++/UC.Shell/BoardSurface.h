#pragma once
#include "FieldSurface.h"

namespace uc
{
	class CBoardSurface : public CFieldSurface
	{
		public:
			CShellLevel *								Level;
			CView *										View;
			CSolidQuadragonMesh *						SolidMesh;
			CGridQuadragonMesh *						GridMesh;
			bool										Perspective = false;

			UOS_RTTI
			CBoardSurface(CShellLevel * l) : CFieldSurface(l->World, GetClassName())
			{
				Level = l;

				SolidMesh = new CSolidQuadragonMesh(&l->Engine->EngineLevel);
				Active->SetMesh(SolidMesh);

				Visual->SetMesh(SolidMesh);
				Visual->SetMaterial(l->World->Materials->GetMaterial(L"0 0 0 0.5"));

				GridMesh = new CGridQuadragonMesh(&Level->World->Engine->EngineLevel);
												
				auto v = Level->Engine->CreateVisual(L"grid", GridMesh, l->World->Materials->GetMaterial(L"0 0.3 0.3"), CMatrix(0, 0, Z_STEP));
				Visual->AddNode(v);
				v->Free();
			}

			~CBoardSurface()
			{
				GridMesh->Free();
				SolidMesh->Free();
			}

			void UpdateLayout(CLimits const & l, bool apply) override
			{
				__super::UpdateLayout(l, apply);

				if(apply)
				{

					if(Perspective)
					{
						float f = 100;
						Polygon = {{0, 0}, {f, H}, {W-f, H}, {W, 0}};
						SolidMesh->Generate(Polygon[0], Polygon[1], Polygon[2], Polygon[3]);
						GridMesh->Generate({0, 0}, {f, H}, {W-f, H}, {W, 0}, int(W/24), int(sqrt(f*f + H*H)/24), 1.f);
					}
					else
					{
						Polygon = {{0, 0}, {0, H}, {W, H}, {W, 0}};
						SolidMesh->Generate(Polygon[0], Polygon[1], Polygon[2], Polygon[3]);
						GridMesh->Generate(Polygon[0], Polygon[1], Polygon[2], Polygon[3], int(W/24), int(W/24 * H/W), 1.f);
					}
				}
			}
	};
}

