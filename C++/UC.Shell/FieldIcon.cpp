#include "StdAfx.h"
#include "FieldIcon.h"

using namespace uc;

CFieldIcon::CFieldIcon(CShellLevel * l, CString const & name) : CIcon(l->World, l->Server, name)
{
	Level = l;

	AMesh = new CSolidRectangleMesh(&l->World->Engine->EngineLevel);
	Active->SetMesh(AMesh);

	auto msh = new CMesh(&Level->World->Engine->EngineLevel);
	msh->SetPrimitiveInfo(EPrimitiveType::TriangleList);	
	Visual->SetMesh(msh);
	msh->Free();

	auto mtl = Level->World->Materials->GetMaterial(L"1 1 1");
	Visual->SetMaterial(mtl);

}
	
CFieldIcon::~CFieldIcon()
{
	AMesh->Free();
}

void CFieldIcon::OnModelOpened(CModel * a)
{
}

void CFieldIcon::OnModelClosed(CModel * a)
{
}

void CFieldIcon::UpdateLayout(CLimits const & l, bool apply)
{
	__super::UpdateLayout(l, apply);
	
	if(apply && IW > 0 && IH > 0)
	{
		if(Visual->Mesh)
		{
			Visual->Mesh->Clear();
		}
		
		auto t = 0.75f * min(IW, IH) / 3;
	
		auto m = new CSolidRectangleMesh(&Level->World->Engine->EngineLevel);
	
		for(int x=0; x<3; x++)
		{
			for(int y=0; y<3; y++)
			{
				float xx = 0.f;
				if(x == 1)		xx = IW/2 - t/2;
				else if(x == 2) xx = IW - t;
			
				float yy = 0.f;
				if(y == 1)		yy = IH/2 - t/2;
				else if(y == 2) yy = IH - t;
			
						
				m->Generate(O.x + xx, O.y + yy, t, t);
	
				Visual->Mesh->Merge(m);
			}
		}
	
		m->Free();
		
		AMesh->Generate(O.x, O.y, IW, IH);
	}
}
