#pragma once
#include "AvatarMetrics.h"
#include "Element.h"
#include "WorldCapabilities.h"

namespace uc
{
	class CWorldProtocol;
	class CAvatarProtocol;
	
	class UOS_WORLD_LINKING CAvatar : public CElement, public CPersistentObject 
	{
		public:
			inline static const CString				Scheme = L"worldavatar";

			CProtocolConnection<CAvatarProtocol>	Protocol;
			CWorldProtocol *								World;

			UOS_RTTI
			CAvatar(CWorldProtocol * l, CServer * s, CString const & name); 

			using CPersistentObject::Load;

			virtual void							SetEntity(CUol & e);
			virtual void							DetermineSize(CSize & smax, CSize & s);
	};
	
	class UOS_WORLD_LINKING CStaticAvatar : public CAvatar
	{
		public: 
			UOS_RTTI
			CStaticAvatar(CWorldProtocol * l, CServer * s, CXon * r, IMeshStore * mhs, IMaterialStore * mts, const CString & name = CGuid::Generate64(GetClassName()));
			CStaticAvatar(CWorldProtocol * l, CServer * s, CElement * e, const CString & name);
			virtual ~CStaticAvatar();
	};

	class CAvatarProtocol : public virtual IProtocol
	{
		public:
			inline static const CString				InterfaceName = L"Avatar1";

			virtual CInterObject *					GetEntity(CUol & a)=0;
			virtual CAvatar *						CreateAvatar(CUol & a)=0;
			virtual CList<CUol>						GenerateSupportedAvatars(CUol & o, CString const & type)=0;
			virtual void							DestroyAvatar(CAvatar * a) = 0;

			virtual ~CAvatarProtocol(){}
	};
}
