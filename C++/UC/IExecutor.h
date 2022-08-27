#pragma once
#include "Core.h"

namespace uc
{
	class IExecutor
	{
		public:
			inline static const CString   		InterfaceName = L"IExecutor";

			void virtual						Execute(CXon * arguments, CExecutionParameters * parameters)=0;

			virtual ~IExecutor(){}
	};
}