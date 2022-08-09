#include "StdAfx.h"
#include "MenuWidget.h"

using namespace uc;

CMenuWidgetSectionItem::CMenuWidgetSectionItem(CWorld * w, CMenuWidget * mw, const CString & name) : CRectangleSectionMenuItem(w, w->Style, name)
{
	World = w;
	Widget = mw;

	Menu = new CRectangleMenu(w, w->Style);

	Section->Closing +=	[this](auto ms)
						{
							if(!ClosingByWidget)
							{
								Widget->Section->Unhighlight();
							}
						};

	Menu->SetSection(Section);
}

CMenuWidgetSectionItem::~CMenuWidgetSectionItem()
{
	if(Menu->Parent)
	{
		Menu->Close();
	}
	Menu->Free();
}

void CMenuWidgetSectionItem::Highlight(CArea * a, bool e, CSize & s, CPick * p) // it's only called by changing hightlighting - so if ClosingByWidget=false above then it means that the Menu being closed by itself
{
	CRectangleTextMenuItem::Highlight(a, e, s, p);
	HighlightArrow(e, s);

	if(e)
	{
		auto pk = p->Active->Graph->ReversePick(p->Camera, p->Space, Active, CFloat3(0));
		Menu->Open(pk, Size);
	}
	else
	{
		ClosingByWidget = true;
		Menu->Close();
		ClosingByWidget = false;
	}
}

/// **********************************************************************************
/// ********************************* MenuWidget *************************************
/// **********************************************************************************

CMenuWidget::CMenuWidget(CShellLevel * l, CString const & name) : CAvatar(l->World, l->Server, name)
{
	Level = l;

	Express(L"C",	[this](bool apply)
					{ 
						Section->UpdateLayout(Climits, apply);
						return Section->Size; 
					});

	Active->MouseEvent[EListen::NormalAll] += ThisHandler(OnMouse);
	Active->StateChanged += ThisHandler(OnStateModified);
		
	auto e = new CMenuItem(GetClassName());
	e->Meta = new CMergeMeta();

	Section = new CRectangleMenuSection(Level->World, Level->Style);
	Section->Active->SetMeta(e);
	Section->Open(this);
	Section->Express(L"IW", [this]{ return min(Section->C.W, 200); });
	Section->Express(L"B", [this]{ return CFloat6(0); });
	Section->Visual->SetMaterial(null);

	e->Free();
}

CMenuWidget::~CMenuWidget()
{
	if(Menu)
	{
		Menu->Close();
		Menu->Free();
	}

	Section->Close();
	Section->Free();

	OnDependencyDestroying(Entity);

	Active->StateChanged -= ThisHandler(OnStateModified);
}

void CMenuWidget::SetEntity(CUol & e)
{
	Entity = Server->FindObject(e);
	Entity->Destroying += ThisHandler(OnDependencyDestroying);

	auto mm = Section->Active->GetMetaPointer<CMenuItem>()->MetaAs<CMergeMeta>();

	for(auto i : Entity->Paths)
	{
		mm->Sources.push_back(i);
	}
	
	for(auto i : Level->LoadLinks(mm->Sources))
	{
		AddMenuWidgetSectionItem(i);
	}
}

void CMenuWidget::AddMenuWidgetSectionItem(CMenuItem * i)
{
	if(i->Execute)
	{
		Level->AddMenuItem(Section, i);
	} 
	else
	{
		auto si = new CMenuWidgetSectionItem(Level->World, this, i->Label);
					
		// for 200 px see also CShellLevel::AddMenuItem
		auto ms = dynamic_cast<CRectangleMenuSection *>(si->Section);
		ms->Express(L"IW", [ms](){ return min(ms->C.W, 200); });
							
		si->Section->Opening +=	[this, i, si](auto)
								{
									if(i->Opening)
									{
										i->Opening();
									}
												
									si->Section->Clear();
	
									for(auto j : i->Items)
									{
										Level->AddMenuItem(si->Section, j);
									}
								};
	
		if(!i->IconEntity.IsEmpty())
		{
			Level->ImageExtractor->GetIconMaterial(this, (CUrl)i->IconEntity, 16)->Done = [si](auto m){  si->Icon->Visual->SetMaterial(m);  }; 
		}
	
		si->Active->SetMeta(i);
		Section->AddItem(si);
		si->Free();
	}
}

CMenuWidgetSectionItem * CMenuWidget::AddMenuWidgetSectionItem(CString const & lable)
{
	auto si = new CMenuWidgetSectionItem(Level->World, this, lable);
				
	// for 200 px see also CShellLevel::AddMenuItem
	si->Section->Express(L"IW", [si](){ return min(si->Section->C.W, 200); });
						

	//si->Active->SetMeta(i);
	Section->AddItem(si);
	si->Free();

	return si;
}

void CMenuWidget::OnDependencyDestroying(CBaseNexusObject * o)
{
	if(Entity && o == Entity)
	{
		Entity->Destroying -= ThisHandler(OnDependencyDestroying);
		Entity.Clear();
	}
}

void CMenuWidget::SaveInstance()
{
}

void CMenuWidget::LoadInstance()
{
}

void CMenuWidget::OnStateModified(CActive * r, CActive * s, CActiveStateArgs * a)
{
	//if(s == Active)
	//{
	//	if(a.Old == EActiveState::Active)
	//	{
	//		auto ss =  dynamic_cast<CRectangleMenuSection *>(Section);
	//
	//		if(ss->Highlighted)
	//		{
	//			ss->Highlighted->Highlight(null, false, CSize(), null);
	//			shared_free(ss->Highlighted);
	//		}
	//	}
	//}
}

void CMenuWidget::OnMouse(CActive *, CActive * s, CMouseArgs * arg)
{
	if(Menu && s->HasAncestor(Menu->Active))
		return;

	if(arg->Event == EGraphEvent::Click)
	{
		auto mi = s->AncestorOwnerOf<CRectangleMenuItem>();
	
		if(arg->Control == EMouseControl::RightButton)
		{
			if(!Menu)
			{
				Menu = new CRectangleMenu(Level->World, Level->Style);
			}

			Menu->Section->Clear();
			Menu->Section->AddItem(L"Delete")->Clicked = [this](auto a, auto ){  Field->DeleteAvatar(this);	};

			Menu->Open(arg->Pick);

			arg->StopPropagation = true;
		}

		if(arg->Control == EMouseControl::LeftButton || arg->Control == EMouseControl::MiddleButton)
		{
			if(mi) // leaf item
			{
				if(mi->Clicked)
				{
					mi->Clicked(arg, mi);
				}
			}

			arg->StopPropagation = true;
		}
	}
}

IMenuSection * CMenuWidget::CreateSection(const CString & name)
{
	auto s = new CRectangleMenuSection(Level->World, Level->Style, name);
	s->Express(L"B", []{ return CFloat6(1.f); });
	return s;
}

void CMenuWidget::DetermineSize(CSize & smax, CSize & s)
{
	if(!World->FullScreen)
	{
		Express(L"IW", [this]{ return C.W; });
		Express(L"IH", [this]{ return C.H; });
	} 
	else
	{
		Express(L"W", [smax]{ return smax.W; });
		Express(L"H", [smax]{ return smax.H; });
	}
	UpdateLayout(CLimits::Empty, false);	
}
