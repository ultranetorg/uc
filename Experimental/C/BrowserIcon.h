#pragma once
#include "Browser.h"

namespace uc
{
	class CBrowserIcon : public CIcon<CBrowser>
	{
		public:
			CExperimentalLevel *						Level;
			CString										CurrentFile;

			UOS_RTTI
			CBrowserIcon(CExperimentalLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CBrowserIcon();

			void virtual								UpdateLayout(CLimits const & l, bool apply) override;
	};
}
