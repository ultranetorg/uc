#pragma once
#include "EmailAccount.h"

namespace uc
{
	class CEmailAccountEnvironment : public CEnvironmentWindow
	{
		public:
			CExperimentalLevel *						Level;
			CEmailAccount *								Entity;

			UOS_RTTI
			CEmailAccountEnvironment(CExperimentalLevel * l, CString const & name);
			~CEmailAccountEnvironment();

			void										SetEntity(CUol & e);
			void										OnDependencyDestroying(CInterObject *);

			virtual void								DetermineSize(CSize & smax, CSize & s) override;
	};
}