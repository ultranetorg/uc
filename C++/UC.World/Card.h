#pragma once
#include "IMenu.h"
#include "World.h"

namespace uc
{
	class UOS_WORLD_LINKING CCard : public CRectangle
	{
		public:
			CWorld *									Level;
			ECardTitleMode								TitleMode = ECardTitleMode::Null;
			EXAlign										TitleXAlign = EXAlign::Null;
			CElement *									Face = null;
			CText *										Text = null;
			CString										Title;
			float										Spacing = 5.f;
			CCardMetrics								Metrics;
			CFloat4										TextColor;
			CString										Type;

			UOS_RTTI
			CCard(CWorld * l, const CString & name = GetClassName());
			~CCard();

			void										SetFace(CElement * f);
			void										SetTitleMode(ECardTitleMode tp);
	};
}