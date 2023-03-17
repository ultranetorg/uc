#pragma once
#include "ShellLevel.h"
#include "DirectoryMenu.h"

namespace uc
{ 
	class CApplicationsMenu : public CDirectoryMenu
	{
		public:
			UOS_RTTI
			CApplicationsMenu(CShellLevel * l, CString const & name = CGuid::Generate64(GetClassName())) : CDirectoryMenu(l, name)
			{
			}

			virtual void SaveInstance() override
			{
			}

			virtual void LoadInstance() override
			{
			}
	};
}
