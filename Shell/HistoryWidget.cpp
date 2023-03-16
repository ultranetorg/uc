#include "StdAfx.h"
#include "HistoryWidget.h"

using namespace uc;

CHistoryWidget::CHistoryWidget(CShellLevel * l, CString const & name) : CFieldableModel(l->World, l->Server, ELifespan::Permanent, name)
{
	Level = l;

	Listbox = new CListbox(l->World, l->Style);
	Listbox->Express(L"W", [this]{ return IW; });
	Listbox->Express(L"H", [this]{ return IH; });
	AddNode(Listbox);
}
	
CHistoryWidget::~CHistoryWidget()
{
	OnDependencyDestroying(Entity);

	RemoveNode(Listbox);
	Listbox->Free();
}

CHistoryItemElement * CHistoryWidget::CreateItem(CHistoryItem * hi)
{
	// copy-paste
	auto e = new CHistoryItemElement(Level);
	e->SetEnity(hi);
	e->SetTitleMode(ECardTitleMode::Right);
	e->UseClipping(EClipping::Apply, false);
	e->Express(L"W", [e]{ return e->Slimits.Smax.W; });

	if(Capabilities)
	{
		Adapt(e);
	}

	return e;
}

void CHistoryWidget::SetEntity(CUol & o)
{
	Entity = Server->FindObject(o);
	Entity->Destroying += ThisHandler(OnDependencyDestroying);
	Entity->Added += ThisHandler(OnItemAdded);
	Entity->Opened += ThisHandler(OnItemOpened);

	for(auto i : Entity->Items)
	{
		auto h = CreateItem(i);
		Listbox->AddItem(h, Listbox->Items.end());
		h->Free();
	}
}

void CHistoryWidget::OnDependencyDestroying(CInterObject * o)
{
	if(Entity && o == Entity)
	{
		///Save();

		for(auto i : Listbox->Items)
		{
			i->As<CHistoryItemElement>()->SetEnity(null);
		}

		Entity->Destroying -= ThisHandler(OnDependencyDestroying);
		Entity->Added -= ThisHandler(OnItemAdded);
		Entity->Opened -= ThisHandler(OnItemOpened);
		Entity.Clear();
	}
}

void CHistoryWidget::OnItemAdded(CHistoryItem * hi)
{
	AddHistory(hi);
}

void CHistoryWidget::OnItemOpened(CHistoryItem * hi)
{
	AddHistory(hi);
}
	
CHistoryItemElement * CHistoryWidget::AddHistory(CHistoryItem * hi)
{
	auto h = Listbox->Items.Find<CHistoryItemElement *>([hi](auto i){ return i->Entity->Object.Url == hi->Object.Url; });

	if(h == null)
	{
		h = CreateItem(hi);
		h->Transform(0.f, Size.H, Z_STEP);
	} 
	else
	{
		h->Take();
		Listbox->RemoveItem(h);
	}
	
	h->SetRecent();
	
	Listbox->AddItem(h, Listbox->Items.begin());

	h->Free();

	return h;
}

void CHistoryWidget::RemoveHistory(CHistoryItemElement * h)
{
	//Items.Remove(h); h->Free();
	Listbox->RemoveItem(h);
	Listbox->UpdateLayout();
}

void CHistoryWidget::OnNodeMouseButton(void *, CActive * r, CActive * s, CMouseArgs * a)
{
}

void CHistoryWidget::SaveInstance()
{
	CTonDocument d;

	d.Add(L"Size")->Set(Size);

	//Level->Storage->CreateGlobalDirectory(this);
	SaveGlobal(d, GetClassName() + L".xon");
}

void CHistoryWidget::LoadInstance()	
{
	CTonDocument d;
	LoadGlobal(d, GetClassName() + L".xon");

	auto s = d.Get<CSize>(L"Size");
	Express(L"W", [s]{ return s.W; });
	Express(L"H", [s]{ return s.H; });
}

void CHistoryWidget::Open(CWorldCapabilities * caps, CUnit * a)
{
	__super::Open(caps, a);

	Listbox->YAlign = EYAlign::Top;
	Listbox->Express(L"P", [this]{ return CFloat6(20.f); });
	Listbox->Visual->SetMaterial(Level->World->Materials->GetMaterial(L"0 0 0"));

	for(auto i : Listbox->Items)
	{
		Adapt(i->As<CHistoryItemElement>());
	}
}

void CHistoryWidget::Place(CFieldWorld * f)
{
	__super::Place(f);

	Listbox->YAlign = EYAlign::Bottom;
	Listbox->Animation = CAnimation(Level->Style->Get<CFloat>(L"Animation/Time"));

	for(auto i : Listbox->Items)
	{
		Adapt(i->As<CHistoryItemElement>());
	}
}

void CHistoryWidget::Adapt(CHistoryItemElement * e)
{
	if(!Capabilities->FullScreen)
	{
		e->Express(L"IH", [e]{ return e->Text->Font->Height; });
	}
	else
	{
		e->SetAvatar(Level->World->GenerateAvatar(e->Entity->Object.Url, AVATAR_ICON2D), L"");
		CCardMetrics m;
		m.FaceSize = {48, 48, 0};
		m.TextSize = {48 * 3, 48, 0};
		m.FaceMargin = CFloat6(0);
		e->Spacing = 20;
		e->SetMetrics(m);
		e->Express(L"M", []{ return CFloat6(10.f); });
	}
}