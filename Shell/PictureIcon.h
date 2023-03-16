#pragma once
#include "Picture.h"

namespace uc
{
	class CPictureIcon : public CIcon<CPicture>
	{
		public:
			CShellLevel *								Level;
					
			UOS_RTTI
			CPictureIcon(CShellLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CPictureIcon();

			void										SetEntity(CUol & o);
	};
}
