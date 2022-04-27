#pragma once
#include "Field.h"

namespace uc
{
	class CFieldIcon : public CIcon<CFieldServer>
	{
		public:
			CShellLevel *								Level;
			CSolidRectangleMesh *						AMesh = null;
						
			UOS_RTTI
			CFieldIcon(CShellLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CFieldIcon();

			void										OnModelOpened(CModel * a);
			void										OnModelClosed(CModel * a);

			virtual void								UpdateLayout(CLimits const & l, bool apply) override;
	};
}
