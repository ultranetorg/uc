#pragma once
#include "Earth.h"

namespace uc
{
	class CEarthIcon : public CIcon<CEarth>
	{
		public:
			CExperimentalLevel *						Level;
						
			UOS_RTTI
			CEarthIcon(CExperimentalLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CEarthIcon();
	};
}
