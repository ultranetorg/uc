#pragma once
#include "Nexus.h"
#include "Sun.h"
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

			
			IProtocol *	Connect(CString const & iface) override;
			void			Disconnect(IProtocol * iface) override;

			CSunSettings	GetSettings() override;

	};
}