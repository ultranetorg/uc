#pragma once
#include "Element.h"

namespace uc
{
	class UOS_WORLD_LINKING CScrollbar : public CElement
	{
		public:
			CStyle *									Style;
			CEngine *									Engine;
			
			float										Value = 0;
			float										Total = NAN;
			float										Visible = NAN;

			float										T; // available area
			float										P;
			float										G;
			float										S; // buttons size

			CFloat3										Point;

			CEvent<>									Scrolled;

			UOS_RTTI
			CScrollbar(CWorldLevel * l, CStyle * s);
			~CScrollbar();

			void										SetTotal(float v);
			void										SetVisible(float v);
			void										ValueToPosition();
			virtual void								Draw();
			void										OnMoveInput(CActive * r, CActive * s, CMouseArgs * a);

			virtual void								UpdateLayout(CLimits const & l, bool apply) override;
	};
}
