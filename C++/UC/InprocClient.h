#pragma once
#include "Client.h"

namespace uc
{
	class UOS_LINKING CInprocClient : public CClient, public virtual IType
	{
		public:
			CNexus * Nexus;

			UOS_RTTI
			CInprocClient(CNexus * nexus, CClientInstance * instance) : CClient(instance)
			{
				Nexus = nexus;
			}

			virtual ~CInprocClient()
			{
			}

			IProtocol * Connect(CString const & protocol) override
			{
				return Nexus->Servers.Find([&](auto i){ return i->Name == Instance->Name; })->Instance->Accept(protocol);
			}

			void Disconnect(IProtocol * protocol) override
			{
				Nexus->Servers.Find([&](auto i){ return i->Name == Instance->Name; })->Instance->Break(protocol);
			}
	};
}