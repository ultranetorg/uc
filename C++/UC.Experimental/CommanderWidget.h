#pragma once
#include "StandardFileList.h"
#include "Commander.h"
//#include "CommanderEnvironment.h"

namespace uc
{
	class CCommanderWidget : public CWidgetWindow
	{
		public:
			CExperimentalLevel *						Level;
			CStandardFileList *							FileList = null;
			CCommander *								Entity;
			CRectangleMenu *							Menu = null;
			
			UOS_RTTI
			CCommanderWidget(CExperimentalLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CCommanderWidget();
			
			void										SetEntity(CUol & e);
	
			void										OnFileListPathChanged(CString const & o);
	};
}
