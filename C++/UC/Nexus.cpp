#include "StdAfx.h"
#include "Nexus.h"

using namespace uc;

CNexus::CNexus(CCore * l, CXonDocument * config)
{
	Core = l;

	auto d = Resolve(Core->GetPathTo(ESystemPath::Root, L"Nexus.xon"));
	auto c = Core->MapToDatabase(UOS_MOUNT_USER_GLOBAL L"\\Nexus.xon");

	DefaultConfig = new CTonDocument(CXonTextReader(&CFileStream(d, EFileMode::Open)));

	if(CNativePath::IsFile(c))
	{
		Config = new CTonDocument(CXonTextReader(&CFileStream(d, EFileMode::Open)), CXonTextReader(&CFileStream(c, EFileMode::Open)));
	}
	else
	{
		Config = new CTonDocument(CXonTextReader(&CFileStream(d, EFileMode::Open)));
	}

	Diagnostic = Core->Supervisor->CreateDiagnostics(GetClassName());
	Diagnostic->Updating += ThisHandler(OnDiagnosticUpdating);
	

//		SetDllDirectory(Core->GetPathTo(ESystemFolder::Executables, L".").c_str());
	wchar_t b[32768];
	GetEnvironmentVariable(L"PATH", b, _countof(b));
	CString e_path = Core->GetPathTo(ESystemPath::Common, L".") + L";" + CString(b);
	SetEnvironmentVariable(L"PATH", e_path.c_str());		

	ExitHotKeyId	= Core->RegisterGlobalHotKey(MOD_ALT|MOD_CONTROL,			VK_ESCAPE, ThisHandler(ProcessHotKey));
	SuspendHotKeyId	= Core->RegisterGlobalHotKey(MOD_ALT|MOD_CONTROL|MOD_SHIFT,	VK_ESCAPE, ThisHandler(ProcessHotKey));

	DirectoryPath		= Core->MapToDatabase(L".");
	ObjectTemplatePath	= Core->GetPathTo(ESystemPath::Root, L"Object.xon");

	StartServers();
	
	Core->RegisterExecutor(this);

	Initialized();
}

CNexus::~CNexus()
{
	StopServers();

	Config->Save(&CXonTextWriter(&CFileStream(Core->MapToDatabase(UOS_MOUNT_USER_GLOBAL L"\\Nexus.xon"), EFileMode::New), false), DefaultConfig);
	delete Config;
	delete DefaultConfig;


	Diagnostic->Updating -= ThisHandler(OnDiagnosticUpdating);
	Core->LevelDestroyed(2, this);
	Core->Log->ReportMessage(this, L"-------------------------------- Nexus destroyed --------------------------------");
}

void CNexus::OnDiagnosticUpdating(CDiagnosticUpdate & a)
{
	for(auto s : Servers)
	{
		Diagnostic->Add(s->Info->Url.Server);

		for(auto o : s->Objects)
		{
			Diagnostic->Add(CString::Format(L"  %-55s %3d", o->Url.Object, o->GetRefs()));
			
			//Diagnostic->Append(CString::Format(L"%-s", CString::Join(o->Users, [](auto & i){ return CString::Format(L"%s(%d)", i.first, i.second.size()); }, L",") ));
		}
	}
}

CString CNexus::MapPathToRealization(CRealizationAddress & release, CString const & path)
{
	auto & r = release;

	return Core->GetPathTo(ESystemPath::Servers, CNativePath::Join(r.Author + L"-" + r.Product + L"-" + r.Platform, path));
}

CString CNexus::MapPathToRelease(CReleaseAddress & release, CString const & path)
{
	return MapPathToRealization(release, CNativePath::Join(release.Version.ToString(), path));
}

void CNexus::StartServers()
{
	std::function<CReleaseInfo *(CReleaseAddress & a)> loadrelease;

	loadrelease =	[&](CReleaseAddress & a) -> CReleaseInfo *
					{
						auto r = Releases.Find([&](auto i){ return i->Manifest->Address == a; });

						if(!r)
						{
							auto & xon = CTonDocument(CXonTextReader(&CFileStream(Resolve(MapPathToRealization(a, a.Version.ToString() + L".manifest")), EFileMode::Open)));

							r = new CReleaseInfo();
							r->Manifest = new CManifest(a, xon);


							if(auto d = xon.One(L"CompleteDependencies"))
							{
								r->Manifest->CompleteDependencies = d->Children.Select<CManifest *>([&](auto i){ return loadrelease(CReleaseAddress::Parse(i->Name))->Manifest; });
							}
						
							Releases.AddBack(r);
						}

						return r;
					};

	for(auto i : Config->Many(L"Server"))
	{
		auto inf = new CServerInfo();
		inf->Xon			= i;
		inf->Name			= i->Get<CString>();
		inf->Locator		= CServerAddress::Parse(i->Get<CString>(L"Locator"));
		inf->Url			= CUsl(L"", inf->Name);
		inf->Installed		= i->Get<CBool>(L"IsInitialized").Value;
		inf->Release		= loadrelease(inf->Locator);

		auto & p = Resolve(MapPathToRelease(inf->Locator, inf->Locator.Server + L".registry"));

		if(CNativePath::IsFile(p))
		{
			inf->Registry = new CTonDocument(CXonTextReader(&CFileStream(p, EFileMode::Open))); 
		}

		Infos.push_back(inf);
	}

	SetDllDirectories();

	for(auto i : Infos)
	{
		auto s = GetServer(i->Url.Server);

		if(!i->Installed)
		{
			CreateMounts(i);
			s->Start(EStartMode::Initialization);
			i->Xon->One(L"IsInitialized")->Set(true);
		}
		else
		{
			s->Start(EStartMode::Start);
		}
	}
	
	Core->Log->ReportMessage(this, L"-------------------------------- Nexus created --------------------------------");
	Core->LevelCreated(2, this);
}

void CNexus::StopServers()
{
	Stopping();

	while(auto s = Servers.Last())
	{
		StopServer(s);
	}

	for(auto i : Infos)
	{
		if(i->HInstance)
		{
			#ifndef _DEBUG
			FreeLibrary(si->HInstance);
			#endif
		}
		delete i;
	}
	Infos.clear();


	for(auto i : Releases)
	{
		delete i;
	}
	Releases.clear();
}

void CNexus::StopServer(CServer * s)
{
	for(auto & i : s->Protocols)
	{
		if(i.second)
		{
			Break(s->Url, i.first);
		}
	}

	Servers.Remove(s);

	auto si = Infos.Find([s](auto i){ return i->Url == s->Url; });
	
	if(si->HInstance)
	{
		auto stop = (FStopUosServer)GetProcAddress(si->HInstance, "StopUosServer");
		stop();
	}
	else
		delete s;
}

void CNexus::Restart(CString const & cmd)
{
	RestartCommand = cmd;
}

void CNexus::SetDllDirectories()
{
	wchar_t b[32767];
	GetEnvironmentVariable(L"PATH", b, _countof(b));

	CString path = b;

	//SetDefaultDllDirectories(LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);

	for(auto i : Releases)
	{
		auto d = CPath::GetDirectoryPath(UniversalToNative(MapPathToRelease(i->Manifest->Address, L"")));
		///auto d = CNativePath::GetDirectoryPath(Storage->UniversalToNative(dll));
		path += L";" + d;
	}

	SetEnvironmentVariable(L"PATH", path.data());
}

CServer * CNexus::GetServer(CString const & name)
{
	auto info = Infos.Find([name](auto i){ return i->Url.Server == name; });

	if(!info)
	{
		return null;
	}

	auto s = Servers.Find([info](auto i){ return i->Info == info; });

	if(s)
	{
		return s;
	}

	auto l = MapPathToRelease(info->Locator, info->Registry->Get<CString>(L"Executable"));

	if(!CNativePath::IsFile(l))
	{
		throw CException(HERE, CString::Format(L"Server dll not found: %s", l));
	}

	info->HInstance = LoadLibrary(l.c_str());

	if(info->HInstance == null)
	{
		throw CLastErrorException(HERE, GetLastError(), L"DLL loading error: %s ", l.c_str());
	}
	
	auto f = (FStartUosServer)GetProcAddress(info->HInstance, "StartUosServer");


	if(f == null)
	{
		FreeLibrary(info->HInstance);
		throw CException(HERE, CString::Format(L"Interface not found: %s", l));
	}
			
	s = f(this, info);
		
	if(s == null)
	{
		throw CException(HERE, CString::Format(L"Unable to initilize system: %s", l));
	}

	if(auto r = info->Registry->One(L"Interfaces"))
	{
		for(auto i : r->Children)
		{
			s->Protocols[i->Name] = null;
		}
	}

	Core->Log->ReportMessage(this, CString::Format(L"Server loaded: %s", info->Url.Server));
		
	Servers.push_back(s);

	return s;
}

CConnection CNexus::Connect(IType * who, CUsl & u, CString const & p)
{
	auto s = GetServer(u.Server);

	if(!s)
		return CConnection();

	if(s->Protocols.Contains(p))
	{
		if(s->Protocols(p) == null)
		{
			auto imp = s->Connect(p);
			if(imp)
			{
				s->Protocols[p] = imp;
				Core->Log->ReportMessage(this, L"Server connected: %-30s -> %-30s -> %-30s", who->GetInstanceName(), p, s->Url.Server);
			}
		}
	
		if(s->Protocols[p] != null)
		{
			s->Users[p].push_back(who);
			return CConnection(who, s, s->Protocols[p], p);
		}
	}
	
	return CConnection();
}

CConnection CNexus::Connect(IType * who, CString const & p)
{
	auto o = FindImplementators(p);
	if(!o.empty())
	{
		return Connect(who, o.front(), p);
	}
	else
		return CConnection();
}


CList<CConnection> CNexus::ConnectMany(IType * who, CString const & p)
{
	if(p.empty())
	{
		throw CException(HERE, L"Protocol must be specified");
	}

	CList<CConnection> cc;

	for(auto i : Servers)
	{
		if(i->Protocols.Contains(p))
		{
			cc.push_back(Connect(who, i->Url, p));
		}
	}
	
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

void CNexus::Break(CUsl & sys, CString const & pr)
{
	Core->Log->ReportMessage(this, L"Disconnecting: %s %s", sys.Server, pr);

	auto s = GetServer(sys.Server);

	if(s->Protocols.Contains(pr) && s->Protocols[pr] != null)
	{
		s->Disconnecting(s, s->Protocols[pr], const_cast<CString &>(pr));
		s->Disconnect(s->Protocols[pr]);
	
		s->Protocols[pr] = null;
	}
}

CList<CUsl> CNexus::FindImplementators(CString const & pr)
{
	CList<CUsl> o;
	
	for(auto i : Servers)
	{
		if(i->Protocols.Contains(pr))
		{
			o.push_back(i->Url);
		}
	}
	return o;
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

void CNexus::Execute(const CUrq & u, CExecutionParameters * p)
{
	if(u.Protocol == CUol::Protocol)
	{
		auto ep = Connect<IExecutorProtocol>(this, CUsl(u), EXECUTOR_PROTOCOL);

		if(ep && ep->CanExecute(u))
		{
			ep->Execute(u, p);
			Disconnect(ep);
			return;
		}

		auto pp = ConnectMany<IExecutorProtocol>(this, EXECUTOR_PROTOCOL);

		for(auto i : pp)
		{
			if(i->CanExecute(u))
			{
				i->Execute(u, p);
				break;
			}
		}

		Disconnect(pp);
	}
	else
	{
		ShellExecute(null, L"open", u.ToString().data(), NULL, NULL, SW_SHOWNORMAL);
	}
}

CMap<CServerInfo *, CXon *> CNexus::GetRegistry(CString const & path)
{
	CMap<CServerInfo *, CXon *> l;

	for(auto i : Infos)
	{
		if(auto r = GetRegistry(i->Url, path))
		{
			l[i] = r;
		}
	}

	return l;
}

CXon * CNexus::GetRegistry(CUsl & s, CString const & path)
{
	auto info = Infos.Find([s](auto i){ return i->Url == s; });

	return info && info->Registry ? info->Registry->One(path) : null;
}


CString CNexus::MapPath(CString const & mount, CString const & path)
{
	return CPath::Join(L"/" + mount, path);
}

CString CNexus::MapPath(CUsl & u, CString const & path)
{
	//auto s = Servers.Find([](auto i){ return i->Url == u});

	auto i = Infos.Find([&](auto i){ return i->Url == u; });

	return MapPathToRelease(i->Locator, path);
}

void CNexus::CreateMounts(CServerInfo * s)
{
	CNativeDirectory::Create(Core->MapToDatabase(CNativePath::Join(UOS_MOUNT_USER_GLOBAL, s->Url.Server)));
	CNativeDirectory::Create(Core->MapToDatabase(CNativePath::Join(UOS_MOUNT_USER_LOCAL, s->Url.Server)));
}

CString CNexus::UniversalToNative(CString const & p)
{
	auto path = p;
	auto mount = path.Substring(L'/', 1);
	auto lpath = p.size() > mount.size() + 1 ? path.Substring(path.find(L'/', path.find(L'/') + 1) + 1) : L"";

	if(mount == UOS_MOUNT_LOCAL)
	{
		if(lpath.empty())
			return L"\\";
		if(lpath.size() == 2 && lpath[1] == L':')
			return lpath + L"\\";
		else
			path = CPath::Nativize(lpath);
	}
	//else if(mount == UOS_MOUNT_PERSONAL)
	//{
	//	path = Level->Core->MapToGlobal(CPath::Nativize(lpath));
	//}
	else if(mount == UOS_MOUNT_CORE)
	{
		path = Resolve(Core->GetPathTo(ESystemPath::Root, CPath::Nativize(lpath)));
	}
	else if(mount == UOS_MOUNT_SERVER)
	{
		if(lpath.empty())
			path = Core->GetPathTo(ESystemPath::Servers, L"");
		else
		{
			auto & s = lpath.Substring(L'/', 0);
			auto & r = s.length() < lpath.length() ? lpath.Substring(s.length() + 1) : L"";
			path = Resolve(MapPathToRelease(Infos.Find([&](auto i){ return i->Name == s; })->Locator, CPath::Nativize(r)));
		}
	}
	else if(mount == UOS_MOUNT_SERVER_TMP)
	{
		path = Core->MapToTmp(CPath::Nativize(lpath));
	}
	else if(mount == UOS_MOUNT_USER_LOCAL)
	{
		path = Core->MapToDatabase(CPath::Nativize(path));
	}
	else if(mount == UOS_MOUNT_USER_GLOBAL)
	{
		path = Core->MapToDatabase(CPath::Nativize(path));
	}

	return path;	
}

CString CNexus::NativeToUniversal(CString const & path)
{
	return L"/" UOS_MOUNT_LOCAL L"/" + CPath::Universalize(path);
}

CString CNexus::Resolve(CString const & n)
{
	if(CNativePath::IsFile(n))
	{
		return n;
	}

	CString s;

	auto c = Core->GetPathTo(ESystemPath::Common, L"");
	
	auto r = CNativePath::Join(c, n.Substring(c.length() - 1));

	r = CNativePath::Canonicalize(r);

	return r;
}
