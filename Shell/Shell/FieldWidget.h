#pragma once
#include "FieldAvatar.h"

namespace uc
{
	class CFieldWidget : public CFieldAvatar
	{
		public:
			UOS_RTTI
			CFieldWidget(CShellLevel * l, const CString & name = CGuid::Generate64(GetClassName()));
	};
}


