#pragma once
//#include "IMenu.h"
#include "Element.h"
#include "MenuItem.h"

namespace uc
{
	class CWorldFriendProtocol : public virtual IProtocol
	{
		public:
			inline static const CString					InterfaceName = L"WorldFriend1";

			virtual CString 							GetTitle()=0;
			virtual CRefList<CMenuItem *>				CreateActions()=0;

			virtual ~CWorldFriendProtocol(){}
	};
}
