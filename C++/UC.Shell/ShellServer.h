#pragma once
#include "Shell.h"
#include "IShellFriend.h"
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
	class CShellServer : public CStorableServer, public CShellLevel, public CShell, public IWorldFriend, public IShellFriend, public IAvatarProtocol, public IFieldProtocol, public IExecutorProtocol
	{
		using CShellLevel::Server;
		using CShellLevel::Nexus;
		using CShellLevel::Storage;
		
		public:
			CRectangleMenu *							Menu = null;

			UOS_RTTI
			CShellServer(CNexus * l, CServerInfo * si);
			~CShellServer();
			
			void										EstablishConnections();

			void										Start(EStartMode sm) override;
			IProtocol * 								Connect(CString const & pr)override;
			void										Disconnect(IProtocol * s) override;
			CStorableObject *								CreateObject(CString const & name) override;

			CString										GetTitle() override { return SHELL_OBJECT; }
			IMenuSection *								CreateNewMenu(CFieldElement * f, CFloat3 & p, IMenu * m) override;
			
			CRefList<CMenuItem *>						CreateActions() override;

			virtual CInterObject *					GetEntity(CUol & a) override;
			virtual CList<CUol>							GenerateSupportedAvatars(CUol & e, CString const & type) override;
			virtual CAvatar *							CreateAvatar(CUol & o) override;
			virtual void								DestroyAvatar(CAvatar * a) override;

			CObject<CField>								FindField(CUol & o) override;
			CObject<CFieldEnvironment>					FindFieldEnvironmentByEntity(CUol & o) override;
			
			CObject<CFieldServer>						GetField(CUol & o) override;

			void virtual								Execute(const CUrq & o, CExecutionParameters * ep) override;
			bool virtual								CanExecute(const CUrq & o) override;

			void										OnWorldSphereMouse(CActive *, CActive *, CMouseArgs *);
	};
}
