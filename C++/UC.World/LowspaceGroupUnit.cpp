#include "stdafx.h"
#include "LowspaceGroupUnit.h"
#include "WorldServer.h"

using namespace uc;

CLowspaceGroupUnit::CLowspaceGroupUnit(CWorldServer * l, CString & dir, CUol & entity, CString const & type, CView * hview) : CGroupUnit(l, dir, entity, type, hview)
{
	Initialize();
}

CLowspaceGroupUnit::CLowspaceGroupUnit(CWorldServer * l, CString & dir, CString const & name, CView * hview) : CGroupUnit(l, dir, name, hview)
{
	Initialize();
	Load();
}

CLowspaceGroupUnit::~CLowspaceGroupUnit()
{
	if(Header->Parent)
		Remove(Header);
}

void CLowspaceGroupUnit::Initialize()
{
	Header->TitleMode			= ECardTitleMode::Right;
	Header->IconSize			= {48, 48, 0};
	Header->Stack->Direction	= EDirection::Y;
	Header->Stack->XAlign		= EXAlign::Left;
	Header->Stack->YAlign		= EYAlign::Center;
	Header->Stack->Spacing		= 30;
	Header->Stack->Express(L"W", [this]{ return Size.W; });
	Header->Stack->Express(L"H", [this]{ return Size.H; });
	Header->Stack->Express(L"P", [this]{ return CFloat6(50); });
	Header->Stack->Visual->SetMaterial(Level->Materials->GetMaterial(L"0 0 0"));

	Header->Selected += [this](auto m, auto arg){ OpenModel(m, arg); };
}

void CLowspaceGroupUnit::Open()
{
	if(!Header->Parent)
		Add(Header, EAddLocation::Front);

	for(auto i : Header->Tabs)
	{
		if(i->Shown)
		{
			CloseModel(i->Model.Url);
		}
	}
}

void CLowspaceGroupUnit::OpenModel(CUol & m, CInputArgs * arg)
{
	Remove(Header);

	auto t = Header->Tabs.Find([&m](auto i){ return i->Model.Url == m; });

	if(!t->Model)
	{
		auto dir = CPath::Join(Directory, CNativePath::GetSafe(CUol(t->Model.Url.Parameters(L"entity")).Object));

		t->Model = Level->CreateAvatar(m, dir)->As<CModel>();
		t->Model->Unit = this;
		t->Model->Open(Level, this);
		//t->Model->Destroying += [](auto i){};

		if(!t->Model->Active->Parent)
			Level->ActiveGraph->Root->AddNode(t->Model->Active);

		if(Size.W > 0 && Size.H > 0)
		{
			t->Model->DetermineSize(Size, t->Size ? t->Size : Size);
			t->Model->UpdateLayout();
		}
	}

	for(auto i : VisualSpaces)
		i.Space->AddVisual(t->Model->Visual);

	for(auto i : ActiveSpaces)
		i.Space->AddActive(t->Model->Active);

	t->Shown = true;
}

CSize CLowspaceGroupUnit::DetermineSize(CSize & smax, CSize & s)
{
	Size = smax;

	return smax;
}

void CLowspaceGroupUnit::Update()
{
	for(auto i : Header->Tabs)
		if(i->Shown)
			i->Model->UpdateLayout();

	Header->Update(Size);
	Header->Transform({-Size.W/2, -Size.H/2, 0});
}

void CLowspaceGroupUnit::Interact(bool e)
{
	Header->Stack->Active->IsPropagator = e;
}

void CLowspaceGroupUnit::Activate(CArea * a, CViewport * vp, CPick & pick, CTransformation & origin)
{
	Level->ActiveGraph->Activate(Header->Stack->Active);
}

void CLowspaceGroupUnit::AdjustTransformation(CTransformation & t)
{
}

void CLowspaceGroupUnit::Save(CXon * x)
{
	__super::Save(x);

	x->Add(L"Entity")->Set(Entity.Url);

	for(auto i : Header->Tabs)
	{
		auto m = x->Add(L"Model");
		m->Set(i->Model.Url);
		//m->Add(L"Size")->Set(i->Model ? i->Model->Size : i->Size);
	}
}

void CLowspaceGroupUnit::Load(CXon * x)
{
	__super::Load(x);

	Entity = Level->FindObject(x->Get<CUol>(L"Entity"))->As<CGroup>();
		
	for(auto i : x->Many(L"Model"))
	{
		auto t = AddModel(i->Get<CUol>(), null);
		//t->Size = i->Get<CSize>(L"Size");
	}
}
