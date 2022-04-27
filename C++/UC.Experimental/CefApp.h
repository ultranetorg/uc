#pragma once

namespace uc
{
	class CCef : public CefApp, public CefBrowserProcessHandler, public CefRenderProcessHandler
	{
		public:
			CLevel2 * Level;
			CServer * Server;

			CCef(CLevel2 * l, CServer * s)
			{
				Level = l;
				Server = s;
			}

			CefRefPtr<CefBrowserProcessHandler> GetBrowserProcessHandler() override
			{
				return this;
			}

			CefRefPtr<CefRenderProcessHandler> GetRenderProcessHandler() override
			{
				return this;
			}

			void OnBeforeCommandLineProcessing(CefString const& process_type, CefRefPtr<CefCommandLine> command_line) override
			{
				//command_line->AppendSwitchWithValue("log-file", CefString(Level->Nexus->Storage->ObjectToLocalPath(Level->Nexus->Storage->MapTmpPath(CFile::GetClassName(), Hub, L"main.log").Object)));
				//command_line->AppendSwitchWithValue("debug-log-path", CefString(Level->Nexus->Storage->ObjectToLocalPath(Level->Nexus->Storage->MapTmpPath(CFile::GetClassName(), Hub, L"debug.log").Object)));

					// disable creation of a GPUCache/ folder on disk
				command_line->AppendSwitch("disable-gpu-shader-disk-cache");
		
				//command_line->AppendSwitch("disable-accelerated-video-decode");
				
				// un-comment to show the built-in Chromium fps meter
				//command_line->AppendSwitch("show-fps-counter");		

				//command_line->AppendSwitch("disable-gpu-vsync");

				// Most systems would not need to use this switch - but on older hardware, 
				// Chromium may still choose to disable D3D11 for gpu workarounds.
				// Accelerated OSR will not at all with D3D11 disabled, so we force it on.
				//
				// See the discussion on this issue:
				// https://github.com/daktronics/cef-mixer/issues/10
				//
				command_line->AppendSwitchWithValue("use-angle", "d3d11");

				// tell Chromium to autoplay <video> elements without 
				// requiring the muted attribute or user interaction
				command_line->AppendSwitchWithValue("autoplay-policy", "no-user-gesture-required");

				#if !defined(NDEBUG)
				// ~RenderProcessHostImpl() complains about DCHECK(is_self_deleted_)
				// when we run single process mode ... I haven't figured out how to resolve yet
				//command_line->AppendSwitch("single-process");
				#endif
			}

			virtual void OnContextInitialized() override
			{
			}

			//
			// CefRenderProcessHandler::OnContextCreated
			//
			// Adds our custom 'mixer' object to the javascript context running
			// in the render process
			//
			void OnContextCreated(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame, CefRefPtr<CefV8Context> context) override
			{
			//	mixer_handler_ = new MixerHandler(browser, context);
			}

			//
			// CefRenderProcessHandler::OnBrowserDestroyed
			//
			void OnBrowserDestroyed(CefRefPtr<CefBrowser> browser) override
			{
			//	mixer_handler_ = nullptr;
			}

			virtual void OnBeforeChildProcessLaunch(CefRefPtr<CefCommandLine> command_line) override
			{
				auto type = command_line->GetSwitchValue(L"type");

				CefString s(Level->Nexus->Storage->UniversalToNative(Server->MapTmpPath(L"Browser/" + type.ToWString() + L".log"))); 
				
				command_line->AppendSwitchWithValue("log-file", s);
			}
			//
			// CefRenderProcessHandler::OnProcessMessageReceived
			//
			//bool OnProcessMessageReceived(CefRefPtr<CefBrowser> browser, CefProcessId /*source_process*/, CefRefPtr<CefProcessMessage> message) 
			//{
			//	auto const name = message->GetName().ToString();
			//	if (name == "mixer-update-stats")
			//	{
			//		if (mixer_handler_ != nullptr)
			//		{
			//			// we expect just 1 param that is a dictionary of stat values
			//			auto const args = message->GetArgumentList();
			//			auto const size = args->GetSize();
			//			if (size > 0) {
			//				auto const dict = args->GetDictionary(0);
			//				if (dict && dict->GetSize() > 0) {
			//					mixer_handler_->update(dict);
			//				}
			//			}
			//		}
			//		return true;
			//	}
			//	return false;
			//}

		private:
			IMPLEMENT_REFCOUNTING(CCef);

			//CefRefPtr<MixerHandler> mixer_handler_;
	};

	class CCefQuitTask : public CefTask
	{
		public:
			void Execute() override
			{
				CefQuitMessageLoop();
			}
			IMPLEMENT_REFCOUNTING(CCefQuitTask);
	};

}
