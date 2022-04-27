#pragma once
#include "ExperimentalLevel.h"
#include "BitfinexProvider.h"
#include "Trade.h"

namespace uc
{
	class CTradingview : public CTrade
	{
		public:
			CExperimentalLevel *								Level;

			UOS_RTTI
			CTradingview(CExperimentalLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CTradingview();
	};
}
