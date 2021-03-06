#include "stdafx.h"
#include "GroupUnit.h"
#include "WorldServer.h"

using namespace uc;

CGroupUnit::CGroupUnit(CWorldServer * l, CString & dir, CUol & entity, CString const & type, CView * hview) : CUnit(l, hview, entity.Object)
{
	Directory = dir;
	View = hview;
	Tags = {L"Apps"};
	Lifespan = ELifespan::Permanent;

	Header = new CHeader(Level, HeaderView);
	Header->Tags = {L"all"};

	Entity = l->FindObject(entity)->As<CGroup>();

	for(auto i : Entity->As<CGroup>()->Entities)
	{
		AddModel(Level->GenerateAvatar(i.Url, Level->Complexity), null);
	}
}

CGroupUnit::CGroupUnit(CWorldServer * l, CString & dir, CString const & name, CView * hview) : CUnit(l, hview, name)
{
	Header = new CHeader(Level, HeaderView);
	Header->Tags = {L"all"};

	Directory = dir;
	View = hview;
	Tags = {L"Apps"};
	Lifespan = ELifespan::Permanent;
}

CGroupUnit::~CGroupUnit()
{
	for(auto i : Header->Tabs)
	{
		if(i->Model)
		{
			if(i->Shown)
				i->Model->Close(this);	

			if(i->Model->Active->Parent == Level->ActiveGraph->Root)
				Level->ActiveGraph->Root->RemoveNode(i->Model->Active);
	
			i->Model->Save();
		
			Level->DestroyAvatar(i->Model);
		}
	}

	Header->Free();
	Header = null;
}

CHeaderTab * CGroupUnit::AddModel(CUol & m, CModel * model)
{
	return Header->AddTab(m, model);
}

void CGroupUnit::OpenModel(CUol & m, CInputArgs * arg)
{
	if(auto t = Header->Tabs.Find([](auto i){ return i->Shown; }))
	{
		CloseModel(t->Model.Url);
	}
	
	auto t = Header->Tabs.Find([&m](auto i){ return i->Model.Url == m; });

	if(!t->Model)
	{
		auto dir = CPath::Join(Directory, CNativePath::GetSafe(CUol(t->Model.Url.Parameters(L"entity")).Object));

		t->Model = Level->CreateAvatar(m, dir)->As<CModel>();
		t->Model->Unit = this;
		
		//t->Model->Destroying += [](auto i){};

		if(!t->Model->Active->Parent)
			Level->ActiveGraph->Root->AddNode(t->Model->Active);

		if(t->Model->Active->State != EActiveState::Active)
		{
			Level->ActiveGraph->Activate(t->Model->Active);
		}

		if(Size.W > 0 && Size.H > 0)
		{
			t->Model->DetermineSize(Size, t->Size ? t->Size : Size);
		}
	}

	for(auto i : VisualSpaces)
	{
		i.Space->AddVisual(t->Model->Visual);
	}

	for(auto i : ActiveSpaces)
	{
		i.Space->AddActive(t->Model->Active);
	}

	t->Model->Open(Level, this);
	t->Shown = true;
	t->Model->UpdateLayout();

	Header->Select(m);

	auto at = Transformation;

	if(!Level->Free3D)
	{
		at.Position.x += (t->Model->Size.W/2 + Header->Transformation.Position.x);
	}

	at.Position.y -= (t->Model->Size.H/2 - Header->Transformation.Position.y);
	Transform(at);

	Header->Transform({-t->Model->Size.W/2, t->Model->Size.H/2, 0});

	if(t->Model->Size)
	{
		Size = t->Model->Size;
		Header->Update(Size);
	}
}

void CGroupUnit::CloseModel(CUol & m)
{
	auto t = Header->Tabs.Find([&m](auto i){ return i->Model.Url == m; });

	t->Model->Close(this);

	for(auto i : VisualSpaces)
	{
		i.Space->RemoveVisual(t->Model->Visual);
	}

	for(auto i : ActiveSpaces)
	{
		i.Space->RemoveActive(t->Model->Active);
	}

	t->Shown = false;
}

CSpaceBinding<CVisualSpace> & CGroupUnit::AllocateVisualSpace(CViewport * vp)
{
	auto & s = __super::AllocateVisualSpace(vp);
	
	for(auto i : Header->Tabs)
	{
		if(i->Shown)
		{
			s.Space->AddVisual(i->Model->Visual);
		}
	}
	return s;
}

void CGroupUnit::DeallocateVisualSpace(CVisualSpace * s)
{
	for(auto i : Header->Tabs)
	{
		if(i->Shown)
		{
			s->RemoveVisual(i->Model->Visual);
		}
	}
	__super::DeallocateVisualSpace(s);
}

CSpaceBinding<CActiveSpace> & CGroupUnit::AllocateActiveSpace(CViewport * vp)
{
	auto & s = __super::AllocateActiveSpace(vp);
	
	for(auto i : Header->Tabs)
	{
		if(i->Shown)
		{
			s.Space->AddActive(i->Model->Active);
		}
	}
	return s;
}

void CGroupUnit::DeallocateActiveSpace(CActiveSpace * s)
{
	for(auto i : Header->Tabs)
	{
		if(i->Shown)
		{
			s->RemoveActive(i->Model->Active);
		}
	}
	__super::DeallocateActiveSpace(s);
}

CSize CGroupUnit::DetermineSize(CSize & smax, CSize & s)
{
	Size = CSize::Empty;

	for(auto i : Header->Tabs)
		if(i->Shown)
		{
			i->Model->DetermineSize(smax, i->Size);
			Size = i->Model->Size;
		}
	
	Header->Update(Size);

	return Size;
}

CTransformation CGroupUnit::DetermineTransformation(CPositioning * ps, CPick & pk, CTransformation & t)
{
	return t;
}

EPreferedPlacement CGroupUnit::GetPreferedPlacement()
{
	return EPreferedPlacement::Default;
}

void CGroupUnit::Update()
{
	for(auto i : Header->Tabs)
		if(i->Shown)
			i->Model->UpdateLayout();
	
	Header->Transform({-Size.W/2, Size.H/2, 0});
	Header->Update(Size);
}

CCamera * CGroupUnit::GetPreferedCamera()
{
	return null;
}

CSize CGroupUnit::Measure()
{
	return Size;
}

void CGroupUnit::AdjustTransformation(CTransformation & d)
{
	if(auto pa = Parent->As<CPositioningArea>())
	{
		Transform(pa->Positioning->Adjust(Transformation, d));
	}
	
	for(auto i : Header->Tabs)
		if(i->Shown)
			Size = i->Model->Size;

	Header->Transform({-Size.W/2, Size.H/2, 0});
	Header->Update(Size);
}

void CGroupUnit::Activate(CArea * a, CViewport * vp, CPick & pick, CTransformation & origin)
{
	auto t = Header->Tabs.Find([](auto i){ return i->Shown; });

	if(t->Model->Active->State != EActiveState::Active)
	{
		Level->ActiveGraph->Activate(t->Model->Active);
	}
}

void CGroupUnit::Interact(bool e)
{
	auto t = Header->Tabs.Find([](auto i){ return i->Shown; });
	t->Model->Active->IsPropagator = e;
}

bool CGroupUnit::ContainsEntity(CUol & o)
{
	return Entity.Url == o || Header->Tabs.Has([&o](auto i){ return CUol(i->Model.Url.Parameters(L"entity")) == o; });
}

bool CGroupUnit::ContainsAvatar(CUol & o)
{
	return Header->Tabs.Has([&o](auto i){ return i->Model.Url == o; });
}

CString & CGroupUnit::GetDefaultInteractiveMasterTag()
{
	return Entity->DefaultInteractiveMasterTag;
}
