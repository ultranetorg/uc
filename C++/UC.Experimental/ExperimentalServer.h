#pragma once
#include "ExperimentalLevel.h"
#include "GeoStore.h"

namespace uc
{
	class CExperimentalServer : public CExperimentalLevel, public CPersistentServer, public IWorldFriend, public IShellFriend, public IAvatarServer, public IExecutor, public IUwmServer
	{
		using CExperimentalLevel::Server;
		using CExperimentalLevel::Nexus;
		using CExperimentalLevel::Storage;
		
		public:
			CGeoStore *					GeoStore;

			UOS_RTTI
			CExperimentalServer(CNexus * l, CServerInstance * si);
			~CExperimentalServer();
			
			void						EstablishConnections(bool storage, bool world);

			void						Initialize() override;
			void						Start() override;

			IInterface * 				Connect(CString const & pr);
			void						Disconnect(IInterface * s);
			CInterObject *				CreateObject(CString const & name) override;

			void						Execute(CXon * command, CExecutionParameters * ep);

			CString						GetTitle() override { return L"Experimental"; }
			CRefList<CMenuItem *>		CreateActions() override;

			IMenuSection *				CreateNewMenu(CFieldElement * fo, CFloat3 & p, IMenu * m) override;

			virtual CInterObject *		GetEntity(CUol & e) override;
			virtual CList<CUol>			GenerateSupportedAvatars(CUol & o, CString const & type) override;
			virtual CAvatar		*		CreateAvatar(CUol & a) override;
			virtual void				DestroyAvatar(CAvatar * a) override;

			virtual CElement *			CreateElement(CString const & name, CString const & type) override;
	};

	class CExperimentalClient : public CClient
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

			IInterface * Connect(CString const & iface) override
			{
				return Server->Connect(iface);
			}

			void Disconnect(IInterface * iface) override
			{
				Server->Disconnect(iface);
			}
	};
}
