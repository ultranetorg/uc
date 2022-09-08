#pragma once
#include "Nexus.h"
#include "Client.h"
#include "HttpClient.h"

namespace uc
{
	class CFunSettings
	{
		public:
			CString		ProfilePath;
	};

	class CFun : public IInterface
	{
		public:
			inline static const CString   	InterfaceName = L"Fun";
	
			virtual CFunSettings			GetSettings() = 0;
	};

	class CFunClient : public CClient, public CFun
	{
		public:
			CHttpClient *	Http = null;
			CNexus *		Nexus;

			CFunClient(CNexus * nexus, CClientInstance * is);

			
			IInterface *	Connect(CString const & iface) override;
			void			Disconnect(IInterface * iface) override;

			CFunSettings	GetSettings() override;

	};
}