#pragma once
#include "ShellLevel.h"
#include "Notepad.h"

namespace uc
{
	class CNotepadIcon : public CIcon<CNotepad>
	{
		public:
			CShellLevel *								Level;
					
			UOS_RTTI
			CNotepadIcon(CShellLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CNotepadIcon();

			void										SetEntity(CUol & o) override;
	};
}
