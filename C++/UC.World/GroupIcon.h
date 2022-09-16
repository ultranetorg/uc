#pragma once
#include "Group.h"
#include "Icon.h"

namespace uc
{
	class CGroupIcon : public CIcon<CGroup>
	{
		public:
			CWorldProtocol *									Level;
						
			UOS_RTTI
			CGroupIcon(CWorldProtocol * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CGroupIcon();
	};
}
