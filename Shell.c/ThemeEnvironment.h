#pragma once
#include "ThemeAvatar.h"

namespace uc
{
	class CThemeEnvironment : public CThemeAvatar
	{
		public:
			CMap<CString, CFloat3>					Models;

			UOS_RTTI
			CThemeEnvironment(CShellLevel  * l, const CString & name = CGuid::Generate64(GetClassName()));
			~CThemeEnvironment();

			virtual void								SaveInstance() override;
			virtual void								LoadInstance() override;

			void										OnCameraMoved(CCamera * c);
			void										OnModelOpened(CShowParameters *, CModel * a);
			//void										OnClick(CActive * r, CActive * s, CCursorEventArgs & a);
			void										OnMoveInput(CActive * r, CActive * s, CMouseArgs * a);
	};
}
