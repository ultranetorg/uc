#include "stdafx.h"
#include "ThemeAvatar.h"

using namespace uc;

CThemeAvatar::CThemeAvatar(CShellLevel * l, CString const & name) : CModel(l->World, l->Server, ELifespan::Permanent, name)
{
	Level = l;
}
	
CThemeAvatar::~CThemeAvatar()
{
	if(Entity)
	{
		///SaveInstance();
		Entity->Destroying -= ThisHandler(OnDependencyDestroying);
	}

	if(Root)
		Root->Free();
}

void CThemeAvatar::SetEntity(CUol & e)
{
	Entity = Server->FindObject(e);
	//Entity->AddGlobalReference(Url);
	Entity->Destroying += ThisHandler(OnDependencyDestroying);	

	LoadScene();
}

void CThemeAvatar::OnDependencyDestroying(CNexusObject * o)
{
	if(Entity && Entity == o)
	{
		///SaveInstance();
		Entity.Clear();
	}
}

void CThemeAvatar::SaveInstance()
{
	__super::SaveInstance();
}

void CThemeAvatar::LoadInstance()
{
	__super::LoadInstance();

}
	
void CThemeAvatar::LoadScene()
{
	if(Entity->Importer)
	{
		std::function<CXon * (CXon *)>	findActor =	[&findActor](auto xn) -> CXon *
													{
														if(xn->Get<CString>() == L"ActorDefault")
														{
															return xn;
														}
														for(auto i : xn->Many(L"Node"))
														{
															auto r = findActor(i);
															if(r != null)
															{
																return r;
															}
														}
														return null;
													};
	
		auto root = Entity->Importer->GetMetadata()->One(L"Node");
		auto actor = findActor(root);
		auto t = actor->Get<CTransformation>(L"Transformation");
					
		//auto pm = CMatrix::FromPosition(t.Position).GetInversed()/* * CMatrix::FromScaling(100, 100, 100)*/;
			
		CAABB bb;
	
	/*	std::function<CWorldNode *(CXon *)>	loadNode =	[&](CXon * p) -> CWorldNode *
														{
															auto n = new CWorldNode(Level->World, p->Get<CString>());
	
															n->Transform(p->Get<CTransformation>(L"Transformation"));
															n->SetView(Level->World->ThemeView);
	
															Importer->LoadVisual(p, n->Visual);
	
															bb = bb.Join(n->Visual->GetOBB(n->Visual->FinalMatrix));
	
															for(auto  i : p->Many(L"Node"))
															{
																auto nn = loadNode(i);
																n->AddNode(nn);
																nn->Free();
															}
	
															return n;
														};*/
	
		Root = Entity->Importer->ImportNodeTree(root);
		Size = Root->Visual->GetAABB().GetSize();
		//Root->SetView(Level->World->ThemeView);
	
//		auto m = new CBoxMesh(&Level->Engine->EngineLevel);
//		m->Generate(bb.Min.x, bb.Min.y, bb.Min.z, bb.GetWidth(), bb.GetHeight(), bb.GetDepth());
//		Active->SetMesh(m);
//		m->Free();
	
		CameraPosition = t.Position;

		for(auto i : Level->World->ThemeView->Cameras)
		{
			i->SetPosition(CameraPosition);
		}
	}
	else
	{
		Root = new CRectangle(Level->World, L"Wallpaper");
		auto r = Root->As<CRectangle>();

		auto m = new CMaterial(&Level->Engine->EngineLevel, Level->Engine->PipelineFactory->DiffuseTextureShader);
		r->EnableActiveBody = false;
		r->Visual->SetMaterial(m);
		m->Free();

		auto t = Level->Engine->TextureFactory->CreateTexture();

		auto s = Level->Storage->OpenReadStream(Entity->Source);
		t->Load(s);
		Level->Storage->Close(s);

		m->Textures[L"DiffuseTexture"] = t;

		t->Free();
		
	}

	AddNode(Root);
}

void CThemeAvatar::UpdateLayout(CLimits const & l, bool apply)
{
	if(auto r = Root->As<CRectangle>())
	{
		r->UpdateLayout(l, apply);
		r->Transform(-r->W/2, -r->H/2, 1000);
	}
}

void CThemeAvatar::DetermineSize(CSize & smax, CSize & s)
{
	if(auto r = Root->As<CRectangle>())
	{
		auto t = Root->Visual->Material->Textures[L"DiffuseTexture"].Texture;

		//auto a = t->Width/t->Height;

		auto k = max(smax.W/t->W, smax.H/t->H);

		auto w = t->W * k;
		auto h = t->H * k;

		r->Express(L"W", [w]{ return w; });
		r->Express(L"H", [h]{ return h; });

		//r->UpdateLayout(l, apply);
		//r->Transform(-w/2, -h/2, Level->World->ThemeView->PrimaryCamera->UnprojectZ(smax.W/2, smax.W/2));
	}

	UpdateLayout(CLimits::Empty, false);
}

CTransformation CThemeAvatar::DetermineTransformation(CPositioning * ps, CPick & pk, CTransformation & t)
{
	return CTransformation::Identity;
}
