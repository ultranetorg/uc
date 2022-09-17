#pragma once
#include "Nexus.h"
#include "SunProtocol.h"
#include "Client.h"
#include "HttpClient.h"

namespace uc
{
	class CSunClient : public CClient, public CSunProtocol
	{
		public:
			CHttpClient *	Http = null;
			CNexus *		Nexus;

			CSunClient(CNexus * nexus, CClientInstance * is);

			
			IProtocol *		Connect(CString const & iface) override;
			void			Disconnect(IProtocol * iface) override;

			void			GetSettings(std::function<void(CSunSettings &)> ok) override;
			void			QueryRelease(CList<CReleaseAddress> & releases, std::function<void(nlohmann::json)> ok, std::function<void()> failure);

			void			Send(CString const & method, CString const & json, std::function<void(nlohmann::json)> ok, std::function<void()> failure);
	};
}