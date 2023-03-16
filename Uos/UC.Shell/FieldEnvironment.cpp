#include "stdafx.h"
#include "FieldEnvironment.h"

using namespace uc;

CFieldEnvironmentServer::CFieldEnvironmentServer(CShellLevel * l, const CString & name) : CFieldAvatar(l, name), Sizer(l->World)
{
	PreferedPlacement[CArea::Fields] = EPreferedPlacement::Default;

	Sizer.SetTarget(Surface);
	Sizer.InGripper =	[this](auto & pk)
						{
							return Sizer.InRightBottomCorner(Surface, pk);
						};
	Sizer.Resizing =	[this](auto & r)
						{
							Size.W = r.Size.W;
							Size.H = r.Size.H;
			
							for(auto i : FieldElement->Items)
							{
								i->Transform(i->Transformation.Position.x, i->Transformation.Position.y - r.PositionDelta.y);
							}

							UpdateLayout();
							Transform(Transformation * CTransformation(r.PositionDelta));
						};

	Sizer.Resized =	[this]()
					{
						ProcessLayoutChanges(this);
					};

}
