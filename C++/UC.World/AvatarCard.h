#pragma once
#include "Card.h"

namespace uc
{
	class UOS_WORLD_LINKING CAvatarCard : public CCard
	{
		public:
			CObject<CAvatar>							Avatar;
			CObject<CWorldEntity>						Entity;
			CProtocolConnection<CAvatarProtocol>		AvatarProtocol;

			UOS_RTTI
			CAvatarCard(CWorldProtocol * l, const CString & name = GetClassName());
			~CAvatarCard();

			void										SetAvatar(CUol & e, CString const & dir);
			void										SetEntity(CUol & a);

			void										OnDependencyDestroying(CInterObject * o);
			void										OnTitleChanged(CWorldEntity *);

			void										SetMetrics(CCardMetrics & m);

			void										Save(CXon * x);
			void										Load(CXon * x);
	};
}