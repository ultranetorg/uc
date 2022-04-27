#pragma once
#include "ChartElement.h"

namespace uc
{
	class CChartWidget : public CWidgetWindow
	{
		public:
			CExperimentalLevel *						Level;
			CChartElement *								Element = null;
			CTradeHistory *								Entity;
			CRectangleMenu *							Menu = null;
			
			UOS_RTTI
			CChartWidget(CExperimentalLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CChartWidget();
			
			void										SetEntity(CUol & e);
			void										OnDependencyDestroying(CNexusObject *);

			void										Place(CFieldWorld * fo) override;

			void										OnChanged();

			void										OnMouse(CActive *, CActive *, CMouseArgs *) override;

	};
}
