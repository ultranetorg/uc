#pragma once
#include "FieldEnvironment.h"

namespace uc
{
	class CShell : public virtual IType
	{
		public:
			virtual CObject<CField>						FindField(CUol & o)=0;
			virtual CObject<CFieldEnvironment>			FindFieldEnvironmentByEntity(CUol & o)=0;

			UOS_RTTI
			virtual ~CShell(){}		
	};
}