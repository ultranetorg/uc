#pragma once
#include "Security.h"
#include "Os.h"
#include "Supervisor.h"
#include "PerformanceCounter.h"
#include "CpuMonitor.h"
#include "Thread.h"
#include "XonDocument.h"
#include "NativeDirectory.h"
#include "ILevel.h"
#include "Sha256.h"
#include "Shared.h"
#include "ServerAddress.h"
#include "License.h"

namespace uc
{
	#undef AddJob

	class CCore;
	class CMmc;

	enum class ESystemPath
	{
		Null, Core, System, Users, Software, RootRemapping, /*UserLocal, UserGlobal, */Tmp
	};

	struct CLevel1
	{
		CLog *		Log;
		CCore *		Core;
	};

	struct CUosNodeInformation
	{
		DWORD	ProcessId = 0;
		DWORD	MainThreadId = 0;
		HWND	Mmc = 0;
	};

	struct CJob
	{
		IType *					Owner;
		CString					Name;
		std::function<bool()>	Work;
	};

	struct IIdleWorker
	{
		virtual void DoIdle()=0;
	};

	class CExecutionParameters : public virtual CShared
	{
		public:
			virtual ~CExecutionParameters(){}
	};

	struct ICommandExecutor
	{
		virtual void Execute(CXon * ep, CExecutionParameters * parameters)=0;
	};


	class UOS_LINKING CCore : public IType, public CLevel1, public ILevel
	{
		public:
			static auto constexpr						START_MODE_UNAP		= L"Unap";
			static auto constexpr						START_MODE_RESTART	= L"Restart";

			CString										Unid;
			CEvent<>									Processed;
			CEvent<>									SecTick;
			CEvent<>									Suspended;
			CEvent<>									Resumed;
			CEvent<>									MmcActivated;
			CEvent<>									ExitRequested;

			CXon *										Commands;
			CString										SupervisorName;
			CString										RestartCommand;
			
			int											LastCommit = -1;
			CList<CString>								Commits;
			bool										CommitDatabase = true;
			CString										DatabaseObject;
			
			CString										CoreDirectory;
			CString										CurrentReleaseSubPath;
			CString										RemapPath;
			CString										SoftwarePath;
			CString										UserPath;

			CString										SupervisorDirectory;
			CString										SourceDirectory;
			CString										DestinationDirectory;
			CString										WorkDirectory;
			CString										TmpDirectory;

			CUosNodeInformation	*						Information = null;
			HANDLE										HInformation;
			CString										LaunchDirectory;
			CString										FrameworkDirectory;
			CString										LaunchPath;
			HANDLE										SingleInstanceHandle = null;
			bool										Initialized = false;
			bool										DatabaseInitialized = false;

			HINSTANCE									LocationInstance;
			HINSTANCE									CreationInstance;
			CProductInfo								Product;

			CXonDocument *								DBConfig;
			CXonDocument *								Config = null;
			CXonDocument *								DefaultConfig = null;

			int											MsgWaitDelay;
			int											IdleCps;

			CString										FolderAppDataProduct;

			COs *										Os = null;
			CSupervisor *								Supervisor = null;
			CMmc *										Mmc = null;

			bool										Exiting = false;
			bool										SuspendStatus = false;
			bool										NeedErrorReporting = false;
			bool										IsAdministrating = false;
			CLevel1Timings								Timings;

			CList<ICommandExecutor *>					Executors;
			CList<CJob *>								Jobs;
			std::mutex									JobsLock;
			CList<IIdleWorker *>						IdleWorkers;
			CArray<HANDLE>								Events;
			CArray<std::function<void()>>				EventHandlers;
			CList<INativeMessageHandler *>				NmHandlers;
			CArray<std::function<void(uint64_t)>>		HotKeys;
			CArray<CPerformanceCounter *>				PerformanceCounters;

			CList<CThread *>							Threads;
			std::atomic<int>							FinishedThreads;
			HANDLE										ThreadFinished;

			CDiagnostic *								Diagnostics = null;
			CTimer										Timer;
			CTimer										TickTimer;
			CTimer										SecTimer;
			CPerformanceCounter *						PCCycle;

			UOS_RTTI
			CCore(CSupervisor * s, HINSTANCE i, wchar_t * cmd, const wchar_t * supervisor_folder, const wchar_t * coredir, CProductInfo & pi);
			~CCore();

			void										InitializeUnid();
			void										InitializeDatabase();
			void										ShutdownDatabase();
			CString										Resolve(CString const & u);

			CTonDocument *								CreateConfig(CString const & d, CString const & u);

			void										AddRestartCommand(CString const & cmd);
			void										SetCommit(bool c);
			CString										ResolveConstants(CString const & dir);

			void										Run();
			void										Exit();
			void										Suspend();
			void										Resume();

			void 										ProcessEvents(int i);
			bool 										ProcessMessages(MSG & m);
			void										ProcessOther();
			void										ProcessCopyData(COPYDATASTRUCT * s);

			CString									 	MapPath(ESystemPath folder, const CString & path);

			void										AddPerformanceCounter(IPerformanceCounter *);
			void										RemovePerformanceCounter(IPerformanceCounter * name);

			void										DoUrgent(IType * owner, const CString & name, std::function<bool()> j);
			void										CancelUrgents(IType * owner);
			void										CancelUrgents(IType * owner, CString const & name);
			CThread	*									RunThread(const CString & name, std::function<void()> main, std::function<void()> exit);

			void 										RegisterExecutor(ICommandExecutor * e);
			void										AddWorker(IIdleWorker *);
			void										RemoveWorker(IIdleWorker *);
			void										RegisterNativeMessageHandler(INativeMessageHandler *);
			void										UnregisterNativeMessageHandler(INativeMessageHandler *);
			void										RegisterEvent(HANDLE e, std::function<void()> fired);
			void										UnregisterEvent(HANDLE e);
			int											RegisterGlobalHotKey(int modifier, int vk, std::function<void(int64_t)> h);
			void										UnregisterGlobalHotKey(int id);
			

			void										Execute(CXon * p, CExecutionParameters * parameters);
			void										Execute(CString const & command);
			
			void										LoadParameters();
			void 										OnDiagnosticsUpdate(CDiagnosticUpdate & a);

			bool										IsMainThread();


	};
}