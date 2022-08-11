#pragma once
#include "ExperimentalLevel.h"
#include "BitfinexProvider.h"
#include "Trade.h"

namespace uc
{
	class CTradeHistory : public CTrade, public IIdleWorker
	{
		public:
			CExperimentalLevel *						Level;
			CHttpRequest *								Request = null;
			CArray<float>								Values;
			std::mutex									Lock;

			int64_t										Begin;
			int64_t										End;

			float										Max;
			float										Min;

			CDateTime									LastCheck = CDateTime::Min;

			UOS_RTTI
			CTradeHistory(CExperimentalLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CTradeHistory();

			void										Refresh();

			void										LoadInstance() override;

			void										OnServerDestructing(CStorableObject * o);

			void										DoIdle() override;
	};
}
