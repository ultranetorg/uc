#pragma once
#include "Browser.h"
#include "CefElement.h"

namespace uc
{
	class CBrowserWidget : public CWidgetWindow
	{
		public:
			CExperimentalLevel *						Level;
			CBrowser *									Entity;
			CRectangleMenu *							Menu = null;
			CCefElement *								CefElement;
			
			UOS_RTTI
			CBrowserWidget(CExperimentalLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CBrowserWidget();

			virtual void								Place(CFieldWorld * fo) override;

			void										SetEntity(CUol & e);

			void										OnCefTitleChanged(CString const & t);
			void										OnCefAddressChanged(CString & url);
			
			void										Open(CWorldCapabilities * caps, CUnit * a) override;

	};
}
