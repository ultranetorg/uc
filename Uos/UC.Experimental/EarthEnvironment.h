#pragma once
#include "Globe.h"
#include "Earth.h"

namespace uc
{
	class CEarthEnvironment : public CEnvironmentWindow
	{
		public:
			CObject<CEarth>								Entity;
			CExperimentalLevel *						Level;
			CGlobe *									Globe;

			UOS_RTTI
			CEarthEnvironment(CExperimentalLevel * l, CGeoStore * s, const CString & name = GetClassName());
			~CEarthEnvironment();
			
			void										SetEntity(CUol & e);

			virtual void								SaveInstance() override;
			virtual void								LoadInstance() override;

			virtual void								DetermineSize(CSize & smax, CSize & s) override;

			virtual void								Open(CWorldCapabilities * caps, CUnit * a) override;

	};
}
