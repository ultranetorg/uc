#include "stdafx.h"
#include "CefClient.h"

using namespace uc;
using namespace std;

CCefClient::CCefClient(CExperimentalLevel * l, CTexture * t, CString const & addr)
{
	Level = l;
	Texture = t;
}

CCefClient::~CCefClient()
{
	sh_free(LastMouseArgs);
}

void CCefClient::DoIdle()
{
	Update();
}

void CCefClient::OnExit()
{
	Level->Core->CancelUrgents(this);

	Level->Core->RemoveWorker(this);
	Level->Core->ExitRequested -= ThisHandler(OnExit);

	Browser->GetHost()->CloseBrowser(true);
}

void CCefClient::Update()
{
	if(W > 0 && H > 0)
	{
		if(!Browser)
		{
			Level->RequireCef();

			Rect.Set(0, 0, W, H);

			CefWindowInfo wi = {};
			wi.SetAsWindowless(nullptr);
			wi.shared_texture_enabled = true;
			wi.external_begin_frame_enabled = true;

			CefBrowserSettings settings = {};
			// Set the maximum rate that the HTML content will render at
			//
			// NOTE: this value is NOT capped to 60 by CEF when using shared textures and
			// it is completely ignored when using SendExternalBeginFrame
			//
			// For testing, this application uses 120Hz to show that the 60Hz limit is ignored
			// (set window_info.external_begin_frame_enabled above to false to test)
			//
			settings.windowless_frame_rate = 120;

			CefBrowserHost::CreateBrowser(wi, this, Url, settings, nullptr);

			//Sleep(1000);
			Ready.wait(unique_lock<mutex>(ReadyMutex), [this]{ return !!Browser; });

			Level->Core->ExitRequested += ThisHandler(OnExit);
		}

		if(W != Rect.width || H != Rect.height)
		{
			Rect.Set(0, 0, W, H);
			Browser->GetHost()->WasResized();
		}

		if(RenderAllowed)
			Browser->GetHost()->SendExternalBeginFrame();
	}
}

void CCefClient::Navigate(CString const & url)
{
	Url = url;

	if(!Browser)
	{
		Level->Core->AddWorker(this);
	}
	else
	{
		Browser->GetMainFrame()->LoadURL(url);
	}

	TitleChanged(L"Opening...");
}

void CCefClient::OnAfterCreated(CefRefPtr<CefBrowser> browser)
{
	if(!CefCurrentlyOn(TID_UI))
	{
		assert(0);
		return;
	}
	
	browser->AddRef();
	Browser = browser;
	Ready.notify_all();
}

bool CCefClient::DoClose(CefRefPtr<CefBrowser> browser)
{
	return false;
}

void CCefClient::OnBeforeClose(CefRefPtr<CefBrowser> browser)
{
	Browser->Release();
	Browser = null;
	Level->CefCounter--;
}

void CCefClient::OnLoadEnd(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame, int /*httpStatusCode*/)
{
	Level->Core->DoUrgent(this, __FUNCTIONW__,	[this]
												{
													RenderAllowed = true;
													CefRefPtr<CCefClient> w = this; //store ref to this to prevent calling Loaded() of destroyed object
													Loaded();
													return true;
												});
}

void CCefClient::SetSize(float w, float h)
{
	if(int(w) != W || int(h) != H)
	{
		W = int(w);
		H = int(h);
	}
}

void CCefClient::GetViewRect(CefRefPtr<CefBrowser> /*browser*/, CefRect & rect)
{
	rect = Rect;
}

bool CCefClient::GetScreenInfo(CefRefPtr<CefBrowser> browser, CefScreenInfo & screen_info)
{
	CefRect view_rect;
	GetViewRect(browser, view_rect);
	
	screen_info.device_scale_factor = Level->Engine->ScreenEngine->Scaling.x;
	
	// The screen info rectangles are used by the renderer to create and position
	// popups. Keep popups inside the view rectangle.
	screen_info.rect			= view_rect;
	screen_info.available_rect	= view_rect;

	return true;
}

void CCefClient::OnPaint(CefRefPtr<CefBrowser> /*browser*/, PaintElementType type, const RectList & dirtyRects, const void * buffer, int width, int height)
{

}

void CCefClient::OnAcceleratedPaint(CefRefPtr<CefBrowser> browser, PaintElementType type, const RectList & dirtyRects, void * sh)
{
	if(Texture->Shared != sh && type == PET_VIEW)
	{
		Level->Engine->Lock.lock();

		Texture->Open(sh);

		Level->Engine->Lock.unlock();
	}
}

void CCefClient::SendMouseButtonEvent(CActive *, CActive *, CFloat2 & p, EMouseControl s, EMouseAction a)
{
	if(s == EMouseControl::LeftButton && (a == EMouseAction::On || a == EMouseAction::Off))
	{
		if(Browser)
		{
			CefMouseEvent mouse;
			mouse.x = int(p.x);
			mouse.y = int(Rect.height - p.y);
			mouse.modifiers = 0;

			cef_mouse_button_type_t ctype;

			switch(s)
			{
				case EMouseControl::MiddleButton: ctype = MBT_MIDDLE; break;
				case EMouseControl::RightButton: ctype = MBT_RIGHT; break;
				case EMouseControl::LeftButton: ctype = MBT_LEFT;
			}
			Browser->GetHost()->SendMouseClickEvent(mouse, ctype, a == EMouseAction::Off, 1);
		}
	}
}

void CCefClient::SendCursorEvent(CActive *, CActive *, CFloat2 & p, bool leave)
{
	if(Browser)
	{
		CefMouseEvent mouse;
		mouse.x = int(p.x);
		mouse.y = int(Rect.height - p.y);
		mouse.modifiers = 0;
		Browser->GetHost()->SendMouseMoveEvent(mouse, leave);
	}
}

void CCefClient::SendWheelEvent(CActive *, CActive *, CFloat2 & p, CFloat2 & d)
{
	if(Browser)
	{
		CefMouseEvent mouse;
		mouse.x = int(p.x);
		mouse.y = int(Rect.height - p.y);
		mouse.modifiers = 0;
		Browser->GetHost()->SendMouseWheelEvent(mouse, int(d.x), int(d.y));
	}
}

void CCefClient::SendKeyboradButton(CKeyboardArgs * arg)
{
	if(Browser)
	{
		CefKeyEvent e;
		e.windows_key_code = (int)arg->Control;
		e.native_key_code = (int)arg->WM;
		e.is_system_key = arg->WM == WM_SYSCHAR || arg->WM == WM_SYSKEYDOWN || arg->WM == WM_SYSKEYUP;
		e.modifiers = GetKeyboardModifiers(arg->Device, arg->Control, arg->Flags);

		if(arg->Action == EKeyboardAction::On)		e.type = KEYEVENT_RAWKEYDOWN; else
		if(arg->Action == EKeyboardAction::Off)		e.type = KEYEVENT_KEYUP; else
		if(arg->Action == EKeyboardAction::Char)	e.type = KEYEVENT_CHAR;

		Browser->GetHost()->SendKeyEvent(e);
	}
}

void CCefClient::SetFocus(bool e)
{
	if(Browser)
	{
		Browser->GetHost()->SendFocusEvent(e);
	}
}

bool CCefClient::OnProcessMessageReceived(CefRefPtr<CefBrowser> /*browser*/, CefProcessId /*source_process*/, CefRefPtr<CefProcessMessage> message)
{
	return false;
}

void CCefClient::OnPopupShow(CefRefPtr<CefBrowser> browser, bool show)
{
	///lock_guard<mutex> guard(lock_);
	///auto const composition = composition_.lock();
	///if(composition)
	///{
	///	if(show)
	///	{
	///		// remove existing
	///		composition->remove_layer(popup_layer_);
	///
	///		// create new layer
	///		popup_layer_ = create_popup_layer(device_, popup_buffer_);
	///		composition->add_layer(popup_layer_);
	///	}
	///	else{
	///		composition->remove_layer(popup_layer_);
	///	}
	///}
}

void CCefClient::OnPopupSize(CefRefPtr<CefBrowser> browser, const CefRect & rect)
{
	///decltype(popup_layer_) layer;
	///
	///{
	///	lock_guard<mutex> guard(lock_);
	///	layer = popup_layer_;
	///}
	///
	///if(layer)
	///{
	///	auto const composition = layer->composition();
	///	if(composition)
	///	{
	///		auto const outer_width = composition->width();
	///		auto const outer_height = composition->height();
	///		if(outer_width > 0 && outer_height > 0)
	///		{
	///			auto const x = rect.x / float(outer_width);
	///			auto const y = rect.y / float(outer_height);
	///			auto const w = rect.width / float(outer_width);
	///			auto const h = rect.height / float(outer_height);
	///			layer->move(x, y, w, h);
	///		}
	///	}
	///}
}

bool CCefClient::OnBeforePopup(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame, const CefString & target_url, const CefString & target_frame_name, CefLifeSpanHandler::WindowOpenDisposition target_disposition, bool user_gesture, const CefPopupFeatures & popup_features, CefWindowInfo & window_info, CefRefPtr<CefClient> & client, CefBrowserSettings & settings, bool * no_javascript_access)
{
	///shared_ptr<Composition> composition;
	///{
	///	lock_guard<mutex> guard(lock_);
	///	composition = composition_.lock();
	///}
	///
	///// we need a composition to add new popup layers to
	///if(!composition) {
	///	return true; // prevent popup
	///}
	///
	///window_info.SetAsWindowless(nullptr);
	///window_info.shared_texture_enabled = use_shared_textures();
	///window_info.external_begin_frame_enabled = send_begin_frame_;
	///
	///// pick some dimensions
	///auto const width = popup_features.widthSet ? popup_features.width : 400;
	///auto const height = popup_features.heightSet ? popup_features.height : 300;
	///
	///CefRefPtr<CCefElement> view(new CCefElement(target_frame_name, device_, width, height, use_shared_textures(), send_begin_frame_));
	///
	///CefBrowserHost::CreateBrowser(window_info, view, target_url, settings, nullptr);
	///
	///// create a new layer to handle drawing for the web popup
	///auto const layer = create_web_layer(device_, true, view);
	///if(!layer) {
	///	return true; // prevent popup
	///}
	///
	///composition->add_layer(layer);
	///
	///// center the popup within the composition
	///auto const outer_width = composition->width();
	///auto const outer_height = composition->height();
	///if(outer_width > 0 && outer_height > 0)
	///{
	///	// convert popup dimensions to normalized units
	///	// for composition space
	///	auto const sx = width / float(outer_width);
	///	auto const sy = height / float(outer_height);
	///	layer->move(0.5f - (sx / 2), 0.5f - (sy / 2), sx, sy);
	///}

	return false;
}

void CCefClient::OnAddressChange(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame, const CefString & url)
{
	Level->Core->DoUrgent(this, __FUNCTIONW__,	[this, url]
												{
													AddressChanged(CString(url));
													return true;
												});
}

void CCefClient::OnLoadError(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame, ErrorCode errorCode, const CefString & errorText, const CefString & failedUrl)
{
	Level->Core->DoUrgent(this, __FUNCTIONW__,	[this, errorText]
												{
													TitleChanged(CUrl(Url).Domain + L" - " + CString(errorText));
													return true;
												});
}

void CCefClient::OnTitleChange(CefRefPtr<CefBrowser> browser, CefString const & title)
{
	Level->Core->DoUrgent(this, __FUNCTIONW__,	[this, title]
												{
													if(!title.empty())
														TitleChanged(CString(title));
													return true;
												});
}

void CCefClient::OnProtocolExecution(CefRefPtr<CefBrowser> browser, const CefString & url, bool & allow_os_execution)
{
	Level->Core->DoUrgent(this, __FUNCTIONW__,	[this, url]
												{
													Level->Core->Execute(CString(url), sh_new<CShowParameters>(LastMouseArgs, Level->Style));
													return true;
												});


	Browser->GoBack();
}

int CCefClient::GetKeyboardModifiers(CKeyboard * k, EKeyboardControl wparam, LPARAM lparam)
{
	int modifiers = 0;
	if(k->GetPressState(EKeyboardControl::Shift)	== EKeyboardAction::On)	modifiers |= EVENTFLAG_SHIFT_DOWN;
	if(k->GetPressState(EKeyboardControl::Control)	== EKeyboardAction::On)	modifiers |= EVENTFLAG_CONTROL_DOWN;
	if(k->GetPressState(EKeyboardControl::Menu)		== EKeyboardAction::On)	modifiers |= EVENTFLAG_ALT_DOWN;
	if(k->GetToggleState(EKeyboardControl::Numlock) == EKeyboardAction::On)	modifiers |= EVENTFLAG_NUM_LOCK_ON;
	if(k->GetToggleState(EKeyboardControl::Capital) == EKeyboardAction::On)	modifiers |= EVENTFLAG_CAPS_LOCK_ON;

	switch(wparam)
	{
		case VK_RETURN:
			if((lparam >> 16) & KF_EXTENDED)
				modifiers |= EVENTFLAG_IS_KEY_PAD;
			break;

		case VK_INSERT:
		case VK_DELETE:
		case VK_HOME:
		case VK_END:
		case VK_PRIOR:
		case VK_NEXT:
		case VK_UP:
		case VK_DOWN:
		case VK_LEFT:
		case VK_RIGHT:
			if(!((lparam >> 16) & KF_EXTENDED))
				modifiers |= EVENTFLAG_IS_KEY_PAD;
			break;

		case VK_NUMLOCK:
		case VK_NUMPAD0:
		case VK_NUMPAD1:
		case VK_NUMPAD2:
		case VK_NUMPAD3:
		case VK_NUMPAD4:
		case VK_NUMPAD5:
		case VK_NUMPAD6:
		case VK_NUMPAD7:
		case VK_NUMPAD8:
		case VK_NUMPAD9:
		case VK_DIVIDE:
		case VK_MULTIPLY:
		case VK_SUBTRACT:
		case VK_ADD:
		case VK_DECIMAL:
		case VK_CLEAR:
			modifiers |= EVENTFLAG_IS_KEY_PAD;
			break;

		case VK_SHIFT:
			if(k->GetPressState(EKeyboardControl::LeftShift) == EKeyboardAction::On)	modifiers |= EVENTFLAG_IS_LEFT; else
			if(k->GetPressState(EKeyboardControl::RightShift) == EKeyboardAction::On)	modifiers |= EVENTFLAG_IS_RIGHT;
			break;

		case VK_CONTROL:
			if(k->GetPressState(EKeyboardControl::LeftControl) == EKeyboardAction::On)	modifiers |= EVENTFLAG_IS_LEFT; else
			if(k->GetPressState(EKeyboardControl::RightControl) == EKeyboardAction::On)	modifiers |= EVENTFLAG_IS_RIGHT;
			break;

		case VK_MENU:
			if(k->GetPressState(EKeyboardControl::LeftMenu) == EKeyboardAction::On)		modifiers |= EVENTFLAG_IS_LEFT; else
			if(k->GetPressState(EKeyboardControl::RightMenu) == EKeyboardAction::On)	modifiers |= EVENTFLAG_IS_RIGHT;
			break;

		case VK_LWIN:
			modifiers |= EVENTFLAG_IS_LEFT;
			break;

		case VK_RWIN:
			modifiers |= EVENTFLAG_IS_RIGHT;
			break;
	}
	return modifiers;
}

void CCefClient::OnRenderProcessTerminated(CefRefPtr<CefBrowser> browser, TerminationStatus status)
{
	Level->Core->DoUrgent(this, __FUNCTIONW__,	[this]
												{
													Level->Log->ReportError(this, L"Render has been terminated");
													return true;
												});
}

void CCefClient::OnRenderViewReady(CefRefPtr<CefBrowser> browser)
{
}

CefRefPtr<CefRenderHandler> CCefClient::GetRenderHandler()
{
	return this;
}

CefRefPtr<CefLifeSpanHandler> CCefClient::GetLifeSpanHandler()
{
	return this;
}

CefRefPtr<CefLoadHandler> CCefClient::GetLoadHandler()
{
	return this;
}

CefRefPtr<CefDisplayHandler> CCefClient::GetDisplayHandler()
{
	return this;
}

CefRefPtr<CefRequestHandler> CCefClient::GetRequestHandler()
{
	return this;
}
