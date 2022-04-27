#include "pch.h"

class CCefApp : public CefApp
{
	public:
		CCefApp(){}

		void OnBeforeCommandLineProcessing(CefString const& /*process_type*/, CefRefPtr<CefCommandLine> command_line) override
		{
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

	private:
		IMPLEMENT_REFCOUNTING(CCefApp);
};

int APIENTRY wWinMain(HINSTANCE instance, HINSTANCE, LPWSTR, int)
{
	CefEnableHighDPISupport();

	// Structure for passing command-line arguments.
	// The definition of this structure is platform-specific.
	CefMainArgs main_args(instance);

	//CefString(&settings.log_file).FromWString(L"cef11111.log");

	// Optional implementation of the CefApp interface.
	CefRefPtr<CCefApp> app(new CCefApp);
	
	// Execute the sub-process logic. This will block until the sub-process should exit.
	return CefExecuteProcess(main_args, app, nullptr);
}
