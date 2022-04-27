#pragma once
#include "BitfinexProvider.h"
#include "TradingviewProvider.h"
#include "CefApp.h"

namespace uc
{
	struct CExperimentalLevel : public CLevel2
	{
		CProtocolConnection<CWorld>		World;
		CProtocolConnection<CShell>		Shell;
		CEngine *						Engine;
		CServer *						Server;
		CStyle *						Style;
		CLog *							Log;
		CStorage *						Storage;
		CBitfinexProvider *				Bitfinex;
		CTradingviewProvider *			Tradingview;

		CefRefPtr<CCef>					Cef;
		std::atomic_int					CefCounter = 0;
		CThread *						CefThread = null;

		std::condition_variable signal_;
		std::atomic_bool ready_;
		std::mutex lock_;

		CExperimentalLevel(CLevel2 * l) : CLevel2(*l)
		{
		}

		~CExperimentalLevel()
		{
		}

		void RequireCef()
		{
			if(CefCounter == 0)
			{
				CefCounter = 1;
	
				Core->RunThread(L"Cef browser lifes",	[this]
														{
															while(CefCounter > 0)
															{
																Sleep(1);
															}
														}, 
														[this]
														{
															CefPostTask(TID_UI, CefRefPtr<CefTask>(new CCefQuitTask()));
															CefThread->Join();

															Cef = nullptr;

														});
			}
			else
				CefCounter++;

			if(Cef)
			{
				if(!ready_)
					signal_.wait(std::unique_lock<std::mutex>(lock_), [this]{ return ready_.load(); });

				return;
			}

			///Engine->Updating += ThisHandler(ProcessCefMessages);

			CefSettings settings = {};

			settings.no_sandbox = true;
			settings.multi_threaded_message_loop = false;
			settings.windowless_rendering_enabled = true;
			settings.log_severity = LOGSEVERITY_ERROR;

			CefString(&settings.resources_dir_path)		.FromWString(Nexus->Storage->Resolve(Server->MapPath(L"Browser"))); 
			CefString(&settings.locales_dir_path)		.FromWString(Nexus->Storage->Resolve(Server->MapPath(L"Browser/locales")));
			CefString(&settings.browser_subprocess_path).FromWString(Nexus->Storage->UniversalToNative(Server->MapPath(CNativePath::GetFileNameBase(PROJECT_TARGET_FILENAME) + L".Browser.Cef.exe")));
			CefString(&settings.cache_path)				.FromWString(Nexus->Storage->UniversalToNative(Server->MapTmpPath(L"Browser/Cache")));
			CefString(&settings.log_file)				.FromWString(Nexus->Storage->UniversalToNative(Server->MapTmpPath(L"Browser/Cef.log")));
			CefString(&settings.user_agent)				.FromWString(CString::Format(L"Mozilla/5.0 Chrome/%d.%d.%d.%d Mobile", CHROME_VERSION_MAJOR, CHROME_VERSION_MINOR, CHROME_VERSION_BUILD, CHROME_VERSION_PATCH));
	
			CefMainArgs args;

			Cef = CefRefPtr<CCef>(new CCef(this, Server));

			CefThread = Core->RunThread(L"Cef", [this, args, settings]
												{
													CefEnableHighDPISupport();

													CefInitialize(args, settings, Cef, nullptr);

													ready_ = true;
													signal_.notify_all();

													CefRunMessageLoop();

													CefShutdown();
												}, []{});

			signal_.wait(std::unique_lock<std::mutex>(lock_), [this]{ return ready_.load(); });

		}
	};
}
