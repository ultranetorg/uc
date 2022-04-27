#pragma once
#include "Event.h"

namespace uc
{
	class ILevel
	{
		public:
			CEvent<int, ILevel *>						LevelCreated;
			CEvent<int, ILevel *>						LevelDestroyed;

			virtual ~ILevel(){};
	};
}