#pragma once
#include "BoardAvatar.h"

namespace uc
{
	class CBoardWidget : public CBoardAvatar
	{
		public:
			UOS_RTTI
			CBoardWidget(CShellLevel * l, CString const & name = CGuid::Generate64(GetClassName())) : CBoardAvatar(l, name)
			{
			}

			~CBoardWidget()
			{
			}

			void Place(CFieldWorld * f) override
			{
				__super::Place(f);

				FieldElement->PositioningType = EFieldPositioningType::Stand;

				FieldElement->ItemMargin		= CFloat6(0);
				FieldElement->ItemPadding		= CFloat6(2);
				FieldElement->IconSize			= CSize(24, 24, 24);
				FieldElement->IconTitleMode		= ECardTitleMode::Right;
				FieldElement->WidgetTitleMode	= ECardTitleMode::No;

				Surface->Perspective = true;
				Surface->Express(L"W", [this]{ return FieldElement->W; });

				if(Level->World->Viewports.Has([](auto i){ return i->Tags.Contains(LAYOUT_HUD_MAXIMIZE); }))
				{
					FieldElement->IconSize	= CSize(48, 48, 48);
					Surface->Express(L"H", [this]{ return FieldElement->H; });
				} 
				else
				{
					Surface->Express(L"H", [this]{ return 50.f; });
				}
			}

			void Open(CWorldCapabilities * caps, CUnit * a) override
			{
				if(FieldElement->PositioningType != EFieldPositioningType::Lay) // means not called yet
				{
					FieldElement->PositioningType = EFieldPositioningType::Lay;

					FieldElement->ItemMargin		= CFloat6(4);
					FieldElement->ItemPadding		= CFloat6(4);
					FieldElement->IconSize			= CSize(48, 48, 48);
					FieldElement->IconTitleMode		= ECardTitleMode::Bottom;
					FieldElement->WidgetTitleMode	= ECardTitleMode::No;
					FieldElement->Visual->SetMaterial(Level->World->Materials->GetMaterial(L"0 0 0"));
					FieldElement->Express(L"P", []{ return CFloat6(5.f); });

					Surface->Express(L"W", [this]{ return Surface->Slimits.Smax.W; });
					Surface->Express(L"H", [this]{ return Surface->Slimits.Smax.H; });
				}

				__super::Open(caps, a);
			}
	};
}