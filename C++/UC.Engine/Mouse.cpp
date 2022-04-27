#include "stdafx.h"			
#include "Mouse.h"
#include "InputSystem.h"

using namespace uc;

CMouse::CMouse(CEngineLevel * ew, CScreenEngine * se, CInputSystem * ie) : CEngineEntity(ew)
{
	ScreenEngine = se;
	InputEngine = ie;

	Default = LoadCursor(null, IDC_ARROW);

}

CMouse::~CMouse()
{
}

void CMouse::ResetImage()
{
	Image = null;
}

void CMouse::SetImage(HCURSOR c)
{
	Image = c;
}

void CMouse::ApplyCursor()
{
	for(auto i : ScreenEngine->Screens)
	{
		i->SetCursor(Image ? Image : Default);
	}
}

bool CMouse::ProcessMessage(MSG * msg)
{
	auto m = new CMouseInput();
	m->Device	= this;
	m->Id		= InputEngine->GetNextID();
	m->Action	= EMouseAction::Null;
	m->Screen	= ScreenEngine->GetScreenByWindow(msg->hwnd);

	POINT cp;

	switch(msg->message)
	{
		case WM_MOUSEMOVE:
			m->Control		= EMouseControl::Cursor;
			m->Action		= EMouseAction::Move;
			
			cp = {GET_X_LPARAM(msg->lParam), GET_Y_LPARAM(msg->lParam)};
			break;
		
		case WM_MOUSEWHEEL:
			m->Control	= EMouseControl::Wheel;
			m->Action	= EMouseAction::Rotation;
			
			cp.x = GET_X_LPARAM(msg->lParam);
			cp.y = GET_Y_LPARAM(msg->lParam);
			ScreenToClient(msg->hwnd, &cp);
			
			m->RotationDelta.y	= float(GET_WHEEL_DELTA_WPARAM(msg->wParam) / WHEEL_DELTA);
			break;
		
		case WM_LBUTTONDOWN:
		case WM_RBUTTONDOWN:
		case WM_MBUTTONDOWN:
			m->Control	= msg->message == WM_LBUTTONDOWN ? EMouseControl::LeftButton : (msg->message == WM_RBUTTONDOWN ? EMouseControl::RightButton : EMouseControl::MiddleButton) ;
			m->Action	= EMouseAction::On;
			cp			= {GET_X_LPARAM(msg->lParam), GET_Y_LPARAM(msg->lParam)};
			break;

		case WM_LBUTTONUP:
		case WM_RBUTTONUP:
		case WM_MBUTTONUP:
			m->Control		= msg->message == WM_LBUTTONUP ? EMouseControl::LeftButton : (msg->message == WM_RBUTTONUP ? EMouseControl::RightButton : EMouseControl::MiddleButton) ;
			m->Action		= EMouseAction::Off;
			cp = {GET_X_LPARAM(msg->lParam), GET_Y_LPARAM(msg->lParam)};
			break;

		case WM_LBUTTONDBLCLK:
		case WM_RBUTTONDBLCLK:
		case WM_MBUTTONDBLCLK:
			m->Control	= msg->message == WM_LBUTTONDBLCLK ? EMouseControl::LeftButton : (msg->message == WM_RBUTTONDBLCLK ? EMouseControl::RightButton : EMouseControl::MiddleButton) ;
			m->Action	= EMouseAction::DoubleClick;
			cp			= {GET_X_LPARAM(msg->lParam), GET_Y_LPARAM(msg->lParam)};
			break;
	}

	if(m->Action != EMouseAction::Null)
	{
		auto p = m->Screen->NativeToScreen(CFloat2(float(cp.x), float(cp.y)));

		if(m->Screen != Screen)
		{
			Screen = m->Screen;
			Position = p;
		}

		m->Position.x		= p.x;
		m->Position.y		= p.y;
		m->PositionDelta	= {p.x - Position.x, p.y - Position.y};
			
		Position = p;
	
		InputEngine->SendInput(m);
	}
	
	m->Free();

	return false;
}

EMouseAction CMouse::GetKeyState(EMouseControl vk)
{
	auto u = GetAsyncKeyState(int(vk));
	return (u & 0x8000) ? EMouseAction::On : EMouseAction::Off;
}

