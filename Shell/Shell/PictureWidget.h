#pragma once
#include "Picture.h"

namespace uc
{
	class CPictureWidget : public CWidgetWindow
	{
		public:
			CShellLevel *								Level;
			CTexture *									Texture = null;
			CRectangleMenu *							Menu = null;
			CPicture *									Entity;

			UOS_RTTI
			CPictureWidget(CShellLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			virtual ~CPictureWidget();

			virtual void								SetEntity(CUol & o) override;

			void										Refresh();

			void virtual								UpdateLayout(CLimits const & l, bool apply) override;

			void										DetermineSize(CSize & smax, CSize & s) override;

	};
}
