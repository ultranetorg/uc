#pragma once
#include "Globe.h"
#include "Earth.h"

namespace uc
{
	class CEarthWidget : public CWidgetWindow
	{
		public:
			CExperimentalLevel *						Level;
			CEarth  *									Entity;
			CGlobe *									Globe = null;
			CRectangleMenu *							Menu = null;
			
			UOS_RTTI
			CEarthWidget(CExperimentalLevel * l, CGeoStore * s, CString const & name = CGuid::Generate64(GetClassName()));
			virtual ~CEarthWidget();

			void										SetEntity(CUol & e);

			void										Open(CWorldCapabilities * caps, CUnit * a) override;

	};
}
