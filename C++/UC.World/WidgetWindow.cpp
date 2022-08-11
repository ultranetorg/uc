#include "stdafx.h"
#include "WidgetWindow.h"

using namespace uc;

CWidgetWindow::CWidgetWindow(CWorld * l, CServer * srv, CStyle * s, const CString & name) : CFieldableModel(l, srv, ELifespan::Permanent, name), Sizer(l)
{
	World = l;
	Style = s;

	Active->MouseEvent[EListen::NormalAll] += ThisHandler(OnMouse);
}

CWidgetWindow::~CWidgetWindow()
{
	if(Menu)
	{
		Menu->Close();
		Menu->Free();
	}

	RemoveNode(Face);
	Sizer.SetTarget(null);

	OnDependencyDestroying(Entity);
}

void CWidgetWindow::SetFace(CElement * e)
{
	Face = e;

	Face->Express(L"W", [this]{ return IW; });
	Face->Express(L"H", [this]{ return IH; });

	AddNode(e);
}

void CWidgetWindow::SetEntity(CUol & o)
{
	Entity = Server->FindObject(o);
	Entity->Destroying += ThisHandler(OnDependencyDestroying);
}

void CWidgetWindow::OnDependencyDestroying(CInterObject * o)
{
	if(Entity && o == Entity)
	{
		Entity->Destroying -= ThisHandler(OnDependencyDestroying);
		Entity.Clear();
	}
}

void CWidgetWindow::SaveInstance()
{
	CTonDocument d;

	d.Add(L"Size")->Set(Size);

	SaveGlobal(d, GetClassName() + L".xon");
}

void CWidgetWindow::LoadInstance()
{
	CTonDocument d;
	LoadGlobal(d, GetClassName() + L".xon");
	
	auto s = d.Get<CSize>(L"Size");
	Express(L"W", [s]{ return s.W; });
	Express(L"H", [s]{ return s.H; });
}

void CWidgetWindow::Place(CFieldWorld * f)
{
	__super::Place(f);

	Sizer.SetTarget(this);
	Sizer.SetGripper(Face);

	Sizer.InGripper =	[this](auto & is)
						{
							return Sizer.InRightBottomCorner(Face, is);
						};
	Sizer.Restricting =	[this](auto & c, auto & pk)
						{
							auto ps = Field->GetPositioning();
							auto p = ps->Move(c, pk).Position;
							return ps->Project(pk.Camera->Viewport, pk.Space, p).ToXY();
						};
	Sizer.Resizing =	[this](auto & r)
						{
							Express(L"W", [r]{ return r.Size.W; });
							Express(L"H", [r]{ return r.Size.H; });
							PropagateLayoutChanges(Face);
						};
	Sizer.Adjusting =	[this](CResizing & r)
						{
							Field->MoveAvatar(this, Parent->Transformation * CTransformation(r.PositionDelta.ReplaceX(0)));
						};
}

void CWidgetWindow::OnMouse(CActive *, CActive *, CMouseArgs * arg)
{
	if(arg->Control == EMouseControl::RightButton && arg->Event == EGraphEvent::Click)
	{
		if(!Menu)
		{
			Menu = new CRectangleMenu(World, Level->Style, L"Menu");
	
			if(Field)
			{
				Field->AddTitleMenu(Menu->Section, this);
				Menu->Section->AddSeparator();
			
				Menu->Section->AddItem(L"Delete")->Clicked = [this](auto, auto){ Field->DeleteAvatar(this); };
				Menu->Section->AddSeparator();
				Menu->Section->AddItem(L"Properties...");
			}
		}
		
		Menu->Open(arg->Pick);
	
		arg->StopPropagation = true;
	}

	if(arg->Control == EMouseControl::LeftButton && arg->Event == EGraphEvent::Click)
	{
		arg->StopPropagation = true;
	}
}

void CWidgetWindow::DetermineSize(CSize & smax, CSize & s)
{
	__super::DetermineSize(smax, s);
}
