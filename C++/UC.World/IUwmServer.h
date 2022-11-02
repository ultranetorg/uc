#pragma once

namespace uc
{
	class CUwmProtocol
	{
		public:
			inline static const CString				InterfaceName = L"Uwm1";

			virtual CElement *						CreateElement(CString const & name, CString const & type)=0;

			virtual ~CUwmProtocol(){}
	};
}