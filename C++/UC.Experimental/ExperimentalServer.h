#pragma once
#include "ExperimentalLevel.h"
#include "GeoStore.h"

namespace uc
{
	class CExperimentalServer : public CExperimentalLevel, public CStorableServer, public IWorldFriend, public IShellFriend, public IAvatarProtocol, public IExecutorProtocol, public IUwmProtocol
	{
		using CExperimentalLevel::Server;
		using CExperimentalLevel::Nexus;
		using CExperimentalLevel::Storage;
		
		public:
			CGeoStore *					GeoStore;

			UOS_RTTI
			CExperimentalServer(CNexus * l, CServerInfo * si);
			~CExperimentalServer();
			
			void						EstablishConnections(bool storage, bool world);

			void						Start(EStartMode sm) override;
			IProtocol * 				Connect(CString const & pr)override;
			void						Disconnect(IProtocol * s) override;
			CInterObject *			CreateObject(CString const & name) override;

			bool						CanExecute(const CUrq & o);
			void						Execute(const CUrq & o, CExecutionParameters * ep);

			CString						GetTitle() override { return L"Experimental"; }
			CRefList<CMenuItem *>		CreateActions() override;

			IMenuSection *				CreateNewMenu(CFieldElement * fo, CFloat3 & p, IMenu * m) override;

			virtual CInterObject *	GetEntity(CUol & e) override;
			virtual CList<CUol>			GenerateSupportedAvatars(CUol & o, CString const & type) override;
			virtual CAvatar		*		CreateAvatar(CUol & a) override;
			virtual void				DestroyAvatar(CAvatar * a) override;

			virtual CElement *			CreateElement(CString const & name, CString const & type) override;
	};
}
