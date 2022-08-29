#include "StdAfx.h"
#include "Nexus.h"
#include "LocalFileSystemProvider.h"

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
	StopServers();

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
		Diagnostic->Add(s->Instance);

		for(auto o : s->Objects)
			Diagnostic->Add(CString::Format(L"  %-55s %3d", o->Url.Object, o->GetRefs()));
			
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

CServerRelease * CNexus::LoadRelease(CServerAddress & address)
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

	auto r = new CServerRelease();
	r->Address	= address;
	r->Manifest	= loadmanifest(r->Address);

	auto & reg = Core->Resolve(MapPathToRelease(r->Address, r->Address.Server + L".registry"));

	if(CNativePath::IsFile(reg))
	{
		r->Registry = new CTonDocument(CXonTextReader(&CLocalFileStream(reg, EFileMode::Open))); 
	}

	Releases.push_back(r);

	return r;
}

CServer * CNexus::CreateServer(CServerAddress & address, CString const & instance, CXon * command, CXon * registration)
{
	auto r = Releases.Find([&](auto i){ return i->Address == address; });

	if(!r)
	{
		r = LoadRelease(address);
	}

	//auto info = Servers.Find([&](auto i){ return i->Address == address; });

	if(r->HInstance == null)
	{
		auto l = MapPathToRelease(r->Address, r->Registry->Get<CString>(L"Executable"));
	
		if(!CNativePath::IsFile(l))
		{
			throw CException(HERE, CString::Format(L"Server dll not found: %s", l));
		}
	
		SetDllDirectories(r);
	
		r->HInstance = LoadLibrary(l.c_str());
	
		if(r->HInstance == null)
		{
			throw CLastErrorException(HERE, GetLastError(), L"DLL loading error: %s ", l.c_str());
		}
	
		r->StartUosServer = (FStartUosServer)GetProcAddress(r->HInstance, "StartUosServer");
	
		if(r->StartUosServer == null)
		{
			FreeLibrary(r->HInstance);
			throw CException(HERE, CString::Format(L"Interface not found: %s", l));
		}
	}

	auto rcmd = registration ? registration->One(L"Command") : null;
	auto cmd = command ? command : rcmd;

	auto s = r->StartUosServer(this, r, cmd);

	if(s == null)
	{
		throw CException(HERE, CString::Format(L"Unable to initilize server: %s", address.ToString()));
	}

	if(auto i = r->Registry->One(L"Interfaces"))
	{
		for(auto j : i->Nodes)
		{
			s->Interfaces[j->Name] = null;

			if(j->Name == IExecutor::InterfaceName)
			{
				for(auto sheme : j->Nodes)
				{
					Core->Os->RegisterUrlProtocol(sheme->Name, Core->FrameworkDirectory, Core->CoreExePath + L" " + CCore::GetClassName() + L"{" + CCore::OpenDirective + L" " + CCore::UrlArgument + L"=\"%1\"}");
				}
			}

		}
	}

	s->Instance		= instance;
	s->Identity		= Identity;
	s->Command		= cmd ? cmd->CloneInternal(null) : null;
	s->Registration	= registration;
	s->Initialized	= registration ? registration->Get<CBool>(L"Initialized") : false;

	Servers.push_back(s);


	Core->Log->ReportMessage(this, CString::Format(L"Server started: %s", instance));


	return s;
}

void CNexus::StartServers()
{
	for(auto i : SystemConfig->Many(L"Server"))
	{
		CreateServer(CServerAddress::Parse(i->Get<CString>(L"Address")), 
					i->Get<CString>(), 
					Core->Commands->Nodes.Find([&](auto j){ return j->Name == i->Get<CString>(); }),
					i);
	}

	for(auto i : SystemConfig->Many(L"Command"))
	{
		auto s = Servers.Find([&](auto j){ return j->Instance == i->Get<CString>(); });
		auto e = Connect<IExecutor>(this, s->Instance);
			
		if(e)
		{
			if(!s->Initialized)
			{
				if(auto c = s->Release->Registry->One(L"Installation/Command"))
				{
					e->Execute(c, null);
			
					if(s->Registration)
					{
						s->Registration->One(L"Initialized")->Set(true);
					}

					s->Initialized = true;
				}
			}

			e->Execute(i, null);
		
			Disconnect(e);
		}
	}


	FileSystem = Connect<IFileSystem>(this);
	CString user = L"User";
	auto lp = Releases.Find([&](auto i){ return i->Address.Server == L"LocalFileSystemProvider"; });
	FileSystem->Mount(IFileSystem::UserLocal,	lp->Address, &CTonDocument(CXonTextReader(L"To=" + Core->MapPath(ESystemPath::Users, user + L".local"))));
	FileSystem->Mount(IFileSystem::UserGlobal,	lp->Address, &CTonDocument(CXonTextReader(L"To=" + Core->MapPath(ESystemPath::Users, user + L".global"))));
	FileSystem->Mount(IFileSystem::UserTmp,		lp->Address, &CTonDocument(CXonTextReader(L"To=" + Core->MapPath(ESystemPath::Tmp, user))));

	Identity = new CIdentity();

	auto d = Core->Resolve(Core->MapPath(ESystemPath::Core, UserNexusFile));
	auto c = FileSystem->UniversalToNative(CPath::Join(IFileSystem::UserGlobal, UserNexusFile));

	UserConfig = new CTonDocument(CXonTextReader(&CLocalFileStream(CNativePath::IsFile(c) ? c : d, EFileMode::Open)));

	for(auto i : UserConfig->Nodes)
	{
		if(i->Name == L"Server")
		{
			CreateServer(CServerAddress::Parse(i->Get<CString>(L"Address")), 
						i->Get<CString>(), 
						Core->Commands->Nodes.Find([&](auto j){ return j->Name == i->Get<CString>(); }),
						i);
		}
	}

	for(auto i : UserConfig->Many(L"Command"))
	{
		auto s = Servers.Find([&](auto j){ return j->Instance == i->Get<CString>(); });
		auto e = Connect<IExecutor>(this, s->Instance);
			
		if(e)
		{
			if(!s->Initialized)
			{
				if(auto c = s->Release->Registry->One(L"Installation/Command"))
				{
					e->Execute(c, null);
			
					if(s->Registration)
					{
						s->Registration->One(L"Initialized")->Set(true);
					}

					s->Initialized = true;
				}
			}

			e->Execute(i, null);
		
			Disconnect(e);
		}
	}


	Core->Log->ReportMessage(this, L"-------------------------------- Nexus created --------------------------------");
	Core->LevelCreated(2, this);
}

void CNexus::StopServers()
{
	Stopping();

	while(auto s = Servers.Last([&](auto s){ return s->Identity != null; }))
		StopServer(s);

	UserConfig->Save(&CXonTextWriter(&CLocalFileStream(FileSystem->UniversalToNative(CPath::Join(IFileSystem::UserGlobal, UserNexusFile)), EFileMode::New), false));
	delete UserConfig;

	delete Identity;

	Disconnect(FileSystem);

	while(auto s = Servers.Last())
		StopServer(s);

	Servers.clear();

	for(auto i : Releases)
	{
		if(i->HInstance)
		{
			#ifndef _DEBUG
			FreeLibrary(i->HInstance);
			#endif
		}
		delete i;
	}

	Releases.clear();

	for(auto i : Manifests)
		delete i;

	Manifests.clear();
}

void CNexus::StopServer(CServer * s)
{
	for(auto & i : s->Interfaces)
	{
		if(i.second)
		{
			Break(s->Instance, i.first);
		}
	}

	//Infos.Remove(s);
	cleandelete(s->Command);

	auto stop = (FStopUosServer)GetProcAddress(s->Release->HInstance, "StopUosServer");
	stop(s);

	Servers.Remove(s);
}

void CNexus::Restart(CString const & cmd)
{
	RestartCommand = cmd;
}

void CNexus::SetDllDirectories(CServerRelease * info)
{
	CString path = InitialPATH;

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

CServer * CNexus::GetServer(CString const & instance)
{
	auto s = Servers.Find([&](auto i){ return i->Instance == instance; });

	if(s)
		return s;

	if(auto s = SystemConfig->Nodes.Find([&](auto i){ return i->Name == L"Server" && i->Get<CString>() == instance; }))
		return CreateServer(CServerAddress::Parse(s->Get<CString>(L"Address")), instance, Core->Commands->Nodes.Find([&](auto j){ return j->Name == s->Get<CString>(); }), s);

	if(auto u = UserConfig->Nodes.Find([&](auto i){ return i->Name == L"Server" && i->Get<CString>() == instance; }))
		return CreateServer(CServerAddress::Parse(u->Get<CString>(L"Address")), instance, Core->Commands->Nodes.Find([&](auto j){ return j->Name == u->Get<CString>(); }), u);

	return null;
}

CServer * CNexus::GetServer(CServerAddress & server)
{
	auto s = Servers.Find([&](auto i){ return i->Release->Address == server; });

	if(s)
		return s;

	return CreateServer(server, L"", null, null);
}

CConnection CNexus::Connect(IType * who, CString const & instance, CString const & iface, std::function<void()> ondisconnect)
{
	auto s = GetServer(instance);

	if(!s)
		return CConnection();

	if(s->Interfaces.Contains(iface))
	{
		if(s->Interfaces(iface) == null)
		{
			if(auto imp = s->Connect(iface))
			{
				s->Interfaces[iface] = imp;
				Core->Log->ReportMessage(this, L"Server connected: %-30s -> %-30s -> %-30s", who->GetInstanceName(), iface, s->Instance);
			}
		}
	
		if(s->Interfaces[iface] != null)
		{
			s->Users[iface][who] = ondisconnect;
			return CConnection(who, s, iface);
		}
	}
	
	return CConnection();
}

CConnection CNexus::Connect(IType * who, CServerAddress & server, CString const & iface, std::function<void()> ondisconnect)
{
	auto s = GetServer(server);

	if(!s)
		return CConnection();

	if(s->Interfaces.Contains(iface))
	{
		if(s->Interfaces(iface) == null)
		{
			if(auto imp = s->Connect(iface))
			{
				s->Interfaces[iface] = imp;
				Core->Log->ReportMessage(this, L"Server connected: %-30s -> %-30s -> %-30s", who->GetInstanceName(), iface, s->Instance);
			}
		}

		if(s->Interfaces[iface] != null)
		{
			s->Users[iface][who] = ondisconnect;
			return CConnection(who, s, iface);
		}
	}

	return CConnection();
}


CConnection CNexus::Connect(IType * who, CString const & iface, std::function<void()> ondisconnect)
{
	auto o = FindImplementators(iface);

	if(!o.empty())
		return Connect(who, o.front()->Address, iface, ondisconnect);
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
		cc.push_back(Connect(who, s->Address, iface));
	
	return cc;
}

void CNexus::Disconnect(CConnection & c)
{
	c.Server->Users[c.ProtocolName].Remove(c.Who);
		
	c.Protocol = null;
	c.Server = null;
	c.Who = null;
}

void CNexus::Disconnect(CList<CConnection> & cc)
{
	for(auto c : cc)
	{
		Disconnect(c);
	}
}

void CNexus::Break(CString const & server, CString const & iface)
{
	Core->Log->ReportMessage(this, L"Disconnecting: %s %s", server, iface);

	auto s = GetServer(server);

	if(s->Interfaces.Contains(iface) && s->Interfaces[iface] != null)
	{
		//s->Disconnecting(s, s->Protocols[protocol], const_cast<CString &>(protocol));

		auto i = s->Users[iface].begin();

		while(i != s->Users[iface].end())
		{
			auto k = i->first;

			if(i->second)
				i->second();

			if(s->Users[iface].Contains(k))
			{
				//Core->Log->ReportWarning(this, L"%s disconnected improperly from %s -> %s", k->GetInstanceName(), s->Url.Server, pr);
				s->Users[iface].Remove(k);
			}
			
			i = s->Users[iface].begin();
		}

		s->Disconnect(s->Interfaces[iface]);
	
		s->Interfaces[iface] = null;
	}
}

CList<CServerRelease *> CNexus::FindImplementators(CString const & iface)
{
	return Releases.Where([&](auto i){ return i->Registry->One(L"Interfaces")->Any(iface); });
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
	if(command->Name == CCore::GetClassName())
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

CXon * CNexus::QueryRegistry(CServerAddress const & server, CString const & path)
{
	auto info = Releases.Find([&](auto i){ return i->Address == server; });

	return info && info->Registry ? info->Registry->One(path) : null;
}

CMap<CServerRelease *, CXon *> CNexus::QueryRegistry(CString const & path)
{
	CMap<CServerRelease *, CXon *> l;

	for(auto i : Releases)
	{
		if(auto r = QueryRegistry(i->Address, path))
		{
			l[i] = r;
		}
	}

	return l;
}
