#pragma once

namespace uc
{
	class CBoardEnvironment : public CBoardAvatar
	{
		public:
			UOS_RTTI
			CBoardEnvironment(CShellLevel * l, CString const & name = CGuid::Generate64(GetClassName())) : CBoardAvatar(l, name)
			{
				FieldElement->PositioningType = EFieldPositioningType::Stand;
			}

			~CBoardEnvironment()
			{
			}

			void Open(CWorldCapabilities * caps, CUnit * a) override
			{
				__super::Open(caps, a);

				a->SetView(Level->World->NearView);

				if(caps->Free3D)
				{
					FieldElement->ItemMargin			= CFloat6(4);
					FieldElement->ItemPadding			= CFloat6(4);
					FieldElement->IconSize				= CSize(64, 64, 64);
					FieldElement->IconTitleMode			= ECardTitleMode::Right;
					FieldElement->WidgetTitleMode		= ECardTitleMode::No;
					FieldElement->ItemTransformation	= CTransformation::FromRotation(CQuaternion(-CFloat::PI/2, 0, 0));

					FieldElement->Surface->Express(L"W", [this]{ return FieldElement->Surface->Slimits.Smax.W; });
					FieldElement->Surface->Express(L"H", [this]{ return FieldElement->Surface->Slimits.Smax.H; });
				}
				else
					throw CException(HERE, L"Not supported");

			}

			void DetermineSize(CSize & smax, CSize & s) override
			{
				if(Capabilities->Free3D)
				{
					Express(L"W", [smax]{ return smax.W * 0.75f; });
					Express(L"H", [smax]{ return smax.H * 0.5f; });
					UpdateLayout(CLimits(smax, smax), false);
				}
			}

			CTransformation DetermineTransformation(CPositioning * ps, CPick & pk, CTransformation & t)
			{
				return t;
				//return CTransformation(CFloat3(-Size.W/2, -1000, 1000), CQuaternion::FromEuler(0, CFloat::PI/2, 0));
			}
	};
}