#pragma once
#include "TradeHistory.h"

namespace uc
{
	class CChartIcon : public CIcon<CTrade>
	{
		public:
			CExperimentalLevel *						Level;
						
			UOS_RTTI
			CChartIcon(CExperimentalLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CChartIcon();
	};
}
