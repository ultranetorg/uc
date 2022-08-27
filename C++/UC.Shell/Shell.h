#pragma once
#include "FieldEnvironment.h"

namespace uc
{
	class CShell : public virtual IType
	{
		public:
			CString static inline						InterfaceName = L"IShell";


			virtual CObject<CField>						FindField(CUol & o)=0;
			virtual CObject<CFieldEnvironment>			FindFieldEnvironmentByEntity(CUol & o)=0;

			UOS_RTTI
			virtual ~CShell(){}		
	};
}