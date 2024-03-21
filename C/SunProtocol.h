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
	
			virtual void					GetSettings(std::function<void(CSunSettings &)> ok) = 0;
			virtual void					QueryRelease(CList<CReleaseAddress> & releases, std::function<void(nlohmann::json)> ok, std::function<void()> failure) = 0;
	};

}