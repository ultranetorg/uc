#pragma once
#include "Connection.h"
#include "Base58.h"
#include "IExecutor.h"
#include "ILevel.h"
#include "Server.h"
#include "Client.h"
#include "FileSystem.h"
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
			CList<CApplicationRelease *>				Releases;
			CList<CServerInstance *>					Servers;
			CList<CClientInstance *>					Clients;
			CProtocolConnection<CFileSystem>			FileSystem;
			CProtocolConnection<CSun>					Sun;
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

			CApplicationRelease *						LoadRelease(CApplicationAddress & address);
			CServerInstance *							AddServer(CApplicationAddress & address, CString const & instance, CXon * command, CXon * registration);
			CClientInstance *							AddClient(CApplicationAddress & address, CString const & instance);
			void										Instantiate(CServerInstance * si);
			//void										Instantiate(CClientInstance * si);
			//CClientInstance *							FindClient(CString const & instance);
			CClientInstance *							GetClient(CString const & instance);

			CConnection 								Connect(IType * who, CClientInstance * si, CString const & iface,			std::function<void()> ondisconnect = std::function<void()>());
			CConnection									Connect(IType * who, CString const & instance, CString const & iface,		std::function<void()> ondisconnect = std::function<void()>());
			//CConnection									Connect(IType * who, CApplicationAddress & server, CString const & iface,	std::function<void()> ondisconnect = std::function<void()>());
			CConnection 								Connect(IType * who, CString const & iface,									std::function<void()> ondisconnect = std::function<void()>() = std::function<void()>());
			CList<CConnection> 							ConnectMany(IType * who, CString const & iface);

			template<class T> CProtocolConnection<T>	Connect(IType * who, CString const & instance, std::function<void()> ondisconnect = std::function<void()>())
														{
															auto c = Connect(who, instance, T::InterfaceName, ondisconnect);

															if(c && c.As<T>() == null)
															{
																return CProtocolConnection<T>();
															}
															return CProtocolConnection<T>(c);
														}

			template<class T> CProtocolConnection<T>	Connect(IType * who, CClientInstance * instance, std::function<void()> ondisconnect = std::function<void()>())
														{
															auto c = Connect(who, instance, T::InterfaceName, ondisconnect);

															if(c && c.As<T>() == null)
															{
																return CProtocolConnection<T>();
															}
															return CProtocolConnection<T>(c);
														}

			template<class P> CProtocolConnection<P>	Connect(IType * who, std::function<void()> ondisconnect = std::function<void()>())
														{
															return CProtocolConnection<P>(Connect(who, P::InterfaceName, ondisconnect));
														}

			template<class P> CProtocolConnection<P>	Connect(IType * who, CApplicationAddress & application, std::function<void()> ondisconnect = std::function<void()>())
														{
															return CProtocolConnection<P>(Connect(who, application, P::InterfaceName, ondisconnect));
														}

			template<class T> 
			CList<CProtocolConnection<T>>				ConnectMany(IType * who)
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

			void										Disconnect(CConnection & c);
			void										Disconnect(CList<CConnection> & c);

			//CSystem *									GetSystem(CUsl & u);

			CList<CServerInstance *>					FindImplementators(CString const & pr);

			void										Break(CString const & server, CString const & iface);

		protected:
			void										Execute(CXon * arguments, CExecutionParameters * parameters);
	};
}