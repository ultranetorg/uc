#pragma once
//#include "IMenu.h"
#include "Element.h"
#include "MenuItem.h"

namespace uc
{
	struct CAttacheble
	{
		CString		Title;
		CUol		Url;
	};

	class IWorldFriend : public virtual IInterface
	{
		public:
			inline static const CString					InterfaceName = L"IWorldFriend";

			//virtual CUol								CreateAvatar(const CString & type, CUol & name)=0;
			//virtual CAvatar *							AcquireAvatar(CUol & u)=0;
			//virtual void								ReleaseAvatar(CAvatar * o)=0;

			virtual CString 							GetTitle()=0;
			virtual CRefList<CMenuItem *>				CreateActions()=0;

			virtual ~IWorldFriend(){}
	};
}
