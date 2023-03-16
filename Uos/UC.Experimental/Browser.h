#pragma once
#include "ExperimentalLevel.h"

namespace uc
{
	class CBrowser : public virtual CWorldEntity
	{
		public:
			CExperimentalLevel *						Level;
			CUrl										Address;

			CEvent<>									AddressChanged;

			UOS_RTTI
			CBrowser(CExperimentalLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CBrowser();

			void										SetAddress(CUrl & u);

			void										SaveInstance() override;
			void										LoadInstance() override;

	}; 
}
