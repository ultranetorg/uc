#include "StdAfx.h"
#include "Core.h"
#include "Mmc.h"
#include "LocalFileStream.h"
#include "StaticArray.h"
#include "Path.h"
#include "Nexus.h"

using namespace uc;

CCore::CCore(CSupervisor * s, HINSTANCE instance, wchar_t * command, const wchar_t * supervisor_folder, const wchar_t * coredir, CProductInfo & pi)
{
	Supervisor			= s;
	Core				= this;
	CreationInstance	= instance;
	Product				= pi;
	Os					= new COs();
	Commands			= new CTonDocument(CXonTextReader(command));

	CoInitialize(NULL);

	wchar_t p[4096];
	GetModuleFileNameW(CreationInstance, p, _countof(p));

	SupervisorName			= supervisor_folder;
	CoreExePath				= CNativePath::Canonicalize(p);
	LaunchDirectory			= CNativePath::GetDirectoryPath(CoreExePath);
	CoreDirectory			= CNativePath::Join(LaunchDirectory, coredir);
	DatabaseObject			= GetClassName() + L"/Database";
	CurrentReleaseSubPath	= CPath::Universalize(CoreDirectory.Substring(CNativePath::GetDirectoryPath(CNativePath::GetDirectoryPath(CoreDirectory)).length() + 1));

	auto hProcess = OpenProcess(PROCESS_QUERY_INFORMATION|PROCESS_VM_READ, FALSE, GetCurrentProcessId());
	GetModuleFileNameEx(hProcess, LoadLibrary(L"UC.dll"), p, sizeof(p) / sizeof(TCHAR));
	FrameworkDirectory = CNativePath::GetDirectoryPath(p);

	auto cmd = Commands->One(GetClassName());

	if(!cmd || !cmd->Any(VersionAutoUpArgument))
	{
		auto versions = CNativeDirectory::Enumerate(CNativePath::Join(CoreDirectory, L".."), L"[0-9]+\\.[0-9]+\\.[0-9]+\\.[0-9]+", EDirectoryFlag::DirectoriesOnly);
		versions.Sort([](auto & a, auto & b){ return CVersion(a.Name) > CVersion(b.Name); });
	
		if(CVersion(versions.First().Name) > CVersion(CNativePath::GetDirectoryName(CoreDirectory)))
		{
			STARTUPINFO info = {sizeof(info)};
			PROCESS_INFORMATION pi;
	
			auto exe = CoreExePath.Replace(CNativePath::GetDirectoryName(CoreDirectory), versions.First().Name);
			auto dir = FrameworkDirectory.Replace(CNativePath::GetDirectoryName(CoreDirectory), versions.First().Name);
	
			wchar_t c[4096] = {};
			wcscpy_s(c, _countof(c), (L"\"" + exe + L"\" " + command).data());
	
			//SetDllDirectory(FrameworkDirectory.data());
			//SetCurrentDirectory(FrameworkDirectory.data());
	
			if(CreateProcess(null, c, null, null, true, /*IsDebuggerPresent() ? DEBUG_PROCESS : 0*/0, null, dir.data(), &info, &pi))
			{
				//auto e = GetLastError();
				CloseHandle(pi.hProcess);
				CloseHandle(pi.hThread);
			}
	
			return;
		}
	}

	Timings.TotalTicks			= 0;
	Timings.MessageP1SCounter	= 0;
	Timings.EventP1SCounter		= 0;
	
	Log			= Supervisor->MainLog = Supervisor->CreateLog(L"Core");
	Diagnostics	= Supervisor->CreateDiagnostics(L"Core");
	Diagnostics->Updating += ThisHandler(OnDiagnosticsUpdate);

	Log->ReportMessage(this, L"Welcome, %s", Os->GetUserName());
	Log->ReportMessage(this, Product.ToString(L"NVSPB"));
	Log->ReportMessage(this, L"OS: %s", Os->GetVersion().ToString());
	Log->ReportMessage(this, L"User Admin?: %s", CSecurity().IsUserAdmin()? L"y" : L"n");
	Log->ReportMessage(this, L"Core directory: %s", CoreDirectory);

	HANDLE hAccessToken = NULL;

	if(OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hAccessToken))
	{
		Os->SetPrivilege(hAccessToken, SE_BACKUP_NAME, TRUE);
		Os->SetPrivilege(hAccessToken, SE_CREATE_GLOBAL_NAME, TRUE);
	}

	DBConfig = new CTonDocument(CXonTextReader(&CLocalFileStream(CNativePath::Join(CoreDirectory, L"Database.xon"), EFileMode::Open)));
	
	auto modes = DBConfig->Many(L"Database");
	CXon * xdb = null;

	for(auto i : modes)
	{
		auto xfor = i->One(L"For");
		
		if(xfor)
		{
			CString dbg = xfor->One(L"Debugger")->Get<CString>();
			bool debug =	(dbg == L"On"	&& (IsDebuggerPresent()==TRUE)) || 
							(dbg == L"Off"	&& (IsDebuggerPresent()==FALSE)) || 
							dbg == L"";

			bool build =	(xfor->Get<CString>(L"BuildConfig") == Product.Build) ||
							(xfor->Get<CString>(L"BuildConfig") == L"");

			bool machine =	(xfor->One(L"Machine")->AsString() == Os->ComputerName()) || (xfor->One(L"Machine")->AsString() == L"");

			if(debug && build && machine)
			{
				xdb = i;
				break;
			}
		}
	}

	if(xdb)
	{
		UserPath		= ResolveConstants(xdb->Get<CString>(L"User"));
		SoftwarePath	= ResolveConstants(xdb->Get<CString>(L"Servers"));
		RemapPath		= ResolveConstants(xdb->Get<CString>(L"Common"));
	}
	else
		throw CException(HERE, L"Database config is incorrect");
	
	InitializeUnid();
	
	int bufsize = sizeof(CUosNodeInformation);
	auto fmname = L"Global\\" + Product.Name + L"-" + Unid;

	HInformation = CreateFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, bufsize, fmname.c_str());

	if(HInformation == null)
	{
		throw CLastErrorException(HERE, GetLastError(), L"Could not create file mapping: %s", fmname.c_str());
	}

	auto status = GetLastError();

	auto info = (CUosNodeInformation *)MapViewOfFile(HInformation, FILE_MAP_ALL_ACCESS, 0, 0, bufsize);

	if(info == NULL)
	{
		throw CLastErrorException(HERE, GetLastError(), L"Could not map view of file: %s", fmname.c_str());
	}

	if(status == ERROR_ALREADY_EXISTS)
	{
		if(cmd)
		{
			if(cmd->Any(RestartDirective))
			{
				do 
				{
					HInformation = CreateFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, bufsize, fmname.c_str());
					
					status = GetLastError();
	
					Sleep(10);
				}
				while(status != ERROR_SUCCESS);
			}
			else
			{
				COPYDATASTRUCT cd = {};
				cd.dwData = 0;
				cd.lpData = command;
				cd.cbData = (DWORD)(wcslen(command) + 1) * sizeof(*command);
		
				auto e = SendMessage(info->Mmc, WM_COPYDATA, (WPARAM)0, (LPARAM)&cd);
			
				return;
			}
		}
		else
			return;
	}

	if(status == ERROR_SUCCESS)
	{
		Information = info;
		Information->ProcessId = GetCurrentProcessId();
		Information->MainThreadId = GetCurrentThreadId();
	}

	static int memory=0;
	GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS, (LPWSTR)&memory, &LocationInstance);

	Timer.Restart();
	TickTimer.Restart();
	SecTimer.Restart();

	ThreadFinished = CreateEvent(null, FALSE, FALSE, NULL);
	Events.push_back(ThreadFinished);

	InitializeDatabase();
	
	if(!DatabaseInitialized)
	{
		return;
	}

	Supervisor->StartWriting(SupervisorDirectory);
	PCCycle =  new CPerformanceCounter();

	Os->RegisterUrlProtocol(CliScheme, FrameworkDirectory, CoreExePath + L" {" + OpenDirective + L" " + UrlArgument + L"=\"%1\"}");

	Log->ReportMessage(this, L"Supervisor directory: %s", Supervisor->Directory);
	Log->ReportMessage(this, L"Database source:      %s", SourceDirectory);
	Log->ReportMessage(this, L"Database destination: %s", DestinationDirectory);
	Log->ReportMessage(this, L"Database work:        %s", WorkDirectory);

	LoadParameters();
	SetDllDirectory(LaunchDirectory.data());

	for(auto i : Commands->Nodes)
	{
		Execute(i, null);
	}

	Mmc = new CMmc(this);

	Initialized = true;

	Nexus = new CNexus(this, Config);

	Log->ReportMessage(this, L"-------------------------------- Core created --------------------------------");

	Run();
}
	
CCore::~CCore()
{
	if(Initialized)
	{
		delete Nexus;

		delete Mmc;

		Config->Save(&CXonTextWriter(&CLocalFileStream(Core->MapPath(ESystemPath::System, L"Core.xon"), EFileMode::New), false), DefaultConfig);

		Supervisor->StopWriting();

		ShutdownDatabase();

		delete Config;
		delete DefaultConfig;
		delete DBConfig;
		delete PCCycle;

		if(Information)
		{
			UnmapViewOfFile(Information);
			CloseHandle(HInformation);
		}

		if(!RestartCommand.empty())
		{
			STARTUPINFO info = {sizeof(info)};
			PROCESS_INFORMATION processInfo;

			wchar_t cmd[32768] = {};
			wcscpy_s(cmd, _countof(cmd), (L"\"" + CoreExePath + L"\" " + GetClassName() +  L"{" + RestartDirective + L"} " + RestartCommand).data());

			SetDllDirectory(FrameworkDirectory.data());
			SetCurrentDirectory(FrameworkDirectory.data());

			if(CreateProcess(null, cmd, null, null, true, /*IsDebuggerPresent() ? DEBUG_PROCESS : 0*/0, null, FrameworkDirectory.data(), &info, &processInfo))
			{
				//auto e = GetLastError();
				CloseHandle(processInfo.hProcess);
				CloseHandle(processInfo.hThread);
			}
		}
	}

	delete Commands;
	delete Os;
}

void CCore::InitializeUnid()
{
	auto pattern = CNativePath::GetDirectoryName(UserPath);

	CString name;

	for(auto i : CNativeDirectory::Enumerate(CNativePath::GetDirectoryPath(UserPath), pattern + L"-.+", DirectoriesOnly))
	{
		name = i.Name;
		break;
	}
	
	if(name.empty())
	{
		#ifdef _DEBUG
		Unid = L"XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
		#else
		Unid = CBase58::Encode(CGuid::New256());
		#endif

		CNativeDirectory::CreateAll(UserPath + L"-" + Unid);
	}
	else
	{
		std::wregex r(CNativePath::EscapeRegex(pattern) + L"-(.+)");
		auto words_begin = std::wsregex_iterator(name.begin(), name.end(), r);

		Unid = (*words_begin)[1].str();

		for(auto & i : CNativeDirectory::Enumerate(CNativePath::Join(CNativePath::GetDirectoryPath(UserPath), name), L".+", EDirectoryFlag(DirectoriesOnly)))
		{
			if(CInt32::Valid(i.Name))
			{
				Commits.push_back(i.Name);
				auto c = CInt32::Parse(i.Name);
				LastCommit = max(LastCommit, c);
			}
		}
	}
}

void CCore::InitializeDatabase()
{
	auto fbase = UserPath + L"-" + Unid + L"\\%08d";

	auto cmd = Commands->One(GetClassName());

	if(cmd && cmd->Any(RollbackDirective) && LastCommit >= 0)
	{
		CNativeDirectory::Delete(CString::Format(fbase, LastCommit));
		LastCommit--;
		Log->ReportWarning(this, L"Database rollback requested");
	}

	while(true)
	{
		SourceDirectory			= CString::Format(fbase, LastCommit);
		DestinationDirectory	= CString::Format(fbase, LastCommit + 1);

		if(LastCommit >= 0)
		{
			if(CNativePath::IsFile(SourceDirectory + L"\\ok") == false)
			{
				CNativeDirectory::Delete(SourceDirectory);
				LastCommit--;
				Log->ReportWarning(this, L"Not properly commited profile");
				continue;
			}
	
			if(DBConfig->Get<CString>(L"Increment") == L"n")
			{
				DestinationDirectory = CString::Format(fbase, LastCommit);
				Log->ReportWarning(this, L"No increment");
			}
		}

		auto workbasename = L"!" + Product.Name + L"-" + Unid + L"-0";
		auto workbasepath = CNativePath::Join(CNativePath::GetTmp(), workbasename);

		WorkDirectory		= workbasepath + L"-" + CGuid::Generate64();
		TmpDirectory		= workbasepath + L"-Tmp";
		SupervisorDirectory = WorkDirectory + L"\\" + SupervisorName;

		try
		{
			for(auto & i : CNativeDirectory::Enumerate(CNativePath::GetTmp(), workbasename + L"-.+", EDirectoryFlag::DirectoriesOnly))
			{
				auto p = CNativePath::Join(CNativePath::GetTmp(), i.Name);
				
				if(p != TmpDirectory)
				{
					CNativeDirectory::Delete(p);
				}
			}

			CNativeDirectory::Create(WorkDirectory);
			CNativeDirectory::Create(TmpDirectory);
			CNativeDirectory::Create(SupervisorDirectory);
 			CNativeDirectory::Create(MapPath(ESystemPath::System, L""));
			CNativeDirectory::Create(MapPath(ESystemPath::Users, L""));

			if(CNativeDirectory::Exists(SourceDirectory))
			{
				CNativeDirectory::Copy(SourceDirectory, WorkDirectory);

				if(DBConfig->Get<CString>(L"Increment") == L"n")
				{
					CNativeDirectory::Clear(DestinationDirectory);
				}
			}
		}
		catch(CException &)
		{
			MessageBox(null, (L"Failed to initialize database. Some files may still be locked by hanging processes. This is often caused by incorrect shutdown of " + Product.HumanName).data(), L"Error", MB_OK|MB_ICONERROR);
			abort();
		}


		auto d = Resolve(MapPath(ESystemPath::Core, L"Core.xon"));
		auto c = MapPath(ESystemPath::System, L"Core.xon");
		DefaultConfig = new CTonDocument(CXonTextReader(&CLocalFileStream(d, EFileMode::Open)));
	
		if(CNativePath::IsFile(c))
		{
			Config = new CTonDocument(CXonTextReader(&CLocalFileStream(d, EFileMode::Open)), CXonTextReader(&CLocalFileStream(c, EFileMode::Open)));
	
			if(Config->Get<CInt32>(L"Database/Version") == DBConfig->Get<CInt32>(L"Version"))
			{
				DatabaseInitialized = true;
			}
			else
			{
				if(MessageBox(null,(L"Your profile needs to be reset to a default state.\n\n"
									L"Ultranet profile " + Unid + L" is not compatible with this version of " + Product.HumanName + L" and migration is not supported yet.\n\n"
									"Confirm that you are agree to recreate profile from scratch.\n\n"
									"All existing data of the previous profile will be lost.").data(), (Product.HumanName + L": Warning").data(), MB_OKCANCEL|MB_ICONEXCLAMATION) == IDOK)
				{
					CNativeDirectory::Delete(WorkDirectory);

					for(auto i : Commits)
					{
						CNativeDirectory::Delete(CNativePath::Join(UserPath + L"-" + Unid, i));
					}

					delete DefaultConfig;
					delete Config;
					LastCommit = -1;

					Log->ReportWarning(this, L"Profile erased");
					continue;
				}
				else
				{
					DatabaseInitialized = false;
				}
			}
		}
		else
		{
			Config = new CTonDocument(CXonTextReader(&CLocalFileStream(d, EFileMode::Open)));
			Config->One(L"Database/Version")->Set(DBConfig->Get<CInt32>(L"Version"));
			DatabaseInitialized = true;
		}
		break;
	}
}

CString CCore::ResolveConstants(CString const & dir)
{
	CString userprofile;
	TCHAR szPath[MAX_PATH];
	
	if(SUCCEEDED(SHGetFolderPath(NULL, CSIDL_APPDATA|CSIDL_FLAG_CREATE, NULL, 0, szPath))) 
	{
		userprofile = CNativePath::Join(szPath, Product.AuthorAbbreviation + L"." + Product.Name);
	}

	CString o = dir;

	o = o.Replace(L"{CoreDirectory}",		CoreDirectory);
	o = o.Replace(L"{LocalUsername}",		Os->GetUserName());
	o = o.Replace(L"{LocalUserProfile}",	userprofile);
	o = o.Replace(L"\\\\",					L"\\");

	return CNativePath::Canonicalize(o);
}

CString CCore::Resolve(CString const & orig)
{
	if(Core->RemapPath.empty())
	{
		return orig;
	}

	if(CNativePath::IsFile(orig))
	{
		return orig;
	}

	CString s;

	auto r = MapPath(ESystemPath::RootRemapping, orig.Substring(SoftwarePath.length()));

	return CNativePath::Canonicalize(r);
}


CTonDocument * CCore::CreateConfig(CString const & d, CString const & u)
{
	if(!u.empty() && CNativePath::IsFile(u))
	{
		return new CTonDocument(CXonTextReader(&CLocalFileStream(d, EFileMode::Open)), CXonTextReader(&CLocalFileStream(u, EFileMode::Open)));
	
	}
	else
	{
		return new CTonDocument(CXonTextReader(&CLocalFileStream(d, EFileMode::Open)));
	}
}

void CCore::ShutdownDatabase()
{
	if(CommitDatabase)
	{
		CNativeDirectory::Create(DestinationDirectory);
		CNativeDirectory::Copy(WorkDirectory, DestinationDirectory);
		
		auto pattern = CNativePath::GetDirectoryName(UserPath);
		
		CArray<CString> dirs;

		for(auto i : CNativeDirectory::Enumerate(CNativePath::GetDirectoryPath(UserPath), pattern + L"-.+" + Unid + L"\\\\.+", DirectoriesOnly))
		{
			if(CInt32::Valid(i.Name))
			{
				dirs.push_back(i.Name);
			}
		}

		dirs.Sort([](auto a, auto b){ return a < b; });

		for(int i=0; i<dirs.Count(); i++)
		{
			if(int(dirs.size()) - i - 100 > 0)
			{
				CNativeDirectory::Delete(CNativePath::Join(UserPath + L"-" + Unid, dirs[i]));
			}
		}

		CLocalFileStream s(DestinationDirectory + L"\\ok", EFileMode::New);
	}
}
	
CString CCore::MapPath(ESystemPath folder, const CString & path)
{
	switch(folder)
	{
		//case ESystemPath::Root:			 return CNativePath::Join(RootPath, path);
		case ESystemPath::Core:				return CNativePath::Join(CoreDirectory, path);
		case ESystemPath::RootRemapping:	return CNativePath::Join(RemapPath, path);
		case ESystemPath::Software:			return CNativePath::Join(SoftwarePath, path);
		case ESystemPath::Tmp:				return CNativePath::Join(TmpDirectory, path);
		case ESystemPath::System:			return CNativePath::Join(WorkDirectory, L"System", path);
		case ESystemPath::Users:			return CNativePath::Join(WorkDirectory, L"Users", path);
	}

	throw CException(HERE, L"Wrong folder type");
}

void CCore::RegisterExecutor(ICommandExecutor * e)
{
	Executors.push_back(e);

	for(auto i : Commands->Nodes)
	{
		Execute(i, null);
	}
}

void CCore::Open(CUrl const & object, CExecutionParameters * parameters)
{
	CTonDocument d;

	auto core = d.Add(L"");
	core->Add(CCore::OpenDirective);
	core->Add(CCore::UrlArgument)->Set(object.ToString());

	Execute(core, parameters);
}

void CCore::Execute(CString const & command, CExecutionParameters * parameters)
{
	auto & d = CTonDocument(CXonTextReader(command));

	for(auto i : d.Nodes)
	{
		Execute(i, parameters);
	}
}

void CCore::Execute(CXon * command, CExecutionParameters * parameters)
{
	if(command->Name.empty())
	{
		if(command->Nodes.First()->Name == CCore::OpenDirective && command->Any(CCore::UrlArgument))
		{
			auto u = command->Get<CString>(CCore::UrlArgument);

			auto url = CUrl(u);

			if(url.Scheme == CCore::CliScheme)
			{
				Execute(url.Path, parameters);

				return;
			}
		}
	}

	for(auto j : Executors)
	{
		j->Execute(command, parameters);
	}
}

void CCore::AddRestartCommand(CString const & c)
{
	RestartCommand += c + L" ";
}

void CCore::SetCommit(bool c)
{
	CommitDatabase = c;
}

void CCore::Run()
{
	while(!Exiting || !Threads.empty() || !Jobs.empty())
	{
		MSG m;

		JobsLock.lock();
		bool jobs = !Jobs.empty();
		JobsLock.unlock();
				
		auto reason = MsgWaitForMultipleObjectsEx(DWORD(Events.size()), Events.data(), jobs || FinishedThreads > 0 ? 0 : MsgWaitDelay, QS_ALLEVENTS|QS_ALLINPUT|QS_ALLPOSTMESSAGE, MWMO_ALERTABLE);

		if(reason == WAIT_OBJECT_0 + Events.size())
		{
			while(PeekMessage(&m, null, 0, 0, PM_REMOVE)) // while only!!! dont change!!
			{
				PCCycle->BeginMeasure();

				if(!ProcessMessages(m))
				{
					TranslateMessage(&m); 
					DispatchMessage(&m); 
				}

				PCCycle->EndMeasure();

				Processed();
			}
		}
		else if(WAIT_OBJECT_0 <= reason && reason < WAIT_OBJECT_0 + Events.size())
		{
			if(Events[reason] != ThreadFinished)
			{
				PCCycle->BeginMeasure();
				ProcessEvents(reason);
				PCCycle->EndMeasure();
			}

			Processed();
		}

		if(FinishedThreads > 0)
		{
			while(auto t = Threads.Find([this](auto j){ return WaitForSingleObject(j->Handle, 0) == WAIT_OBJECT_0; }))
			{
				t->Exited();
				Threads.remove(t);
				delete t;
				FinishedThreads--;
			}
		}
		
		ProcessOther();
	}
}

void CCore::ProcessOther()
{
	if(Initialized && !SuspendStatus)
	{
		PCCycle->BeginMeasure();

		if(!Jobs.empty())
		{
			std::lock_guard<std::mutex> guard(JobsLock);

			for(auto i = Jobs.begin(); i != Jobs.end();)
			{
				if((*i)->Work() == true)
				{
					delete *i;
					Jobs.erase(i++);
				}
				else
					i++;
			}
		}

		if(TickTimer.IsElapsed(1.f/IdleCps, true))
		{
			for(auto i : IdleWorkers)
			{
				i->DoIdle();
			}
		}
		
		PCCycle->EndMeasure();
		
		if(SecTimer.IsElapsed(1.f, true))
		{
			SecTick();

			Timings.MessageP1SCounter	= 0;
			Timings.EventP1SCounter		= 0;

			PCCycle->Reset();

			for(auto i : PerformanceCounters)
				i->Reset();
		}
	}

	Supervisor->Cycles++;
}

void CCore::ProcessCopyData(COPYDATASTRUCT * cd)
{
	Log->ReportMessage(this, L"Instance message recieved: %s", cd->lpData);

	CTonDocument d(CXonTextReader((wchar_t *)cd->lpData));
	
	for(auto i : d.Nodes)
	{
		Execute(i, null);
	}
}
	
void CCore::ProcessEvents(int i)
{
	if(!SuspendStatus && Initialized)
	{
		Timings.EventP1SCounter++;

		EventHandlers[i]();
	}
}

bool CCore::ProcessMessages(MSG & m)
{
	if(!SuspendStatus && Initialized)
	{
		Timings.MessageP1SCounter++;

		for(auto i : NmHandlers)
		{
			if(i->ProcessMessage(&m))
			{
				return true;
			}
		}
		if(m.message == WM_HOTKEY)
		{
			auto id = m.wParam;
			if(id < HotKeys.size())
			{
				if(HotKeys[id])
				{
					HotKeys[id](id);
				}
			}
		}
		return false;
	}
	return false;
}	
/*

void CCore::ProcessException(CException & e)
{
	NeedErrorReporting	= true;
	Log->ReportException(this, e);
	Supervisor->Commit();
}
	
void CCore::ProcessAttentionException(CAttentionException & e)
{
	Suspend();
	Log->ReportError(this, L"Attention:");
	Log->ReportException(this, e);
}
*/

CThread	* CCore::RunThread(const CString & name, std::function<void()> main, std::function<void()> exit)
{
	CThread * t = new CThread(this);
	t->Name = name;
	t->Main = main;
	t->Exited = exit;
	
	Threads.push_back(t);
	t->Start();

	return t;
}

void CCore::LoadParameters()
{
	auto p = Config->One(L"Level1");
	IsAdministrating	= p->One(L"Admin")->AsBool();
	MsgWaitDelay		= p->One(L"MsgWaitDelay")->AsInt32();
	IdleCps				= p->One(L"IdleCps")->AsInt32();
}

void CCore::RegisterEvent(HANDLE e, std::function<void()> fired)
{
	for(auto i : Events)
	{
		if(i == e)
		{
			throw CException(HERE, L"Event already registered");
		}
	}
	
	Events.push_back(e);
	EventHandlers.push_back(fired);
}

void CCore::UnregisterEvent(HANDLE e)
{
	auto j = EventHandlers.begin();
	for(auto i = Events.begin(); i != Events.end(); i++)
	{
		if(*i == e)
		{
			Events.erase(i);
			EventHandlers.erase(j);
			break;
		}
		j++;
	}
}

void CCore::RegisterNativeMessageHandler(INativeMessageHandler * h)
{
	NmHandlers.push_back(h);
}

void CCore::UnregisterNativeMessageHandler(INativeMessageHandler * h)
{
	NmHandlers.remove(h);
}

int CCore::RegisterGlobalHotKey(int modifier, int vk, std::function<void(int64_t)> h)
{
	bool foundFree = false;
	int id = 0;

	for(; id<(int)HotKeys.size(); id++)
	{
		if(!HotKeys[id])
		{
			foundFree = true;
			break;
		}
	}

	if(!foundFree)
	{
		id = (int)HotKeys.size();
	}
		
	if(::RegisterHotKey(null, id, modifier, vk) != 0)
	{
		HotKeys.resize(HotKeys.size() + 1);
		HotKeys[id] = h;
		return id;
	}
	else
	{
		Log->ReportWarning(this, L"RegisterGlobalHotKey faield, LastErorr: %d", GetLastError());
	}
	return -1;
}

void CCore::UnregisterGlobalHotKey(int id)
{
	::UnregisterHotKey(null, id);
	HotKeys[id] = std::function<void(int64_t)>();
}

void CCore::AddPerformanceCounter(IPerformanceCounter * pc)
{
	PerformanceCounters.push_back(dynamic_cast<CPerformanceCounter *>(pc));
}

void CCore::RemovePerformanceCounter(IPerformanceCounter * pc)
{
	PerformanceCounters.Remove(dynamic_cast<CPerformanceCounter *>(pc));
}

void CCore::DoUrgent(IType * owner, const CString & name, std::function<bool()> work)
{
	if(!IsMainThread())
		JobsLock.lock();

	auto j = new CJob();
	j->Owner = owner;
	j->Name = name;
	j->Work = work;

	Jobs.push_back(j);

	if(!IsMainThread())
		JobsLock.unlock();
}

void CCore::CancelUrgents(IType * owner)
{
	if(!IsMainThread())
		JobsLock.lock();

	for(auto i = Jobs.begin(); i != Jobs.end(); )
	{
		if((*i)->Owner == owner)
		{	
			delete *i;
			Jobs.erase(i++);
		}
		else
			i++;
	}

	if(!IsMainThread())
		JobsLock.unlock();
}

void CCore::CancelUrgents(IType * owner, CString const & name)
{
	if(!IsMainThread())
		JobsLock.lock();

	for(auto i = Jobs.begin(); i != Jobs.end(); )
	{
		if((*i)->Owner == owner && (*i)->Name == name)
		{	
			delete *i;
			Jobs.erase(i++);
		}
		else
			i++;
	}

	if(!IsMainThread())
		JobsLock.unlock();
}

void CCore::AddWorker(IIdleWorker * w)
{
	IdleWorkers.push_back(w);
}

void CCore::RemoveWorker(IIdleWorker * w)
{
	IdleWorkers.Remove(w);
}

void CCore::Exit()
{
	Exiting = true;
	ExitRequested();
}

void CCore::Suspend()
{
	if(!SuspendStatus)
	{
		SuspendStatus = true;
		Suspended();
		Log->ReportWarning(this, L"Suspended");
	}
}

void CCore::Resume()
{
	if(SuspendStatus)
	{
		SuspendStatus = false;
		Resumed();
		Log->ReportWarning(this, L"Resumed");
	}
}

void CCore::OnDiagnosticsUpdate(CDiagnosticUpdate & a)
{
	Diagnostics->Add(L"CPU %%     : %10.0f", Timings.CpuMonitor.GetUsage());
	Diagnostics->Add(L"Cycle Max : %10.0f", 1e6 * PCCycle->Max);
	Diagnostics->Add(L"Cycle Min : %10.0f", 1e6 * PCCycle->Min);
	Diagnostics->Add(L"CPS       : %10lld", PCCycle->GetMeasures());
	Diagnostics->Add(L"EPS       : %10d", Timings.EventP1SCounter);
	Diagnostics->Add(L"MPS       : %10d", Timings.MessageP1SCounter);
	
	_CrtMemState s;
	_CrtMemCheckpoint(&s);

	Diagnostics->Add(L"Allocations");
	Diagnostics->Add(L"   Free   :  %10d", s.lCounts[0]);
	Diagnostics->Add(L"   Normal :  %10d", s.lCounts[1]);
	Diagnostics->Add(L"   Crt    :  %10d", s.lCounts[2]);
	Diagnostics->Add(L"   Ignore :  %10d", s.lCounts[3]);
	Diagnostics->Add(L"   Client :  %10d", s.lCounts[4]);

	Diagnostics->Add(L"");
	Diagnostics->Add(L"Running Threads");
	for(auto i : Threads)
	{
		Diagnostics->Add(L"  " + i->Name);;
	}

	Diagnostics->Add(L"");
	Diagnostics->Add(L"Jobs");
	{
		std::lock_guard<std::mutex> guard(JobsLock);
	
		for(auto i : Jobs)
		{
			Diagnostics->Add(L"  " + i->Name);;
		}
	}

	Diagnostics->Add(L"");

	for(auto i : PerformanceCounters)
	{
		Diagnostics->Add(L"%-30s   min: %10.f   max: %10.f   %d nps", i->Name, 1e6 * i->Min, 1e6 * i->Max, i->GetMeasures());
	}
}

bool CCore::IsMainThread()
{
	return GetCurrentThreadId() == Information->MainThreadId;
}