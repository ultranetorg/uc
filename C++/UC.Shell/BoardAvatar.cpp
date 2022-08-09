#include "StdAfx.h"
#include "BoardAvatar.h"
#include "MenuWidget.h"

using namespace uc;

CBoardAvatar::CBoardAvatar(CShellLevel * l, CString const & name) : CFieldableModel(l->World, l->Server, ELifespan::Permanent, name)
{
	Level = l;

	FieldElement = new CFieldElement(l);
	FieldElement->DefaultAvatarType	= AVATAR_ICON2D;
	FieldElement->Added		+= ThisHandler(OnItemAdded);
	FieldElement->Removed	+= ThisHandler(OnItemRemoved);
	FieldElement->Placing	+= ThisHandler(OnItemPlacing);
	FieldElement->Express(L"W", [this]{ return FieldElement->Slimits.Smax.W; });
	FieldElement->Express(L"H", [this]{ return FieldElement->Slimits.Smax.H; });
	AddNode(FieldElement);
			
	Surface = new CBoardSurface(l);
	FieldElement->SetSurface(Surface);

	Surface->Active->MouseEvent[EListen::Normal] += ThisHandler(OnSurfaceToggleEvent);
}
	
CBoardAvatar::~CBoardAvatar()
{
	if(ContextMenu)
	{
		ContextMenu->Close();
		ContextMenu->Free();
	}

	Surface->Active->MouseEvent -= ThisHandler(OnSurfaceToggleEvent);

	OnDependencyDestroying(Entity);

	FieldElement->SetSurface(null);
	RemoveNode(FieldElement);
	FieldElement->Free();

	Surface->Free();
}

void CBoardAvatar::SetDirectories(CString const & dir)
{
	__super::SetDirectories(dir);
	FieldElement->SetDirectories(dir);
}

void CBoardAvatar::SetEntity(CUol & o)
{	
	Entity = Server->FindObject(o);

	if(Entity)
	{
		//Entity->AddGlobalReference(Url);
		Entity->Destroying += ThisHandler(OnDependencyDestroying);

		FieldElement->SetField(Entity);
		FieldElement->Load();
		//FieldElement->Revive();
	}
}

void CBoardAvatar::OnDependencyDestroying(CBaseNexusObject * o)
{
	if(Entity && o == Entity) 
	{
		FieldElement->SetField(null);
		///SaveInstance();
		Entity->Destroying -= ThisHandler(OnDependencyDestroying);
		Entity.Clear();
	}
}

void CBoardAvatar::SaveInstance()
{
	CTonDocument d;

	d.Add(L"Size")->Set(Size);

	SaveGlobal(d, GetClassName() + L".xon");

	FieldElement->Save();
}

void CBoardAvatar::LoadInstance()
{
	CTonDocument d;
	LoadGlobal(d, GetClassName() + L".xon");
	
	auto s = d.Get<CSize>(L"Size");
	Express(L"W", [s]{ return s.W; });
	Express(L"H", [s]{ return s.H; });
}

void CBoardAvatar::OnItemPlacing(CFieldItemElement * fie)
{
	auto p = FieldElement->GetRandomFreePosition(fie);

	fie->Transform(p); // need final pos to corectly calculate intersections with others
				
	CAnimated<CTransformation> an(CTransformation(p.Position.x - fie->W, p.Position.y - fie->H, p.Position.z, 0, 0, 0, 0, 3, 3, 3), p, CAnimation(0.3f));
	Level->World->RunAnimation(fie, an);
}

void CBoardAvatar::OnItemMouse(CActive * r, CActive * s, CMouseArgs * arg)
{ 
	if(arg->Control == EMouseControl::MiddleButton && arg->Action == EMouseAction::DoubleClick)
	{
		auto fi = FieldElement->FindItem(s);
		FieldElement->DeleteItem(fi);
	}
}

void CBoardAvatar::OnItemAdded(CFieldItemElement * fie)
{
	fie->Active->MouseEvent[EListen::NormalAll] += ThisHandler(OnItemMouse);
}

void CBoardAvatar::OnItemRemoved(CFieldItemElement * fie)
{
	fie->Active->MouseEvent -= ThisHandler(OnItemMouse);
}

void CBoardAvatar::OnSurfaceToggleEvent(CActive * r, CActive * s, CMouseArgs * arg)
{
	if(arg->Control == EMouseControl::RightButton && arg->Event == EGraphEvent::Click)
	{
		//if(s == Surface->Active)
		{
			auto p = arg->Pick.Active->GetXyPlaneIntersectionPoint(arg->Pick.Space->Matrix, arg->Pick.Camera, arg->Pick.Vpp);
			p.z = 0;

			if(ContextMenu == null)
			{
				ContextMenu = new CRectangleMenu(Level->World, Level->Style, GetInstanceName() + L"Menu");
			}

			ContextMenu->Section->Clear();

			Level->AddSystemMenuItems(ContextMenu->Section);

			ContextMenu->Open(arg->Pick);
		}

		arg->StopPropagation = true;
	}
}
