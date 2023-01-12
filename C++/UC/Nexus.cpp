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

	auto systemstart =	[this](CXon * config)
						{
							for(auto i : config->One(L"Servers")->Nodes)
							{
								AddServer(	CApplicationReleaseAddress::Parse(i->Get<CString>(L"Address")), 
											i->Name, 
											Core->Commands->One(i->Name),
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

								s->Instance->SystemStart();

								if(i->Name == Sun0)
								{
									Sun = Connect<CSunProtocol>(null, Sun0);

									Core->RunThread(L"Sun", [&]
															{
																Sun->GetSettings(	[&](CSunSettings & s)
																					{
																						Sun->QueryRelease(	GetLatestReleases(),
																											[](auto & j)
																											{
																												int i = 0;
																											},
																											[]{});
																					});
															}, 
															[]{});
								}
							}
						};

	systemstart(SystemConfig);

	auto fss = Servers.Find([](auto i){ return i->Name == FileSystem0; });
	FileSystem = Connect<CFileSystemProtocol>(null, FileSystem0);

	//FileSystem = Connect<CFileSystemProtocol>(null, FileSystem0);

	auto id = new CIdentity();
	id->Provider = new CUserStorageProvider();
	auto user = id->ToString();

	//auto lp = Servers.Find([&](auto j){ return j->Name == FileSystem0; });
	auto & lpa = fss->Release->Address;
	lpa.Application = CLocalFileSystemProvider::GetClassName();
	FileSystem->Mount(CFileSystemProtocol::UserLocal,	lpa, &CTonDocument(CXonTextReader(L"To=" + Core->MapPath(ESystemPath::Users, user + L".local"))));
	FileSystem->Mount(CFileSystemProtocol::UserGlobal,	lpa, &CTonDocument(CXonTextReader(L"To=" + Core->MapPath(ESystemPath::Users, user + L".global"))));
	FileSystem->Mount(CFileSystemProtocol::UserTmp,		lpa, &CTonDocument(CXonTextReader(L"To=" + Core->MapPath(ESystemPath::Tmp, user))));

	d = Core->Resolve(Core->MapPath(ESystemPath::Core, User_nexus));
	c = FileSystem->UniversalToNative(CPath::Join(CFileSystemProtocol::UserGlobal, User_nexus));

	UserConfig = new CTonDocument(CXonTextReader(&CLocalFileStream(CNativePath::IsFile(c) ? c : d, EFileMode::Open)));

	systemstart(UserConfig);

	Identity = id;

	for(auto i : Servers)
	{
		i->Instance->UserStart();
	}

	Core->Log->ReportMessage(this, L"Nexus created");
	Core->LevelCreated(2, this);	
	Core->RegisterExecutor(this);
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

CList<CReleaseAddress> CNexus::GetLatestReleases()
{
	CList<CReleaseAddress> r;

	for(auto & rlz : CNativeDirectory::Enumerate(Core->Resolve(Core->MapPath(ESystemPath::Software, L".")), L".+-.+-.+", DirectoriesOnly))
		for(auto & v : CNativeDirectory::Enumerate(Core->Resolve(Core->MapPath(ESystemPath::Software, rlz.Name)), L".*", DirectoriesOnly).OrderBy([](auto & a, auto & b){ return CVersion(a.Name) > CVersion(b.Name); }))
		{
			auto a = rlz.Name.Split(L"-");
			r.push_back(CReleaseAddress(a[0], a[1], a[2], CVersion(v.Name)));
		}

	return r;
}

CApplicationRelease * CNexus::LoadRelease(CApplicationReleaseAddress & address, bool server)
{
	std::function<CCompiledManifest *(CReleaseAddress & a)> loadmanifest;

	loadmanifest =	[&](CReleaseAddress & a) -> CCompiledManifest *
					{
						auto r = Manifests.Find([&](auto i){ return i->Address == a; });

						if(!r)
						{
							r = new CCompiledManifest(a);

							auto deps = Core->Resolve(MapPathToRealization(a, a.Version.ToString() + L".dependencies"));

							if(CNativePath::IsFile(deps))
							{
								auto & m = CTonDocument(CXonTextReader(&CLocalFileStream(deps, EFileMode::Open)));
	
	
								r->Dependencies = m.Nodes.SelectArray<CCompiledManifest *>([&](auto i){ return loadmanifest(CReleaseAddress::Parse(i->Name)); });
							
							}

							Manifests.AddBack(r);
						}

						return r;
					};

	auto r = new CApplicationRelease();
	r->Address	= address;
	r->Manifest	= loadmanifest(r->Address);

	auto & reg = Core->Resolve(MapPathToRelease(r->Address, r->Address.Application + (server ? L".server" : L".client")));

	//if(CNativePath::IsFile(reg))
	{
		r->Registry = new CTonDocument(CXonTextReader(&CLocalFileStream(reg, EFileMode::Open))); 
	}

	if(server)
		ServerReleases.push_back(r);
	else
		ClientReleases.push_back(r);

	return r;
}

CApplicationRelease * CNexus::GetRelease(CApplicationReleaseAddress & address, bool server)
{
	auto r = server ? ServerReleases.Find([&](auto i){ return i->Address == address; }) : ClientReleases.Find([&](auto i){ return i->Address == address; });

	if(r)
		return r;

	return LoadRelease(address, server);
}

CServerInstance * CNexus::AddServer(CApplicationReleaseAddress & address, CString const & instance, CXon * command, CXon * registration)
{
	auto r = GetRelease(address, true);

	auto s = new CServerInstance();

	s->Name			= instance;
	s->Release		= r;
	s->Identity		= Identity;
	s->Command		= command ? command->CloneInternal(null) : null;
	s->Registration	= registration;
	//s->Initialized	= registration ? registration->Any(L"Initialized") : false;

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

CClientInstance * CNexus::GetClient(CApplicationReleaseAddress & address, CString const & instance)
{
	auto c = Clients.Find([&](auto i){ return i->Release->Address == address && i->Name == instance; });

	if(c)
		return c;

	auto r = GetRelease(address, false);

	c = new CClientInstance();

	c->Name			= instance;
	c->Release		= r;
	c->Identity		= Identity;

	if(auto i = r->Registry->One(L"Implementer"))
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

	if(r->Module == null)
	{
		auto exe = MapPathToRelease(r->Address, r->Registry->Get<CString>(L"Executable"));
		
		SetDllDirectories(r);
		
		r->Module = LoadLibrary(exe.c_str());
		
		if(r->Module == null)
		{
			throw CLastErrorException(HERE, GetLastError(), L"DLL loading error: %s ", exe.c_str());
		}
	}
		
	auto f = (FCreateUosClient)GetProcAddress(r->Module, "CreateUosClient");
		
	//if(r->CreateClient == null)
	//{
	//	FreeLibrary(r->ClientModule);
	//	throw CException(HERE, CString::Format(L"Interface not found: %s", l));
	//}
	
	c->Instance = f(this, c);
	
	if(c->Instance == null)
	{
		throw CException(HERE, CString::Format(L"Unable to create client: %s", c->Name));
	}
	
	Clients.push_back(c);

	return c;
}

void CNexus::Stop()
{
	Stopping();

// 	while(auto c = Clients.Last([&](auto i){ return i->Identity != null; }))
// 		Stop(c);

	while(auto s = Servers.Last([&](auto s){ return s->Identity != null; }))
		Stop(s);

	UserConfig->Save(&CXonTextWriter(&CLocalFileStream(FileSystem->UniversalToNative(CPath::Join(CFileSystemProtocol::UserGlobal, User_nexus)), EFileMode::New), false));
	delete UserConfig;

	delete Identity->Provider;
	delete Identity;

	Disconnect(Sun);
	Disconnect(FileSystem);

//	while(auto c = Clients.Last())
//		Stop(c);

	while(auto s = Servers.Last())
		Stop(s);

	Servers.clear();
	Clients.clear();

	for(auto i : ClientReleases)
	{
		if(i->Module)
		{
			#ifndef _DEBUG
			FreeLibrary(i->Module);
			#endif
		}
		delete i;
	}

	ClientReleases.clear();

	for(auto i : ServerReleases)
	{
		if(i->Module)
		{
			#ifndef _DEBUG
			FreeLibrary(i->Module);
			#endif
		}
		delete i;
	}

	ServerReleases.clear();

	for(auto i : Manifests)
		delete i;

	Manifests.clear();
}

void CNexus::Stop(CServerInstance * s)
{
	for(auto i : Clients.Where([&](auto i){ return i->Name == s->Name; }))
	{
		Stop(i);
	}

	if(s->Instance)
	{
		if(s->Release->Module)
		{
			auto f = (FDestroyUosServer)GetProcAddress(s->Release->Module, "DestroyUosServer");
			f(s->Instance);
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
			Break(c, i.first);
		}
	}

	auto f = (FDestroyUosClient)GetProcAddress(c->Release->Module, "DestroyUosClient");
	f(c->Instance);

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

	auto add =	[&](const auto & self, CCompiledManifest * r) -> void
				{
					auto d = MapPathToRelease(r->Address, L"");
					
					path += L";" + d;
					//addDllDirectory(d.c_str());

					for(auto i : r->Dependencies)
					{
						self(self ,i);
					}
				};

	add(add, info->Manifest);

	SetEnvironmentVariable(L"PATH", path.data());

	//FreeLibrary(l);
}

void CNexus::Instantiate(CServerInstance * si)
{
	auto r = si->Release;

	if(si->Instance == null)
	{
		auto exe = r->Registry->Get<CString>(L"Executable");

		if(CPath::GetExtension(exe) == L"dll")
		{
			if(r->Module == null)
			{
				auto l = MapPathToRelease(r->Address, exe);
			
				SetDllDirectories(r);
			
				r->Module = LoadLibrary(l.c_str());
			
				if(r->Module == null)
				{
					throw CLastErrorException(HERE, GetLastError(), L"DLL loading error: %s ", l.c_str());
				}
			}

			auto f = (FCreateUosServer)GetProcAddress(r->Module, "CreateUosServer");

			si->Instance = f(this, si);
		
			if(si->Instance == null)
			{
				throw CException(HERE, CString::Format(L"Unable to create server: %s", si->Name));
			}
		} 
		else if(CPath::GetExtension(exe) == L"exe")
		{
			si->Instance = new CServerProcess(this, si);
		}
	}

	//if(!si->Initialized)
	//{
	//	si->Instance->Initialize();
	//		
	//	if(si->Registration)
	//	{
	//		si->Registration->Add(L"Initialized");
	//	}
	//
	//	si->Initialized = true;
	//}
}

CClientConnection * CNexus::Connect(CApplicationRelease * who, CClientInstance * client, CString const & iface, std::function<void()> ondisconnect)
{
	if(client->Interfaces.Contains(iface))
	{
		if(client->Interfaces(iface) == null)
		{
			if(auto imp = client->Instance->Connect(iface))
			{
				client->Interfaces[iface] = imp;
				Core->Log->ReportMessage(this, L"Client connected: %-30s -> %-30s -> %-30s -> %-30s", who ? who->Address.ToString() : L"", iface, client->Name, client->Release->Address.ToString());
			}
		}
	
		if(client->Interfaces(iface) != null)
		{
			auto c = new CClientConnection(who, client->Instance, iface, ondisconnect);
			client->Users[iface].push_back(c);
			return c;
		}
	}
	
	return null;
}

CClientConnection * CNexus::Connect(CApplicationRelease * who, CString const & server, CString const & iface, std::function<void()> ondisconnect)
{
	auto s = Servers.Find([&](auto i){ return i->Name == server; });

	if(!s->Instance)
	{
		Instantiate(s);
	}

	auto c = Clients.Find([&](auto i){ return i->Name == server && i->Release->Registry->One(L"Implementer")->Any(iface); });

	if(!c)
	{
		//auto r = ClientReleases.Find([&](auto i){ return i->Registry->One(L"Interfaces")->Any(iface); });
		for(auto & rlz : CNativeDirectory::Enumerate(Core->Resolve(Core->MapPath(ESystemPath::Software, L".")), L".+-.+-.+", DirectoriesOnly))
			for(auto & v : CNativeDirectory::Enumerate(Core->Resolve(Core->MapPath(ESystemPath::Software, rlz.Name)), L".*", DirectoriesOnly).OrderBy([](auto & a, auto & b){ return CVersion(a.Name) > CVersion(b.Name); }))
				for(auto & cl : CNativeDirectory::Enumerate(Core->Resolve(Core->MapPath(ESystemPath::Software, CNativePath::Join(rlz.Name, v.Name))), L".+\\.client", FilesOnly))
				{
					auto a = rlz.Name.Split(L"-");
					auto r = LoadRelease(CApplicationReleaseAddress(a[0], a[1], a[2], CVersion(v.Name), CNativePath::GetFileNameBase(cl.Name)), false);

					if(r->Registry->One(L"Implementer")->Any(iface))
					{
						c = GetClient(r->Address, s->Name);
						goto found;
					}
				}
	}

found:
	if(!c)
	{
		auto r = GetRelease(CApplicationReleaseAddress::Parse(who->Registry->Get<CString>(L"Requirements/" + iface)), false);
		c = GetClient(r->Address, s->Name);
	}

	if(c)
		return Connect(who, c, iface, ondisconnect);

	return null;
}

CList<CClientConnection *> CNexus::ConnectMany(CApplicationRelease * who, CString const & iface)
{
	if(iface.empty())
	{
		throw CException(HERE, L"Protocol must be specified");
	}

	auto releases = FindImplementators(iface);

	CList<CClientConnection *> cc;

	for(auto s : releases)
		cc.push_back(Connect(who, s->Name, iface));
	
	return cc;
}

void CNexus::Disconnect(CClientConnection * c)
{
	c->Client->Instance->Users[c->ProtocolName].Remove(c);
	
	delete c;
}

void CNexus::Disconnect(CList<CClientConnection *> & cc)
{
	for(auto c : cc)
	{
		Disconnect(c);
	}
}

void CNexus::Break(CClientInstance * client, CString const & iface)
{
	auto i = client->Users(iface).begin();

	while(i != client->Users(iface).end())
	{
		auto c = *i;

		if(c->OnDisconnecting)
			c->OnDisconnecting();

		if(client->Users(iface).Contains(c))
		{
			Disconnect(c);
		}
			
		i = client->Users(iface).begin();
	}

	client->Instance->Disconnect(client->Interfaces(iface));
	
	client->Interfaces[iface] = null;
}

CList<CServerInstance *> CNexus::FindImplementators(CString const & iface)
{
	return Servers.Where(	[&](auto i)
							{
								if(auto x = i->Release->Registry->One(L"Implementer"))
									return x->Any(iface); 
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

			if(auto e = Connect<CExecutorProtocol>(null, o.Server))
			{
				e->Execute(command, parameters);
				Disconnect(e);
			}
		}
	}
	else if(Servers.Has([&](auto i){ return i->Name == command->Name; }))
		if(auto e = Connect<CExecutorProtocol>(null, command->Name))
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
