#pragma once
#include "Nexus.h"
#include "Sun.h"
#include "Client.h"
#include "HttpClient.h"

namespace uc
{
	class CSunClient : public CClient, public CSun
	{
		public:
			CHttpClient *	Http = null;
			CNexus *		Nexus;

			CSunClient(CNexus * nexus, CClientInstance * is);

			
			IInterface *	Connect(CString const & iface) override;
			void			Disconnect(IInterface * iface) override;

			CSunSettings	GetSettings() override;

	};
}