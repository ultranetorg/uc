#pragma once
#include "ThemeAvatar.h"

namespace uc
{
	class CThemeWidget : public CThemeAvatar
	{
		public:
			UOS_RTTI
			CThemeWidget(CShellLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CThemeWidget();
	};

}

