#pragma once

namespace uc
{
	class IUwm
	{
		public:
			inline static const CString				InterfaceName = L"IUwm";

			virtual CElement *						CreateElement(CString const & name, CString const & type)=0;

			virtual ~IUwm(){}
	};
}