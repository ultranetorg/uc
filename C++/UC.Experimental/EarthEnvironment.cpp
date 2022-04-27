#include "stdafx.h"
#include "EarthEnvironment.h"

using namespace uc;

CEarthEnvironment::CEarthEnvironment(CExperimentalLevel * l, CGeoStore * s, const CString & name) : CEnvironmentWindow(l->World, l->Server, l->Style, name)
{
	Level = l;

	Globe = new CGlobe(Level, s);
	Globe->Active->Index = 0;
	Globe->Express(L"W", [this]{ return W; });
	Globe->Express(L"H", [this]{ return H; });

	SetContent(Globe);
}

CEarthEnvironment::~CEarthEnvironment()
{
	Globe->Free();
}

void CEarthEnvironment::SetEntity(CUol & e)
{
	Entity.Url = e;
}

void CEarthEnvironment::SaveInstance()
{
	__super::SaveInstance();

	CTonDocument d;
	d.Add(L"Entity")->Set(Entity.Url);
	Globe->Save(&d);

	SaveGlobal(d, GetClassName() + L".xon");
}

void CEarthEnvironment::LoadInstance()
{
	__super::LoadInstance();

	CTonDocument d; LoadGlobal(d, GetClassName() + L".xon");

	SetEntity(d.Get<CUol>(L"Entity"));
	Globe->Load(&d);
}

void CEarthEnvironment::DetermineSize(CSize & smax, CSize & s)
{
	if(s)
	{
		Express(L"W", [s]{ return s.W; });
		Express(L"H", [s]{ return s.H; });
	}
	else
	{
		Express(L"W", [smax]{ return smax.H * 0.7f; });
		Express(L"H", [smax]{ return smax.H * 0.7f; });
	}
	UpdateLayout(CLimits::Empty, false);
}

void CEarthEnvironment::Open(CWorldCapabilities * caps, CUnit * a)
{
	__super::Open(caps, a);

	Globe->Unit = Unit;
}