#pragma once
#include "ExperimentalLevel.h"

namespace uc
{
	class CCefClient : public CefClient, public CefRenderHandler, public CefLifeSpanHandler, public CefLoadHandler, public CefDisplayHandler, public CefRequestHandler, public IType, public IIdleWorker
	{
		public:
			CExperimentalLevel *						Level;
			CTexture *									Texture;
			CefBrowser *								Browser = null;
			int											W = 0;
			int											H = 0;
			CefRect										Rect = {0, 0, 0, 0};

			std::mutex									RectLock;
			std::mutex									ReadyMutex;
			std::condition_variable						Ready;
			std::mutex									TextureMutex;
			std::condition_variable						TextureReady;

			CEvent<const CString &>						TitleChanged;
			CEvent<CString &>							AddressChanged;
			CEvent<CString &, CExecutionParameters &>	ExecuteProtocol;
			CEvent<>									Loaded;

			bool										InCreation = false;
			bool										RenderAllowed = false;

			CMouseArgs *								LastMouseArgs = null;

			CString										Url;

			UOS_RTTI
			CCefClient(CExperimentalLevel * l, CTexture * t, CString const & addr);
			~CCefClient();

			void										OnExit();
			void										Update();

			CefRefPtr<CefDisplayHandler>				GetDisplayHandler() override;
			CefRefPtr<CefRequestHandler>				GetRequestHandler() override;
			
			void										Navigate(CString const & url);
			void										SetSize(float w, float h);

			void										OnAfterCreated(CefRefPtr<CefBrowser> browser) override;
			bool										DoClose(CefRefPtr<CefBrowser> browser) override;
			void										OnBeforeClose(CefRefPtr<CefBrowser> browser) override;

			void										OnLoadEnd(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame, int /*httpStatusCode*/) override;
			void										OnLoadError(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame, ErrorCode errorCode, const CefString & errorText, const CefString & failedUrl) override;
			virtual void								OnTitleChange(CefRefPtr<CefBrowser> browser, const CefString& title) override;

			void										GetViewRect(CefRefPtr<CefBrowser> /*browser*/, CefRect& rect) override;
			bool										GetScreenInfo(CefRefPtr<CefBrowser> browser, CefScreenInfo& screen_info) override;

			void										SendMouseButtonEvent(CActive *, CActive *, CFloat2 & p, EMouseControl s, EMouseAction a);
			void										SendCursorEvent(CActive *, CActive *, CFloat2 &p, bool leave);
			void										SendWheelEvent(CActive *, CActive *, CFloat2 & p, CFloat2 & d);
			void										SendKeyboradButton(CKeyboardArgs * arg);
			void										SetFocus(bool e);

			CefRefPtr<CefRenderHandler>					GetRenderHandler() override;
			CefRefPtr<CefLifeSpanHandler>				GetLifeSpanHandler() override;
			CefRefPtr<CefLoadHandler>					GetLoadHandler() override;

			bool										OnProcessMessageReceived(CefRefPtr<CefBrowser> /*browser*/, CefProcessId /*source_process*/, CefRefPtr<CefProcessMessage> message) override;
			void										OnPaint(CefRefPtr<CefBrowser> /*browser*/, PaintElementType type, const RectList& dirtyRects,const void* buffer, int width, int height) override;
			void										OnAcceleratedPaint(CefRefPtr<CefBrowser> /*browser*/,PaintElementType type, const RectList& dirtyRects, void* sh) override;
			void										OnPopupShow(CefRefPtr<CefBrowser> browser, bool show) override;
			void										OnPopupSize(CefRefPtr<CefBrowser> browser, const CefRect& rect) override;
			bool										OnBeforePopup(CefRefPtr<CefBrowser>		browser,
																	  CefRefPtr<CefFrame>		frame,
																	  const CefString&			target_url,
																	  const CefString&			target_frame_name,
																	  CefLifeSpanHandler::WindowOpenDisposition	target_disposition,
																	  bool						user_gesture,
																	  const						CefPopupFeatures& popup_features,
																	  CefWindowInfo&			window_info,
																	  CefRefPtr<CefClient>&		client,
																	  CefBrowserSettings&		settings,
																	  bool* no_javascript_access) override;

			virtual void								OnAddressChange(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame, const CefString& url) override;
			virtual void								OnProtocolExecution(CefRefPtr<CefBrowser> browser, const CefString& url, bool& allow_os_execution) override;

			int											GetKeyboardModifiers(CKeyboard * k, EKeyboardControl wparam, LPARAM lparam);


			void OnRenderProcessTerminated(CefRefPtr<CefBrowser> browser, TerminationStatus status) override;
			void OnRenderViewReady(CefRefPtr<CefBrowser> browser) override;





			void DoIdle() override;



		private:
			IMPLEMENT_REFCOUNTING(CCefClient);
	};
}