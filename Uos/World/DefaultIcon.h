#pragma once
#include "Icon.h"

namespace uc
{
	class UOS_WORLD_LINKING CDefaultIcon : public CIcon<CWorldEntity>
	{
		public:
			UOS_RTTI
			CDefaultIcon(CWorldProtocol * l, CString const & name = CGuid::Generate64(GetClassName()));
			virtual ~CDefaultIcon();

			void										SetEntity(CUol & e) override;

	};
}