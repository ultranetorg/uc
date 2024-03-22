#pragma once
#include "ShellLevel.h"

namespace uc
{
	class CTheme : public virtual CWorldEntity
	{
		public:
			CShellLevel *								Level;
			//CConfig *									Config;
			CXon *										RootParameter;

			CString										Source;
			CVwmImporter *								Importer = null;
			IDirectory *								Directory = null;

			UOS_RTTI
			CTheme(CShellLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CTheme();

			virtual void								SaveInstance() override;
			virtual void								LoadInstance() override;

			void										SetSource(CString & o);
	};
}
