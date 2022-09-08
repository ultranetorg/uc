#pragma once
#include "AvatarMetrics.h"
#include "Element.h"
#include "WorldCapabilities.h"

namespace uc
{
	class CWorld;
	class IAvatarServer;
	
	class UOS_WORLD_LINKING CAvatar : public CElement, public CPersistentObject 
	{
		public:
			inline static const CString				Scheme = L"worldavatar";

			CProtocolConnection<IAvatarServer>	Protocol;
			CWorld *								World;

			UOS_RTTI
			CAvatar(CWorld * l, CServer * s, CString const & name); 

			using CPersistentObject::Load;

			virtual void							SetEntity(CUol & e);
			virtual void							DetermineSize(CSize & smax, CSize & s);
	};
	
	class UOS_WORLD_LINKING CStaticAvatar : public CAvatar
	{
		public: 
			UOS_RTTI
			CStaticAvatar(CWorld * l, CServer * s, CXon * r, IMeshStore * mhs, IMaterialStore * mts, const CString & name = CGuid::Generate64(GetClassName()));
			CStaticAvatar(CWorld * l, CServer * s, CElement * e, const CString & name);
			virtual ~CStaticAvatar();
	};

	class IAvatarServer : public virtual IInterface
	{
		public:
			inline static const CString				InterfaceName = L"IAvatar";

			virtual CInterObject *					GetEntity(CUol & a)=0;
			virtual CAvatar *						CreateAvatar(CUol & a)=0;
			virtual CList<CUol>						GenerateSupportedAvatars(CUol & o, CString const & type)=0;
			virtual void							DestroyAvatar(CAvatar * a) = 0;

			virtual ~IAvatarServer(){}
	};
}
