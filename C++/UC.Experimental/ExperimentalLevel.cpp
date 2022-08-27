#include "stdafx.h"
#include "ExperimentalLevel.h"
#include "CefApp.h"

using namespace uc;

void CExperimentalLevel::RequireCef()
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

	auto cef = Server->Release->Manifest->CompleteDependencies.Find([](auto i){ return i->Address.Product == L"cef"; });

	CefString(&settings.resources_dir_path)		.FromWString(Core->Resolve(Nexus->MapPathToRelease(cef->Address, L""))); 
	CefString(&settings.locales_dir_path)		.FromWString(Core->Resolve(Nexus->MapPathToRelease(cef->Address, L"locales")));
	CefString(&settings.browser_subprocess_path).FromWString(Storage->UniversalToNative(Server->MapReleasePath(CNativePath::GetFileNameBase(PROJECT_TARGET_FILENAME) + L".Browser.Cef.exe")));
	CefString(&settings.cache_path)				.FromWString(Storage->UniversalToNative(Server->MapUserLocalPath(L"Browser/Cache")));
	CefString(&settings.log_file)				.FromWString(Storage->UniversalToNative(Server->MapUserLocalPath(L"Browser/Cef.log")));
	CefString(&settings.user_agent)				.FromWString(CString::Format(L"Mozilla/5.0 Chrome/%d.%d.%d.%d Mobile", CHROME_VERSION_MAJOR, CHROME_VERSION_MINOR, CHROME_VERSION_BUILD, CHROME_VERSION_PATCH));
	
	CefMainArgs args;

	Cef = CefRefPtr<CCef>(new CCef(Nexus, Server));

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
