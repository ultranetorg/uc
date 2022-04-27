#include "stdafx.h"			
#include "Keyboard.h"
#include "InputSystem.h"

using namespace uc;

CKeyboard::CKeyboard(CEngineLevel * ew, CInputSystem * ie) : CEngineEntity(ew)
{
	InputEngine = ie;
}

CKeyboard::~CKeyboard()
{
}

bool CKeyboard::ProcessMessage(MSG * msg)
{
	auto v = new CKeyboardInput();
	v->Id		= InputEngine->GetNextID();
	v->Control	= (EKeyboardControl)msg->wParam;
	v->Device	= this;
	v->WM		= msg->message;
	v->Flags	= msg->lParam;

	switch(msg->message)
	{
		case WM_KEYDOWN:
		case WM_SYSKEYDOWN:
			v->Action = EKeyboardAction::On;
			break;
			
		case WM_KEYUP:
		case WM_SYSKEYUP:
			v->Action = EKeyboardAction::Off;
			break;

		case WM_CHAR:
		case WM_SYSCHAR:
			v->Action = EKeyboardAction::Char;
			break;
	}

	if(v->Action != EKeyboardAction::Null)
	{
		InputEngine->SendInput(v);
	}

	v->Free();

	return false;
}

EKeyboardAction CKeyboard::GetPressState(EKeyboardControl vk)
{
	USHORT  u= GetKeyState(int(vk));
	return (u & 0x8000) ? EKeyboardAction::On : EKeyboardAction::Off;
}

EKeyboardAction CKeyboard::GetToggleState(EKeyboardControl vk)
{
	USHORT  u= GetKeyState(int(vk));
	return (u & 0x0001) ? EKeyboardAction::On : EKeyboardAction::Off;
}
