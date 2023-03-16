#pragma once
#include "Browser.h"
#include "CefElement.h"

namespace uc
{
	class CBrowserEnvironment : public CEnvironmentWindow
	{
		public:
			CExperimentalLevel *						Level;
			CBrowser *									Entity;
			CRectangleMenu *							Menu = null;

			CButton	*									BackButton;
			CButton	*									ForwardButton;
			CButton	*									ReloadButton;
			CTextEdit *									AddressEdit;
			CCefElement *								CefElement;

			UOS_RTTI
			CBrowserEnvironment(CExperimentalLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CBrowserEnvironment();
			
			void										SetEntity(CUol & l);
			
			void										OnCefTitleChanged(CString const & t);
			void										OnCefAddressChanged(CString & t);
			
			void										DetermineSize(CSize & smax, CSize & s) override;

			void										OnMouse(CActive * r, CActive * s, CMouseArgs * a);
	};
}