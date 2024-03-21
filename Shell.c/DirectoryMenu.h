#pragma once
#include "ShellLevel.h"

namespace uc
{ 
	class CDirectoryMenu : public CWorldEntity
	{
		public:
			CShellLevel *								Level;
			CList<CString>								Paths;

			UOS_RTTI
			CDirectoryMenu(CShellLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CDirectoryMenu();

			void										AddPath(CString & o);
			virtual void								SaveInstance() override;
			virtual void								LoadInstance() override;
	};
}
