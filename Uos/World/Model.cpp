#include "stdafx.h"
#include "Model.h"
#include "World.h"

using namespace uc;

CModel::CModel(CWorldProtocol * l, CServer * s, ELifespan life, CString const & name) : CAvatar(l, s, name), Lifespan(life)
{
}

CModel::~CModel()
{
}

void CModel::Open(CWorldCapabilities * caps, CUnit * a)
{
	Capabilities = caps;
	Unit = a;
}

void CModel::Close(CUnit * a)
{
	Unit = null;
}

EPreferedPlacement CModel::GetPreferedPlacement()
{
	CArea * a = Unit;

	while(a)
	{
		if(PreferedPlacement.Contains(a->Name))
		{
			return PreferedPlacement(a->Name);
		}

		a = a->Parent;
	}

	if(PreferedPlacement.Contains(L""))
	{
		return PreferedPlacement(L"");
	}

	return EPreferedPlacement::Null;
}

CTransformation CModel::DetermineTransformation(CPositioning * ps, CPick & pk, CTransformation & t)
{
	return t;
}

void CModel::DetermineSize(CSize & smax, CSize & s)
{
	if(!Capabilities->FullScreen)
	{
		if(!Size)
		{
			if(s)
			{
				Express(L"W", [s]{ return s.W; });
				Express(L"H", [s]{ return s.H; });
			}
			else
			{
				Express(L"W", [smax]{ return smax.W; });
				Express(L"H", [smax]{ return smax.H; });
			}
		}
	} 
	else
	{
		Express(L"W", [smax]{ return smax.W; });
		Express(L"H", [smax]{ return smax.H; });
	}

	UpdateLayout(CLimits::Empty, false);
}

void CModel::UpdateLayout(CLimits const & l, bool apply)
{
	__super::UpdateLayout(l, apply);

	Transform(-W/2, -H/2, 0);
}

