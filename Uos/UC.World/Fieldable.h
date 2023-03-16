#pragma once
#include "FieldWorld.h"

namespace uc
{
	class UOS_WORLD_LINKING CFieldable
	{
		public:
			CFieldWorld * Field = null;
		
			virtual ~CFieldable(){}
			
			virtual void Place(CFieldWorld * f)
			{
				Field = f;
			}
	};


	class UOS_WORLD_LINKING CFieldableModel : public CModel, public CFieldable
	{
		public:
			UOS_RTTI
			CFieldableModel(CWorldProtocol * l, CServer * s, ELifespan life, CString const & name) : CModel(l, s, life, name)
			{
			}

			virtual void Place(CFieldWorld * f) override
			{
				Field = f;
				Capabilities = f;
			}
	};

}

