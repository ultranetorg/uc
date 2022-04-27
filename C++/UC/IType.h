#pragma once
#include "String.h"

namespace uc
{
	class IType
	{
		public:
			virtual CString &							GetInstanceName()=0;
			template<class T> T	*						As(){ return dynamic_cast<T *>(this); }

			//virtual CString		ToString(){ return L""; }

		protected:
			virtual ~IType(){}
	};
}