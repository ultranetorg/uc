#pragma once

namespace uc
{
	class CSunSettings
	{
		public:
			CString		ProfilePath;
	};

	class CSun : public IInterface
	{
		public:
			inline static const CString   	InterfaceName = L"Sun";
	
			virtual CSunSettings			GetSettings() = 0;
	};

}