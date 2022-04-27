#pragma once
#include "ColumnFileList.h"
#include "Commander.h"

namespace uc
{
	class CCommanderEnvironment : public CEnvironmentWindow
	{
		public:
			CExperimentalLevel *						Level;
			CColumnFileList *							FileList = null;
			CCommander *								Entity;
			
			UOS_RTTI
			CCommanderEnvironment(CExperimentalLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CCommanderEnvironment();
			
			void										SetEntity(CUol & l);

			void										DetermineSize(CSize & smax, CSize & s) override;

			void										OnFileListPathChanged(CString const & o);
			
			virtual void								SaveInstance() override;
			virtual void								LoadInstance() override;
	};
}
