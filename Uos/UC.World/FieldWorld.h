#pragma once

namespace uc
{
	class CFieldWorld : public CWorldCapabilities
	{
		public:
			virtual void								MoveAvatar(CAvatar * a, CTransformation & p)=0;
			virtual void								DeleteAvatar(CAvatar * a)=0;
			virtual CPositioning *						GetPositioning()=0;
						
			virtual void								AddIconMenu(IMenuSection * ms, CAvatar * a)=0;
			virtual void								AddTitleMenu(IMenuSection * ms, CAvatar * a)=0;

			virtual ~CFieldWorld(){}
	};

}