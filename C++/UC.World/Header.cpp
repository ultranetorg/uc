#include "stdafx.h"
#include "Header.h"
#include "WorldServer.h"
#include "AvatarCard.h"
#include "RectangleMenu.h"

using namespace uc;

CHeaderTab::CHeaderTab(CWorldServer * l, CUol & m, CModel * model)
{
	Level = l;
	Model.Url = m;
	Model.Object = model;

	auto a = l->GenerateAvatar(CUol(m.Parameters(L"entity")), AVATAR_ICON2D);
	Card = new CAvatarCard(l);
	Card->SetAvatar(a, L"");
	Card->SetEntity(CUol(m.Parameters(L"entity")));
	Card->ApplyStyles(Level->Style, {L"Header/Normal"});
}

CHeaderTab::~CHeaderTab()
{
	 Card->Free();
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

CHeader::CHeader(CWorldServer * l, CView * view) : CArea(l, GetClassName())
{
	View = view;

	Stack = new CStack(l, l->Style, L"header");
	Stack->Spacing = 10;
	Stack->Direction = EDirection::X;

	Stack->XAlign = EXAlign::Left;
	Stack->YAlign = EYAlign::Center;
	Stack->ApplyStyles(Level->Style, {L"Header"});
	Stack->Active->IsPropagator = false;

	Stack->Active->MouseEvent[EListen::NormalAll] +=	[this](auto r, CActive * s, auto arg)
														{
															auto icon = s->AncestorOwnerOf<CAvatarCard>();

															if(icon)
															{
																auto t = Tabs.Find([icon](auto i){ return i->Card == icon; });
		
																if(arg->Event == EGraphEvent::Click && arg->Control == EMouseControl::LeftButton)
																{
																	Selected(t->Model.Url, arg);
																}
	
																arg->StopPropagation = true;
															}
															else
															{
																if(arg->Event == EGraphEvent::Click && arg->Control == EMouseControl::RightButton)
																{
																	if(!Menu)
																	{
																		Menu = new CRectangleMenu(Level, Level->Style);
																		Menu->Section->AddItem(L"Hide")->Clicked = [this](auto, auto)	{ Level->Hide(Parent->As<CUnit>(), null); };
																		Menu->Section->AddItem(L"Push")->Clicked = [this](auto arg, auto){ Level->Show(Parent->As<CUnit>(), AREA_BACKGROUND, sh_new<CShowParameters>(arg, Level->Style)); };
																	}
		
																	Menu->Open(arg->Pick);
																}
															}
														};

	Stack->Active->TouchEvent[EListen::NormalAll] +=	[this](auto r, auto s, auto arg)
														{
															auto icon = s->AncestorOwnerOf<CAvatarCard>();

															if(icon)
															{
																auto t = Tabs.Find([icon](auto i){ return i->Card == icon; });
		
																if(arg->Event == EGraphEvent::Click)
																{
																	Selected(t->Model.Url, arg);
																}
	
																arg->StopPropagation = true;
															}
														};


	Level->ActiveGraph->Root->AddNode(Stack->Active);
}

CHeader::~CHeader()
{
	if(Menu)
	{
		Menu->Close();
		Menu->Free();
	}

	Level->ActiveGraph->Root->RemoveNode(Stack->Active);

	Stack->Free();

	for(auto i : Tabs)
	{
		delete i;
	}
}

void CHeader::Select(CUol & m)
{
	auto t = Tabs.Find([m](auto i){ return i->Model.Url == m; });

	for(auto i : Tabs)
	{
		if(i->Selected)
		{
			i->Card->ApplyStyles(Level->Style, {L"Header/Normal"});
		}
	}
	
	t->Selected = true;
	t->Card->ApplyStyles(Level->Style, {L"Header/Selected"});
	
	Update(Size);
}

CHeaderTab * CHeader::AddTab(CUol & m, CModel * model)
{
	auto t = new CHeaderTab(Level, m, model);

	Tabs.push_back(t);
	Stack->AddNode(t->Card);

	return t;
}

CSpaceBinding<CVisualSpace> & CHeader::AllocateVisualSpace(CViewport * vp)
{
	auto & s = __super::AllocateVisualSpace(vp);
	s.Space->AddVisual(Stack->Visual); // nodes to graphs
	return s;
}

CSpaceBinding<CActiveSpace> & CHeader::AllocateActiveSpace(CViewport * vp)
{
	auto & s = __super::AllocateActiveSpace(vp);
	s.Space->AddActive(Stack->Active); // nodes to graphs
	return s;
}

void CHeader::DeallocateVisualSpace(CVisualSpace * s)
{
	__super::DeallocateVisualSpace(s);
}

void CHeader::DeallocateActiveSpace(CActiveSpace * s)
{
	__super::DeallocateActiveSpace(s);
}

void CHeader::Update(CSize & s)
{
	Size = s;

	Stack->Express(L"W", [s]{ return s.W; });
	
	CCardMetrics am;
	am.FaceSize = IconSize;

	if(Stack->Direction == EDirection::X)	am.TextSize = {(s.W - IconSize.W * Tabs.size())/Tabs.size(), Level->Style->GetFont(L"Text/Font")->Height * 2, 0}; else
	if(Stack->Direction == EDirection::Y)	am.TextSize = {s.W - IconSize.W, IconSize.H, 0};

	for(auto i : Tabs)
	{
		i->Card->SetMetrics(am);
		i->Card->SetTitleMode(TitleMode);
	}

	Stack->UpdateLayout(CLimits::Max, true);
}
