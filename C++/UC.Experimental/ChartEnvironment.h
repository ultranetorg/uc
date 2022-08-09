#pragma once
#include "ChartElement.h"
#include "TradingviewElement.h"

namespace uc
{
	class CChartEnvironment : public CEnvironmentWindow
	{
		public:
			CExperimentalLevel *						Level;
			CElement *									ChartElement = null;
			CRectangleSizer								Sizer;
			CSize										ContentArea;
			CObject<CTrade>							Entity;
			
			UOS_RTTI
			CChartEnvironment(CExperimentalLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CChartEnvironment();
			
			void										SetEntity(CUol & l);
			void										OnDependencyDestroying(CBaseNexusObject *);
			void										OnTitleChanged(CWorldEntity * e);

			virtual void								SaveInstance() override;
			virtual void								LoadInstance() override;
	};
}
