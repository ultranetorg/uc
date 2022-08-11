#pragma once
#include "TradingviewElement.h"

namespace uc
{
	class CTradingviewWidget : public CWidgetWindow
	{
		public:
			CExperimentalLevel *						Level;
			CTradingviewElement *						Element = null;
			CTradingview *								Entity;
			bool										Confugured = false;
			
			UOS_RTTI
			CTradingviewWidget(CExperimentalLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CTradingviewWidget();
			
			void										SetEntity(CUol & e);
			void										OnDependencyDestroying(CStorableObject *);

			void										Open(CWorldCapabilities * caps, CUnit * a) override;
			void										Place(CFieldWorld * fo) override;

			void										OnChanged();

			void										OnMouse(CActive *, CActive *, CMouseArgs *) override;

	};
}
