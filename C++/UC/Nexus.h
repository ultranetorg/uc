#pragma once
#include "Connection.h"
#include "Base58.h"
#include "ExecutorProtocol.h"
#include "ILevel.h"
#include "Server.h"
#include "Client.h"
#include "FileSystemProtocol.h"
#include "Sun.h"

namespace uc
{
	class UOS_LINKING CNexus : public ILevel, public IType, public ICommandExecutor
	{
		public:
			CCore *										Core;

			bool										Primary = true;

			int											ExitHotKeyId;
			int											SuspendHotKeyId;
			
			CList<CManifest *>							Manifests;
			CList<CApplicationRelease *>				ServerReleases;
			CList<CApplicationRelease *>				ClientReleases;
			CList<CServerInstance *>					Servers;
			CList<CClientInstance *>					Clients;
			CProtocolConnection<CFileSystemProtocol>			FileSystem;
			CProtocolConnection<CSunProtocol>					Sun;
			CIdentity *									Identity = null;

			CXonDocument *								SystemConfig = null;
			CXonDocument *								UserConfig = null;
			CDiagnostic *								Diagnostic;

			CString										ObjectTemplatePath;
			
			CEvent<>									Initialized;
			CEvent<>									Stopping;
			
			CString										RestartCommand;
			
			CString										InitialPATH;

			inline static const CString					SystemNexusFile = L"System.nexus";
			inline static const CString					UserNexusFile = L"User.nexus";

			inline static const CString					FileSystem0 = L"FileSystem0";
			inline static const CString					Sun0 = L"Sun0";
			inline static const CString					World0 = L"World0";
			inline static const CString					Shell0 = L"Shell0";

			UOS_RTTI
			CNexus(CCore * l, CXonDocument * config);
			~CNexus();

			void										Restart(CString const & cmd);

			void										ProcessHotKey(int64_t id);
			void										OnDiagnosticUpdating(CDiagnosticUpdate & a);
			void										StartServers();
			void										Stop();
			void										Stop(CServerInstance * s);
			void										Stop(CClientInstance * s);

			CString										MapPathToRelease(CReleaseAddress & release, CString const & path);
			CString										MapPathToRealization(CRealizationAddress & release, CString const & path = CString());

			CMap<CString, CXon *>						QueryRegistry(CString const & path);
			CXon *										QueryRegistry(CString const & instance, CString const & path);

			void										SetDllDirectories(CApplicationRelease * info);

			CApplicationRelease *						LoadRelease(CApplicationReleaseAddress & address, bool client);
			CApplicationRelease *						GetRelease(CApplicationReleaseAddress & address, bool server);
			CServerInstance *							AddServer(CApplicationReleaseAddress & address, CString const & instance, CXon * command, CXon * registration);
			CClientInstance *							GetClient(CApplicationReleaseAddress & address, CString const & instance);
			void										Instantiate(CServerInstance * si);
			//void										Instantiate(CClientInstance * si);
			//CClientInstance *							FindClient(CString const & instance);
			//CClientInstance *							GetClient(CString const & server);

			CClientConnection *							Connect(CApplicationRelease * who, CClientInstance * si, CString const & iface,	std::function<void()> ondisconnect = std::function<void()>());
			CClientConnection *							Connect(CApplicationRelease * who, CString const & instance, CString const & iface,	std::function<void()> ondisconnect = std::function<void()>());
			//CConnection								Connect(IType * who, CApplicationAddress & server, CString const & iface,	std::function<void()> ondisconnect = std::function<void()>());
			//CConnection 								Connect(IType * who, CString const & iface,	std::function<void()> ondisconnect = std::function<void()>() = std::function<void()>());
			CList<CClientConnection *> 					ConnectMany(CApplicationRelease * CApplicationRelease, CString const & iface);

			template<class T> CProtocolConnection<T>	Connect(CApplicationRelease * who, CString const & server, std::function<void()> ondisconnect = std::function<void()>())
														{
															auto c = Connect(who, server, T::InterfaceName, ondisconnect);

															return CProtocolConnection<T>(c);
														}

			template<class T> CProtocolConnection<T>	Connect(CApplicationRelease * who, CClientInstance * instance, std::function<void()> ondisconnect = std::function<void()>())
														{
															auto c = Connect(who, instance, T::InterfaceName, ondisconnect);

															return CProtocolConnection<T>(c);
														}
// 
// 			template<class P> CProtocolConnection<P>	Connect(IType * who, CString const & server, std::function<void()> ondisconnect = std::function<void()>())
// 														{
// 															return CProtocolConnection<P>(Connect(who, server, P::InterfaceName, ondisconnect));
// 														}

			template<class P> CProtocolConnection<P>	Connect(CApplicationRelease * who, CApplicationReleaseAddress & application, std::function<void()> ondisconnect = std::function<void()>())
														{
															return CProtocolConnection<P>(Connect(who, application, P::InterfaceName, ondisconnect));
														}

			template<class T> 
			CList<CProtocolConnection<T>>				ConnectMany(CApplicationRelease * who)
														{
															CList<CProtocolConnection<T>> cc;
															
															for(auto c : ConnectMany(who, T::InterfaceName))
															{
																cc.push_back(CProtocolConnection<T>(c));
															}
															
															return cc;
														}

			template<class T> void						Disconnect(CList<CProtocolConnection<T>> & cc)
														{
															for(auto & c : cc)
															{
																Disconnect(c);
															}
														}

			void										Disconnect(CClientConnection * c);
			void										Disconnect(CList<CClientConnection *> & c);

			//CSystem *									GetSystem(CUsl & u);

			CList<CServerInstance *>					FindImplementators(CString const & pr);

			void										Break(CClientInstance * client, CString const & iface);

		protected:
			void										Execute(CXon * arguments, CExecutionParameters * parameters);
	};
}