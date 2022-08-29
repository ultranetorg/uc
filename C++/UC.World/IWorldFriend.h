#pragma once
//#include "IMenu.h"
#include "Element.h"
#include "MenuItem.h"

namespace uc
{
	class IWorldFriend : public virtual IInterface
	{
		public:
			inline static const CString					InterfaceName = L"IWorldFriend";

			virtual CString 							GetTitle()=0;
			virtual CRefList<CMenuItem *>				CreateActions()=0;

			virtual ~IWorldFriend(){}
	};
}
