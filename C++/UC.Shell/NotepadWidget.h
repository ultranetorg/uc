#pragma once
#include "ShellLevel.h"
#include "Notepad.h"

namespace uc
{
	class CNotepadWidget : public CWidgetWindow
	{
		public:
			CShellLevel *								Level;
			CRectangleMenu *							Menu = null;
			CNotepad *									Entity;

			UOS_RTTI
			CNotepadWidget(CShellLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			virtual ~CNotepadWidget();

			virtual void								SetEntity(CUol & o) override;

			virtual void								SaveInstance() override;
			virtual void								LoadInstance() override;
	};
}
