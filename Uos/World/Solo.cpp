#include "stdafx.h"
#include "Solo.h"
#include "WorldServer.h"

using namespace uc;

CSolo::CSolo(CWorldServer * l, CModel * m, CView * hview) : CUnit(l, hview, CGuid::Generate64(GetClassName()))
{
	Model = m;
	m->Take();
	Initialize();
}

CSolo::CSolo(CWorldServer * l, CString & dir, CUol & entity, CString const & type, CView * hview) : CUnit(l, hview, entity.Object)
{
	Directory = dir;
	Model = l->CreateAvatar(l->GenerateAvatar(entity, type), dir)->As<CModel>();
	Entity = Model->Protocol->GetEntity(entity);
	Initialize();
}

CSolo::CSolo(CWorldServer * l, CString & dir, CView * hview, CString const & name) : CUnit(l, hview, name)
{
	Directory = dir;

	Load();

	auto adir = CPath::Join(Directory, CNativePath::GetSafe(Model.Url.Object));
	Model = l->CreateAvatar(Model.Url, adir)->As<CModel>();
	Entity = Model->Protocol->GetEntity(Entity.Url);
	Initialize();
}

CSolo::~CSolo()
{
	Model->Close(this);

	if(Header)
	{
		if(Header->Parent)
			Remove(Header);
	
		Header->Free();
	}

	if(Model->Active->Parent == Level->ActiveGraph->Root)
		Level->ActiveGraph->Root->RemoveNode(Model->Active);

	Model->Save();
	
	if(Model->Protocol)
		Level->DestroyAvatar(Model);
	else
		Model->Free();
}

void CSolo::Initialize()
{
	Model->Unit = this;

	Lifespan	= Model->Lifespan;
	Tags		= Model->Tags;
		
	if(Model->UseHeader)
	{
		Header = new CHeader(Level, HeaderView);
		Header->Tags				= {L"all"};
		Header->TitleMode			= ECardTitleMode::Right;
		Header->IconSize			= {24, 24, 0};
		Header->Stack->Direction	= EDirection::X;
		Header->Stack->XAlign		= Level->Free3D ? EXAlign::Center : EXAlign::Left;
		Header->Stack->YAlign		= EYAlign::Center;

		Header->AddTab(Model.Url, Model);

		Add(Header, EAddLocation::Front);
	}

	// active as root
	if(!Model->Active->Parent)
		Level->ActiveGraph->Root->AddNode(Model->Active);

	if(Lifespan == ELifespan::Permanent)
	{
		auto adir = CPath::Join(Directory, CNativePath::GetSafe(Model.Url.Object));
		Model->SetDirectories(adir);
	}

	Model->Open(Level, this);
}

CSpaceBinding<CVisualSpace> & CSolo::AllocateVisualSpace(CViewport * vp)
{
	auto & s = __super::AllocateVisualSpace(vp);
	s.Space->AddVisual(Model->Visual); // nodes to graphs
	return s;
}

CSpaceBinding<CActiveSpace> & CSolo::AllocateActiveSpace(CViewport * vp)
{
	auto & s = __super::AllocateActiveSpace(vp);
	s.Space->AddActive(Model->Active);
	return s;
}

void CSolo::DeallocateVisualSpace(CVisualSpace * s)
{
	s->RemoveVisual(Model->Visual);
	__super::DeallocateVisualSpace(s);
}

void CSolo::DeallocateActiveSpace(CActiveSpace * s)
{
	s->RemoveActive(Model->Active);
	__super::DeallocateActiveSpace(s);
}

CSize CSolo::DetermineSize(CSize & smax, CSize & s)
{
	Model->DetermineSize(smax, s);
	return Model->Size;
}

CTransformation CSolo::DetermineTransformation(CPositioning * ps, CPick & pk, CTransformation & t)
{
	return Model->DetermineTransformation(ps, pk, t);
}

EPreferedPlacement CSolo::GetPreferedPlacement()
{
	return Model->GetPreferedPlacement();
}

void CSolo::Update()
{
	Model->UpdateLayout();
	UpdateHeader();
}

void CSolo::UpdateHeader()
{
	if(Header)
	{
		Header->Transform({-Model->Size.W/2, Model->Size.H/2, 0});
		Header->Update(Model->Size);
	}
}

CCamera * CSolo::GetPreferedCamera()
{
	auto c = GetActualView()->Cameras.Find([this](auto i){ return i->Viewport->Tags.Has([this](auto j){ return Model->Tags.Contains(j); }); });
	return c; 
}

CSize CSolo::Measure()
{
	return Model->Size;
}

void CSolo::AdjustTransformation(CTransformation & d)
{
	if(auto pa = Parent->As<CPositioningArea>())
	{
		Transform(pa->Positioning->Adjust(Transformation, d));
	}
	
	UpdateHeader();

	//auto p = (Transformation * CTransformation(Model->Size.W/2, Model->Size.H/2, 0)).Position;
	//
	
}

void CSolo::Activate(CArea * a, CViewport * vp, CPick & pick, CTransformation & origin)
{
	if(Model->Active->State != EActiveState::Active)
	{
		Level->ActiveGraph->Activate(Model->Active);
	}
}

void CSolo::Interact(bool e)
{
	Model->Active->IsPropagator = e;
}

bool CSolo::ContainsEntity(CUol & o)
{
	return Entity.Url == o;
}

bool CSolo::ContainsAvatar(CUol & o)
{
	return Model->Url == o;
}

CString & CSolo::GetDefaultInteractiveMasterTag()
{
	return Entity->DefaultInteractiveMasterTag;
}

void CSolo::Save(CXon * x)
{
	__super::Save(x);

	x->Add(L"Entity")->Set(Entity.Url);
	x->Add(L"Model")->Set(Model.Url);
}

void CSolo::Load(CXon * x)
{
	__super::Load(x);

	Entity.Url	= x->Get<CUol>(L"Entity");
	Model.Url	= x->Get<CUol>(L"Model");
}
