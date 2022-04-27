#pragma once
#include "ShellLevel.h"
#include "Link.h"

namespace uc
{
	class CLinkIcon : public CIcon<CLink>
	{
		public:
			using CElement::UpdateLayout;

			CShellLevel *								Level;

			UOS_RTTI
			CLinkIcon(CShellLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CLinkIcon();

			void										SetEntity(CUol & o) override;

			void										UpdateLayout(CLimits const & l, bool apply) override;
	};
}
