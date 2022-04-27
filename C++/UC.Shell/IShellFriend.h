#pragma once
#include "FieldElement.h"

namespace uc
{
	class IShellFriend  : public virtual IProtocol
	{
		public:
			virtual CString 							GetTitle()=0;
			virtual IMenuSection *						CreateNewMenu(CFieldElement * f, CFloat3 & p, IMenu * m)=0;

			virtual ~IShellFriend(){}
	};
}
	