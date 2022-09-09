#include "StdAfx.h"
#include "Nexus.h"
#include "LocalFileSystemProvider.h"
#include "ServerProcess.h"

using namespace uc;

CNexus::CNexus(CCore * l, CXonDocument * config)
{
	Core = l;

	auto d = Core->Resolve(Core->MapPath(ESystemPath::Core, SystemNexusFile));
	auto c = Core->MapPath(ESystemPath::System, SystemNexusFile);

	SystemConfig = new CTonDocument(CXonTextReader(&CLocalFileStream(CNativePath::IsFile(c) ? c : d, EFileMode::Open)));

	Diagnostic = Core->Supervisor->CreateDiagnostics(GetClassName());
	Diagnostic->Updating += ThisHandler(OnDiagnosticUpdating);
	
	wchar_t b[32767];
	GetEnvironmentVariable(L"PATH", b, _countof(b));
	InitialPATH = b; // Core->GetPathTo(ESystemPath::Common, L".") + L";" + CString(b);
	//SetEnvironmentVariable(L"PATH", InitialPATH.c_str());		

	ExitHotKeyId	= Core->RegisterGlobalHotKey(MOD_ALT|MOD_CONTROL,			VK_ESCAPE, ThisHandler(ProcessHotKey));
	SuspendHotKeyId	= Core->RegisterGlobalHotKey(MOD_ALT|MOD_CONTROL|MOD_SHIFT,	VK_ESCAPE, ThisHandler(ProcessHotKey));

	ObjectTemplatePath	= Core->MapPath(ESystemPath::Core, L"Object.xon");

	StartServers();
	
	Core->RegisterExecutor(this);

	Initialized();
}

CNexus::~CNexus()
{
	Stop();

	SystemConfig->Save(&CXonTextWriter(&CLocalFileStream(Core->MapPath(ESystemPath::System, SystemNexusFile), EFileMode::New), false));
	delete SystemConfig;

	Diagnostic->Updating -= ThisHandler(OnDiagnosticUpdating);
	Core->LevelDestroyed(2, this);
	Core->Log->ReportMessage(this, L"-------------------------------- Nexus destroyed --------------------------------");
}

void CNexus::OnDiagnosticUpdating(CDiagnosticUpdate & a)
{
	for(auto s : Servers)
	{
		Diagnostic->Add(s->Name);

		if(s->Instance)
		{
			for(auto o : s->Instance->Objects)
				Diagnostic->Add(CString::Format(L"  %-55s %3d", o->Url.Object, o->GetRefs()));
		}
			
		//Diagnostic->Append(CString::Format(L"%-s", CString::Join(o->Users, [](auto & i){ return CString::Format(L"%s(%d)", i.first, i.second.size()); }, L",") ));
	}
}

// CString CNexus::AddressToName(CRealizationAddress & release)
// {
// 	auto & r = release;
// 
// 	return r.Author + L"-" + r.Product + L"-" + r.Platform;
// }

CString CNexus::MapPathToRealization(CRealizationAddress & release, CString const & path)
{
	auto & r = release;

	return Core->MapPath(ESystemPath::Software, CNativePath::Join(r.Author + L"-" + r.Product + L"-" + r.Platform, path));
}

CString CNexus::MapPathToRelease(CReleaseAddress & release, CString const & path)
{
	return MapPathToRealization(release, CNativePath::Join(release.Version.ToString(), path));
}

CApplicationRelease * CNexus::LoadRelease(CApplicationAddress & address)
{
	std::function<CManifest *(CReleaseAddress & a)> loadmanifest;

	loadmanifest =	[&](CReleaseAddress & a) -> CManifest *
					{
						auto r = Manifests.Find([&](auto i){ return i->Address == a; });

						if(!r)
						{
							auto & xon = CTonDocument(CXonTextReader(&CLocalFileStream(Core->Resolve(MapPathToRealization(a, a.Version.ToString() + L".manifest")), EFileMode::Open)));

							r = new CManifest(a, xon);

							if(auto d = xon.One(L"CompleteDependencies"))
							{
								r->CompleteDependencies = d->Nodes.Select<CManifest *>([&](auto i){ return loadmanifest(CReleaseAddress::Parse(i->Name)); });
							}
						
							Manifests.AddBack(r);
						}

						return r;
					};

	auto r = new CApplicationRelease();
	r->Address	= address;
	r->Manifest	= loadmanifest(r->Address);

	auto & reg = Core->Resolve(MapPathToRelease(r->Address, r->Address.Application + L".application"));

	if(CNativePath::IsFile(reg))
	{
		r->Registry = new CTonDocument(CXonTextReader(&CLocalFileStream(reg, EFileMode::Open))); 
	}

	Releases.push_back(r);

	return r;
}

CServerInstance * CNexus::AddServer(CApplicationAddress & address, CString const & instance, CXon * command, CXon * registration)
{
	auto r = Releases.Find([&](auto i){ return i->Address == address; });

	if(!r)
	{
		r = LoadRelease(address);
	}

	auto s = new CServerInstance();

	s->Name			= instance;
	s->Release		= r;
	s->Identity		= Identity;
	s->Command		= command ? command->CloneInternal(null) : null;
	s->Registration	= registration;
	s->Initialized	= registration ? registration->Any(L"Initialized") : false;

///	if(auto i = r->Registry->One(L"Interfaces"))
///	{
///		for(auto j : i->Nodes)
///		{
///			s->Interfaces[j->Name] = null;
///
/// 			if(j->Name == IExecutor::InterfaceName)
/// 			{
/// 				for(auto sheme : j->Nodes)
/// 				{
/// 					Core->Os->RegisterUrlProtocol(sheme->Name, Core->FrameworkDirectory, Core->CoreExePath + L" {" + CCore::OpenDirective + L" " + CCore::UrlArgument + L"=\"%1\"}");
/// 				}
/// 			}
///
///		}
///	}

	Servers.push_back(s);

	Core->Log->ReportMessage(this, CString::Format(L"Server started: %s", instance));

	return s;
}

CClientInstance * CNexus::AddClient(CApplicationAddress & address, CString const & instance)
{
	auto r = Releases.Find([&](auto i){ return i->Address == address; });

	if(!r)
	{
		r = LoadRelease(address);
	}

	auto c = new CClientInstance();

	c->Name			= instance;
	c->Release		= r;
	c->Identity		= Identity;

	if(auto i = r->Registry->One(L"Interfaces"))
	{
		for(auto j : i->Nodes)
		{
			c->Interfaces[j->Name] = null;

/// 			if(j->Name == IExecutor::InterfaceName)
/// 			{
/// 				for(auto sheme : j->Nodes)
/// 				{
/// 					Core->Os->RegisterUrlProtocol(sheme->Name, Core->FrameworkDirectory, Core->CoreExePath + L" {" + CCore::OpenDirective + L" " + CCore::UrlArgument + L"=\"%1\"}");
/// 				}
/// 			}

		}
	}

	if(r->ClientModule == null)
	{
		auto l = MapPathToRelease(r->Address, r->Registry->Get<CString>(L"Client"));
		
		if(!CNativePath::IsFile(l))
		{
			throw CException(HERE, CString::Format(L"Client dll not found: %s", l));
		}
		
		SetDllDirectories(r);
		
		r->ClientModule = LoadLibrary(l.c_str());
		
		if(r->ClientModule == null)
		{
			throw CLastErrorException(HERE, GetLastError(), L"DLL loading error: %s ", l.c_str());
		}
		
		r->CreateClient = (FCreateUosClient)GetProcAddress(r->ClientModule, "CreateUosClient");
		
		if(r->CreateClient == null)
		{
			FreeLibrary(r->ClientModule);
			throw CException(HERE, CString::Format(L"Interface not found: %s", l));
		}
	}
	
	c->Instance = r->CreateClient(this, c);
	
	if(c->Instance == null)
	{
		throw CException(HERE, CString::Format(L"Unable to create client: %s", c->Name));
	}
	
	Clients.push_back(c);

	return c;
}

void CNexus::StartServers()
{
	auto start = [this](CXon * config)
				 {
					for(auto i : config->One(L"Servers")->Nodes)
					{
						AddServer(	CApplicationAddress::Parse(i->Get<CString>(L"Address")), 
									i->Name, 
									Core->Commands->Nodes.Find([&](auto j){ return j->Name == i->Name; }),
									i);
					}

					for(auto i : config->One(L"Start")->Nodes)
					{
						auto s = Servers.Find([&](auto j){ return j->Name == i->Name; });

						if(!s->Command)
						{
							s->Command = i->Any(L"Command") ? i->One(L"Command")->CloneInternal(null) : null;
						}
			
						Instantiate(s);

						s->Instance->Start();
					}
				};

	start(SystemConfig);

	FileSystem = Connect<IFileSystem>(this);
	CString user = L"User";
	auto lp = Releases.Find([&](auto i){ return i->Address.Application == CLocalFileSystemProvider::Name; });
	FileSystem->Mount(IFileSystem::UserLocal,	lp->Address, &CTonDocument(CXonTextReader(L"To=" + Core->MapPath(ESystemPath::Users, user + L".local"))));
	FileSystem->Mount(IFileSystem::UserGlobal,	lp->Address, &CTonDocument(CXonTextReader(L"To=" + Core->MapPath(ESystemPath::Users, user + L".global"))));
	FileSystem->Mount(IFileSystem::UserTmp,		lp->Address, &CTonDocument(CXonTextReader(L"To=" + Core->MapPath(ESystemPath::Tmp, user))));

	Identity = new CIdentity();

	auto d = Core->Resolve(Core->MapPath(ESystemPath::Core, UserNexusFile));
	auto c = FileSystem->UniversalToNative(CPath::Join(IFileSystem::UserGlobal, UserNexusFile));

	UserConfig = new CTonDocument(CXonTextReader(&CLocalFileStream(CNativePath::IsFile(c) ? c : d, EFileMode::Open)));

	start(UserConfig);

	Core->Log->ReportMessage(this, L"-------------------------------- Nexus created --------------------------------");
	Core->LevelCreated(2, this);
}

void CNexus::Stop()
{
	Stopping();

	while(auto c = Clients.Last([&](auto i){ return i->Identity != null; }))
		Stop(c);

	while(auto s = Servers.Last([&](auto s){ return s->Identity != null; }))
		Stop(s);

	UserConfig->Save(&CXonTextWriter(&CLocalFileStream(FileSystem->UniversalToNative(CPath::Join(IFileSystem::UserGlobal, UserNexusFile)), EFileMode::New), false));
	delete UserConfig;

	delete Identity;

	Disconnect(FileSystem);

	while(auto c = Clients.Last())
		Stop(c);

	while(auto s = Servers.Last())
		Stop(s);

	Servers.clear();

	for(auto i : Releases)
	{
		if(i->ServerModule)
		{
			#ifndef _DEBUG
			FreeLibrary(i->ServerModule);
			#endif
		}
		if(i->ClientModule)
		{
			#ifndef _DEBUG
			FreeLibrary(i->ClientModule);
			#endif
		}
		delete i;
	}

	Releases.clear();

	for(auto i : Manifests)
		delete i;

	Manifests.clear();
}

void CNexus::Stop(CServerInstance * s)
{
	if(s->Instance)
	{
		if(s->Release->ServerModule)
		{
			auto stop = (FDestroyUosServer)GetProcAddress(s->Release->ServerModule, "DestroyUosServer");
			stop(s->Instance);
		} 
		else
		{
			s->Instance->As<CServerProcess>()->Send(CStopMessage());
			delete s->Instance;
		}
	}
	
	delete s->Command;
	delete s;
	Servers.Remove(s);
}

void CNexus::Stop(CClientInstance * c)
{
	for(auto & i : c->Interfaces)
	{
		if(i.second)
		{
			Break(c->Name, i.first);
		}
	}

	auto stop = (FDestroyUosClient)GetProcAddress(c->Release->ClientModule, "DestroyUosClient");
	stop(c->Instance);

	delete c;
	Clients.Remove(c);
}

void CNexus::Restart(CString const & cmd)
{
	RestartCommand = cmd;
}

void CNexus::SetDllDirectories(CApplicationRelease * info)
{
	auto path = InitialPATH;

	//auto l = LoadLibrary(L"Kernel32.dll");
	//auto addDllDirectory = (DLL_DIRECTORY_COOKIE (*)(PCWSTR))GetProcAddress(l, "AddDllDirectory");
	//SetDefaultDllDirectories(LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);

	auto add =	[&](const auto & self, CManifest * r) -> void
				{
					auto d = MapPathToRelease(r->Address, L"");
					
					path += L";" + d;
					//addDllDirectory(d.c_str());

					for(auto i : r->CompleteDependencies)
					{
						self(self ,i);
					}
				};

	add(add, info->Manifest);

	SetEnvironmentVariable(L"PATH", path.data());

	//FreeLibrary(l);
}

CClientInstance * CNexus::GetClient(CString const & instance)
{
	auto c = Clients.Find([&](auto i){ return i->Name == instance; });

	if(c)
		return c;

	if(auto s = Servers.Find([&](auto i){ return i->Name == instance; }))
	{
		if(!s->Instance)
		{
			Instantiate(s);
		}
	
		return AddClient(s->Release->Address, s->Name);
	}

	return null;
}

void CNexus::Instantiate(CServerInstance * si)
{
	auto r = si->Release;

	if(si->Instance == null)
	{
		auto bin = r->Registry->Get<CString>(L"Server");

		if(CPath::GetExtension(bin) == L"dll")
		{
			if(r->ServerModule == null)
			{
				auto l = MapPathToRelease(r->Address, r->Registry->Get<CString>(L"Server"));
			
				if(!CNativePath::IsFile(l))
				{
					throw CException(HERE, CString::Format(L"Server dll not found: %s", l));
				}
			
				SetDllDirectories(r);
			
				r->ServerModule = LoadLibrary(l.c_str());
			
				if(r->ServerModule == null)
				{
					throw CLastErrorException(HERE, GetLastError(), L"DLL loading error: %s ", l.c_str());
				}
			
				r->CreateServer = (FCreateUosServer)GetProcAddress(r->ServerModule, "CreateUosServer");
			
				if(r->CreateServer == null)
				{
					FreeLibrary(r->ServerModule);
					throw CException(HERE, CString::Format(L"Interface not found: %s", l));
				}
			}
		
			si->Instance = r->CreateServer(this, si);
		
			if(si->Instance == null)
			{
				throw CException(HERE, CString::Format(L"Unable to create server: %s", si->Name));
			}
		} 
		else if(CPath::GetExtension(bin) == L"exe")
		{
			si->Instance = new CServerProcess(this, si);
		}
	}

	if(!si->Initialized)
	{
		si->Instance->Initialize();
			
		if(si->Registration)
		{
			si->Registration->Add(L"Initialized");
		}

		si->Initialized = true;
	}
}

CConnection CNexus::Connect(IType * who, CClientInstance * si, CString const & iface, std::function<void()> ondisconnect)
{
	if(si->Interfaces.Contains(iface))
	{
		if(si->Interfaces(iface) == null)
		{
			if(auto imp = si->Instance->Connect(iface))
			{
				si->Interfaces[iface] = imp;
				Core->Log->ReportMessage(this, L"Client connected: %-30s -> %-30s -> %-30s -> %-30s", who->GetInstanceName(), iface, si->Name, si->Release->Address.ToString());
			}
		}
	
		if(si->Interfaces(iface) != null)
		{
			si->Users[iface][who] = ondisconnect;
			return CConnection(who, si->Instance, iface);
		}
	}
	
	return CConnection();
}

CConnection CNexus::Connect(IType * who, CString const & instance, CString const & iface, std::function<void()> ondisconnect)
{
	auto si = GetClient(instance);

	if(!si)
		return CConnection();

	return Connect(who, si, iface, ondisconnect);
}

// CConnection CNexus::Connect(IType * who, CApplicationAddress & server, CString const & iface, std::function<void()> ondisconnect)
// {
// 	auto si = GetClient(server);
// 
// 	if(!si)
// 		return CConnection();
// 
// 	return Connect(who, si, iface, ondisconnect);
// }

CConnection CNexus::Connect(IType * who, CString const & iface, std::function<void()> ondisconnect)
{
	auto o = FindImplementators(iface);

	if(!o.empty())
		return Connect(who, o.front()->Name, iface, ondisconnect);
	else
		return CConnection();
}

CList<CConnection> CNexus::ConnectMany(IType * who, CString const & iface)
{
	if(iface.empty())
	{
		throw CException(HERE, L"Protocol must be specified");
	}

	auto releases = FindImplementators(iface);

	CList<CConnection> cc;

	for(auto s : releases)
		cc.push_back(Connect(who, s->Name, iface));
	
	return cc;
}

void CNexus::Disconnect(CConnection & c)
{
	c.Client->Instance->Users[c.ProtocolName].Remove(c.Who);
		
	c.Protocol = null;
	c.Client = null;
	c.Who = null;
}

void CNexus::Disconnect(CList<CConnection> & cc)
{
	for(auto c : cc)
	{
		Disconnect(c);
	}
}

void CNexus::Break(CString const & instance, CString const & iface)
{
	auto c = Clients.Find([&](auto i){ return i->Name == instance; });

	if(c->Interfaces.Contains(iface) && c->Interfaces[iface] != null)
	{
		//s->Disconnecting(s, s->Protocols[protocol], const_cast<CString &>(protocol));

		auto i = c->Users[iface].begin();

		while(i != c->Users[iface].end())
		{
			auto k = i->first;

			if(i->second)
				i->second();

			if(c->Users[iface].Contains(k))
			{
				//Core->Log->ReportWarning(this, L"%s disconnected improperly from %s -> %s", k->GetInstanceName(), s->Url.Server, pr);
				c->Users[iface].Remove(k);
			}
			
			i = c->Users[iface].begin();
		}

		c->Instance->Disconnect(c->Interfaces[iface]);
	
		c->Interfaces[iface] = null;
	}
}

CList<CServerInstance *> CNexus::FindImplementators(CString const & iface)
{
	return Servers.Where(	[&](auto i)
							{
								if(auto x = i->Release->Registry->One(L"Interfaces"))
								{
									return x->Any(iface); 
								}
								else
									return false;
							});
}

void CNexus::ProcessHotKey(int64_t id)
{
	if(id == ExitHotKeyId)
	{
		Core->Exit();
	}
	if(id == SuspendHotKeyId)
	{
		if(Core->SuspendStatus)
		{
			Core->Resume();
		}
		else
		{
			Core->Suspend();
		}
	}
}

void CNexus::Execute(CXon * command, CExecutionParameters * parameters)
{
	if(command->Name.empty())
	{
		if(command->Nodes.First()->Name == CCore::OpenDirective && command->Any(CCore::UrlArgument))
		{
			auto u = command->Get<CString>(CCore::UrlArgument);

			auto o = CUol(u);

			if(auto e = Connect<IExecutor>(this, o.Server))
			{
				e->Execute(command, parameters);
				Disconnect(e);
			}
		}
	}
	else if(auto e = Connect<IExecutor>(this, command->Name))
	{
		e->Execute(command, parameters);
		Disconnect(e);
	}
}

CXon * CNexus::QueryRegistry(CString const & instance, CString const & path)
{
	auto info = Servers.Find([&](auto i){ return i->Name == instance; });

	return info ? info->Release->Registry->One(path) : null;
}

CMap<CString, CXon *> CNexus::QueryRegistry(CString const & path)
{
	CMap<CString, CXon *> l;

	for(auto i : Servers)
	{
		if(auto r = QueryRegistry(i->Name, path))
		{
			l[i->Name] = r;
		}
	}

	return l;
}
