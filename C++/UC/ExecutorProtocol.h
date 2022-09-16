#pragma once
#include "Core.h"

namespace uc
{
	class CExecutorProtocol : public virtual IProtocol
	{
		public:
			inline static const CString   		InterfaceName = L"Executor1";
			static const inline CString			CreateDirective	= L"create";

			void virtual						Execute(CXon * arguments, CExecutionParameters * parameters)=0;

			virtual ~CExecutorProtocol(){}
	};
}