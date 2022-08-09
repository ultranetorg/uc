#include "stdafx.h"
#include "HudEnvironment.h"
#include "MenuWidget.h"

using namespace uc;

CHudEnvironment::CHudEnvironment(CShellLevel * l, CString const & name) : CModel(l->World, l->Server, ELifespan::Permanent, name)
{
	Level = l;
	Tags = {L"Hud-Primary"};
	PreferedPlacement[AREA_HUD] = EPreferedPlacement::Default;

	FieldElement = new CHudFieldElement(Level);
	FieldElement->Express(L"W", [this]{ return Size.W; });
	FieldElement->Express(L"H", [this]{ return Size.H; });
	FieldElement->Transform(0, 0, Z_STEP);
	AddNode(FieldElement);
}

CHudEnvironment::~CHudEnvironment()
{
	OnDependencyDestroying(Entity);

	if(FieldElement)
	{
		RemoveNode(FieldElement);
		FieldElement->Free();
	}
}

void CHudEnvironment::SetEntity(CUol & hud)
{
	Entity = Server->FindObject(hud);
	Entity->Destroying += ThisHandler(OnDependencyDestroying);

	FieldElement->SetEntity(Entity);
}

void CHudEnvironment::OnDependencyDestroying(CBaseNexusObject * o)
{
	if(Entity && o == Entity) 
	{
		FieldElement->SetEntity(null);
		Entity->Destroying -= ThisHandler(OnDependencyDestroying);
		Entity.Clear();
	}
}

void CHudEnvironment::SetDirectories(CString const & dir)
{
	__super::SetDirectories(dir);
	FieldElement->SetDirectories(dir);
}

void CHudEnvironment::SaveInstance()
{
	__super::SaveInstance();
	FieldElement->Save();
}

void CHudEnvironment::LoadInstance()
{
	__super::LoadInstance();
}

void CHudEnvironment::DetermineSize(CSize & smax, CSize & s)
{
	///???? auto c = !Level->World->Layout.StartsWith(LAYOUT_DUO) ? Unit->GetActualView()->PrimaryCamera : Unit->GetActualView()->Cameras.back();
	Express(L"W", [smax]{ return smax.W; });
	Express(L"H", [smax]{ return smax.H; });
	UpdateLayout(CLimits::Empty, false);
}

CTransformation CHudEnvironment::DetermineTransformation(CPositioning * ps, CPick & pick, CTransformation & t)
{
	return t;
}
