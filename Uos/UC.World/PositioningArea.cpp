#include "stdafx.h"
#include "PositioningArea.h"

using namespace uc;

CPositioningArea::CPositioningArea(CWorldServer * l, CString const & name) : CArea(l, name)
{
	PlaceNewDefault =	[this](auto c, auto pp, auto & pick, auto & size)
						{
							return CTransformation::Identity;
						};
	PlaceOldDefault =	[this](auto pick, auto size, auto t)
						{
							return t;
						};
}

CPositioningArea::~CPositioningArea()
{
	if(Positioning)
	{
		Positioning->Free();
	}
}

void CPositioningArea::Open(CArea * a, EAddLocation l, CViewport *, CPick & pick, CTransformation & origin)
{
	auto u = a->As<CUnit>();
	u->Open();

	auto newp = Add(a, l);

	auto t = CTransformation::Identity;

	auto c = u->GetPreferedCamera();

	if(!c)
	{
		c = pick.Camera ? u->GetActualView()->GetCamera(pick.Camera->Viewport) : u->GetActualView()->PrimaryCamera;
	}

	if(!a->SizeDetermined)
	{
		a->Size = u->DetermineSize(u->GetActualMaxSize(c->Viewport), a->Size);
		a->SizeDetermined = true;
	}

	auto pm = FindPlacement(a);

	if(newp)
	{
		t = PlaceNew(c, u->GetPreferedPlacement(), pick, a->Size);
	}
	else
	{
		t = PlaceOld(pick, a->Size, pm->Transformation);
	}

	t = u->DetermineTransformation(Positioning, pick, t);
	u->Transform(t);
	u->Update();
	u->Interact(Interactive);
}

void CPositioningArea::Close(CArea * a)
{
	a->As<CUnit>()->Close();
	Remove(a);
}

void CPositioningArea::Activate(CArea * a, CViewport * vp, CPick & pick, CTransformation & origin)
{
	auto u = a->As<CUnit>();

	if(Areas.front()->Area != a)
	{
		auto t = a->Transformation;

		Remove(a);
		Add(a, EAddLocation::Front);

		a->Transform(t);
	}

	u->Activate(a, vp, pick, origin);
}

void CPositioningArea::SetPositioning(CPolygonalPositioning * positioning)
{
	Positioning = positioning;
	Positioning->Take();

	auto polygonal = Positioning->As<CPolygonalPositioning>();
	
	PlaceOldDefault =	[this](auto & pk, auto & s, auto & old)
						{
							return old;
						};
}

void CPositioningArea::SetPositioning(CCylindricalPositioning * positioning, bool copo)
{
	Positioning = positioning;
	Positioning->Take();

	auto cylindrical = positioning->As<CCylindricalPositioning>();

	PlaceOldDefault =	[this, cylindrical, copo](auto & pk, auto & s, auto & old)
						{
							if(copo && pk.Camera)
							{
								auto m = cylindrical->GetMatrix(pk.Camera->Viewport);
								auto p = cylindrical->GetPoint(cylindrical->GetView()->GetCamera(pk.Camera->Viewport)->Raycast().Transform(!m));

								return CMatrix::FromTransformation({0, 0, cylindrical->R}, {0, CFloat3(0, 0, 1).GetAngleInXZ(p), 0}, {0, 0, -cylindrical->R}, CFloat3(1)).Decompose();
							}
							else
							{
								return old;
							}
						};
}

CTransformation CPositioningArea::PlaceNew(CCamera * c, EPreferedPlacement pp, CPick & pick, CSize & size)
{
	CTransformation t;

	if(pp == EPreferedPlacement::Exact)			t = PlaceNewExact(c, pp, pick, size); else
	if(pp == EPreferedPlacement::Convenient)	t = PlaceNewConvenient(c, pp, pick, size); else
												t = PlaceNewDefault(c, pp, pick, size);

	return Positioning ? t * Positioning->GetMatrix(c->Viewport).Decompose() : t;
}

CTransformation CPositioningArea::PlaceOld(CPick & pick, CSize & size, CTransformation & t)
{
	return PlaceOldDefault(pick, size, t);
}

CPositioningArea * CPositioningArea::GetPositioningAncestor()
{
	CArea * p = this;

	while(p && (!p->As<CPositioningArea>() || !p->As<CPositioningArea>()->Positioning))
	{
		p = p->Parent;
	}

	return p?p->As<CPositioningArea>():null;
}