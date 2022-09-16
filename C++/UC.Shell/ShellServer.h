#pragma once
#include "Shell.h"
#include "ShellFriendProtocol.h"
#include "ShellLevel.h"
#include "FieldIcon.h"
#include "FieldWidget.h"
#include "FieldEnvironment.h"
#include "ThemeEnvironment.h"
#include "ThemeWidget.h"
#include "HudEnvironment.h"
#include "LinkIcon.h"
#include "LinkProperties.h"
#include "SystemMenuWidget.h"
#include "PictureWidget.h"
#include "PictureIcon.h"
#include "NotepadWidget.h"
#include "NotepadIcon.h"
#include "BoardWidget.h"
#include "BoardEnvironment.h"
#include "HistoryWidget.h"
#include "TrayWidget.h"

namespace uc
{
	class CShellServer : public CPersistentServer, public CShellLevel, public CShellProtocol, public CWorldFriendProtocol, public CShellFriendProtocol, public CAvatarProtocol, public IFieldProtocol, public CExecutorProtocol
	{
		using CShellLevel::Server;
		using CShellLevel::Nexus;
		using CShellLevel::Storage;
		
		public:
			CRectangleMenu *							Menu = null;

			UOS_RTTI
			CShellServer(CNexus * l, CServerInstance * si);
			~CShellServer();
			
			void										EstablishConnections();

			void										Initialize() override;
			void										Start() override;
			IProtocol * 								Accept(CString const & protocol);
			void										Break(IProtocol * s);
			CPersistentObject *							CreateObject(CString const & name) override;

			CString										GetTitle() override { return Server->Instance->Release->Address.Application; }
			IMenuSection *								CreateNewMenu(CFieldElement * f, CFloat3 & p, IMenu * m) override;
			
			CRefList<CMenuItem *>						CreateActions() override;

			virtual CInterObject *						GetEntity(CUol & a) override;
			virtual CList<CUol>							GenerateSupportedAvatars(CUol & e, CString const & type) override;
			virtual CAvatar *							CreateAvatar(CUol & o) override;
			virtual void								DestroyAvatar(CAvatar * a) override;

			CObject<CField>								FindField(CUol & o) override;
			CObject<CFieldEnvironment>					FindFieldEnvironmentByEntity(CUol & o) override;
			
			CObject<CFieldServer>						GetField(CUol & o) override;

			void virtual								Execute(CXon * arguments, CExecutionParameters * parameters) override;

			void										OnWorldSphereMouse(CActive *, CActive *, CMouseArgs *);
	};

	class CShellClient : public CClient, public virtual IType
	{
		public:
			CShellServer * Server;

			UOS_RTTI
			CShellClient(CNexus * nexus, CClientInstance * instance, CShellServer * server) : CClient(instance)
			{
				Server = server;
			}

			virtual ~CShellClient()
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
