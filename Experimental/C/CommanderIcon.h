#pragma once
#include "Commander.h"

namespace uc
{
	class CCommanderIcon : public CIcon<CCommander>
	{
		public:
			CExperimentalLevel *						Level;
						
			UOS_RTTI
			CCommanderIcon(CExperimentalLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CCommanderIcon();
	};
}
