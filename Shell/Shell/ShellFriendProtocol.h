#pragma once
#include "FieldElement.h"

namespace uc
{
	class CShellFriendProtocol  : public virtual IProtocol
	{
		public:
			inline static const CString					InterfaceName = L"ShellFriend1";

			virtual CString 							GetTitle()=0;
			virtual IMenuSection *						CreateNewMenu(CFieldElement * f, CFloat3 & p, IMenu * m)=0;

			virtual ~CShellFriendProtocol(){}
	};
}
	