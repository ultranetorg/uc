#pragma once
#include "ExperimentalLevel.h"

namespace uc
{
	class CCommander : public virtual CWorldEntity
	{
		public:
			CExperimentalLevel *						Level;
			CString										Root;
			CString										Path;

			CEvent<>									PathChanged;

			UOS_RTTI
			CCommander(CExperimentalLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CCommander();

			void										SetRoot(CString const & u);
			void										SetPath(CString const & u);

			void										SaveInstance() override;
			void										LoadInstance() override;
	}; 
}
