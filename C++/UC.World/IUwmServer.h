#pragma once

namespace uc
{
	class IUwmServer
	{
		public:
			inline static const CString				InterfaceName = L"IUwm";

			virtual CElement *						CreateElement(CString const & name, CString const & type)=0;

			virtual ~IUwmServer(){}
	};
}