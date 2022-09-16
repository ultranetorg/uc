#pragma once

namespace uc
{
	class CSunSettings
	{
		public:
			CString		ProfilePath;
	};

	class CSunProtocol : public IProtocol
	{
		public:
			inline static const CString   	InterfaceName = L"Sun1";
	
			virtual CSunSettings			GetSettings() = 0;
	};

}