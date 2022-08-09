#include "StdAfx.h"
#include "TrayWidget.h"

using namespace uc;

CTrayWidget::CTrayWidget(CShellLevel * l, CString const & name) : CFieldableModel(l->World, l->Server, ELifespan::Permanent, name)
{
	Level = l;

	Panel = new CRectangle(l->World, L"panel");
	Panel->Express(L"W", [this]{ return IW; });
	Panel->Express(L"H", [this]{ return IH; });
	
	Listbox = new CListbox(l->World, l->Style);
	Listbox->Express(L"W", [this]{ return Panel->IW; });
	Listbox->Express(L"H", [this]{ return Panel->IH; });
	Listbox->YAlign = EYAlign::Bottom;

	AddNode(Panel);
	Panel->AddNode(Listbox);
}
	
CTrayWidget::~CTrayWidget()
{
	if(Entity)
	{
		OnDependencyDestroying(Entity);
	}

	Panel->RemoveNode(Listbox);
	Listbox->Free();

	RemoveNode(Panel);
	Panel->Free();
}

void CTrayWidget::UpdateLayout(CLimits const & l, bool apply)
{
	__super::UpdateLayout(l, apply);

	if(Capabilities)
	{
		if(Capabilities->FullScreen)
		{
			Listbox->TransformY(Panel->IH/2);
		}
	}
}

void CTrayWidget::SetEntity(CUol & o)
{
	Entity = Server->FindObject(o);

	Entity->Destroying	+= ThisHandler(OnDependencyDestroying);
	Entity->Added		+= ThisHandler(OnItemAdded);
	Entity->Removed		+= ThisHandler(OnItemRemoved);

	for(auto i : Entity->Items)
	{
		OnItemAdded(i);
	}
}

void CTrayWidget::OnDependencyDestroying(CBaseNexusObject * o)
{
	if(Entity && o == Entity)
	{
		for(auto i : Listbox->Items)
		{
			i->As<CTrayItemElement>()->SetEnity(null);
		}

		Entity->Destroying	-= ThisHandler(OnDependencyDestroying);
		Entity->Added		-= ThisHandler(OnItemAdded);
		Entity->Removed		-= ThisHandler(OnItemRemoved);
		Entity.Clear();
	}
}

void CTrayWidget::OnItemAdded(CTrayItem * hi)
{
	auto e = new CTrayItemElement(Level);
	e->SetEnity(hi);
	e->Express(L"W", [e]{ return e->Slimits.Smax.W; });
	
	CCardMetrics m;
	m.TextSize.H = Level->Style->GetFont(L"Text/Font")->Height;
	e->Metrics = m;

	Listbox->AddFront(e);

	e->Free();
}

void CTrayWidget::OnItemRemoved(CTrayItem * hi)
{
	auto e = Listbox->Nodes.Find([hi](auto i){ return i->As<CTrayItemElement>()->Entity == hi; });
	
	Listbox->RemoveItem(e);
	Listbox->UpdateLayout();
}

void CTrayWidget::SaveInstance()
{
	CTonDocument d;

	d.Add(L"Size")->Set(Size);

	SaveGlobal(d, GetClassName() + L".xon");
}

void CTrayWidget::LoadInstance()	
{
	CTonDocument d;
	LoadGlobal(d, GetClassName() + L".xon");

	auto s = d.Get<CSize>(L"Size");
	Express(L"W", [s]{ return s.W; });
	Express(L"H", [s]{ return s.H; });
}

void CTrayWidget::Open(CWorldCapabilities * caps, CUnit * a)
{
	__super::Open(caps, a);

	if(caps->FullScreen)
	{
		Listbox->ApplyStyles(Level->Style, {L"Widget"});
		Listbox->Express(L"H", [this]{ return Listbox->Slimits.Smax.H/2; });
	}
}
