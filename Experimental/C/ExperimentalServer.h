#pragma once
#include "ExperimentalLevel.h"
#include "GeoStore.h"

namespace uc
{
	class CExperimentalServer : public CExperimentalLevel, public CPersistentServer, public CWorldFriendProtocol, public CShellFriendProtocol, public CAvatarProtocol, public CExecutorProtocol, public CUwmProtocol
	{
		using CExperimentalLevel::Server;
		using CExperimentalLevel::Nexus;
		using CExperimentalLevel::Storage;
		
		public:
			CGeoStore *					GeoStore;
			inline static const CString Experimental_config = L"Experimental.config";

			UOS_RTTI
			CExperimentalServer(CNexus * l, CServerInstance * si);
			~CExperimentalServer();
			
			void						EstablishConnections(bool storage, bool world, bool imageextractor);

			void						UserStart() override;
			IProtocol * 				Accept(CString const & pr) override;
			void						Break(IProtocol * s) override;
			CInterObject *				CreateObject(CString const & name) override;

			void						Execute(CXon * command, CExecutionParameters * ep);

			CString						GetTitle() override { return Instance->Release->Address.Application; }
			CRefList<CMenuItem *>		CreateActions() override;

			IMenuSection *				CreateNewMenu(CFieldElement * fo, CFloat3 & p, IMenu * m) override;

			virtual CInterObject *		GetEntity(CUol & e) override;
			virtual CList<CUol>			GenerateSupportedAvatars(CUol & o, CString const & type) override;
			virtual CAvatar		*		CreateAvatar(CUol & a) override;
			virtual void				DestroyAvatar(CAvatar * a) override;

			virtual CElement *			CreateElement(CString const & name, CString const & type) override;
	};

	class CExperimentalClient : public CClient, public virtual IType
	{
		public:
			CExperimentalServer * Server;

			UOS_RTTI
			CExperimentalClient(CNexus * nexus, CClientInstance * instance, CExperimentalServer * server) : CClient(instance)
			{
				Server = server;
			}

			virtual ~CExperimentalClient()
			{
			}

			IProtocol * Connect(CString const & iface) override
			{
				return Server->Accept(iface);
			}

			void Disconnect(IProtocol * iface) override
			{
				Server->Break(iface);
			}
	};
}
