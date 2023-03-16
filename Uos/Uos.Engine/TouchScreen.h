#pragma once
#include "InputDevice.h"
#include "TouchManipulation.h"
#include "ScreenEngine.h"

namespace uc
{
	#define TOUCHEVENTFMASK_CONTACTAREA 0x0004
	#define MOUSEEVENTF_FROMTOUCH		0xFF515700

	class CInputSystem;
	class CTouchScreen;

	struct CTouch
	{
		int			Id = -1;
		bool		Primary = false;
		CFloat2		Position;
		CFloat2		Origin;
		CFloat2		Delta;
		CFloat2		Size;
	};

	enum class ETouchAction
	{
		Null, Added, Removed, Movement
	};

	static wchar_t * ToString(ETouchAction e)
	{
		static wchar_t * name[] = {L"", L"Added", L"Removed", L"Movement"};
		return name[int(e)];
	}

	class CTouchInput : public CInputMessage
	{
		public:
			CTouchScreen *		Device;
			CScreen *			Screen = null;
			ETouchAction		Action;
			CTouch 				Touch;
			CArray<CTouch>		Touches;

			CTouchInput()
			{
				Class = EInputClass::TouchScreen;
			}
	};


	class CTouchScreen : public CEngineEntity, public CInputDevice, public INativeMessageHandler
	{
		public:
			CInputSystem *					InputEngine;
			CScreenEngine *					ScreenEngine;
			CWindowScreen *					Screen;
			CArray<TOUCHINPUT>				Inputs;
			CArray<CTouch>					Touches;

			UOS_RTTI
			CTouchScreen(CEngineLevel * ew, CInputSystem *, CWindowScreen * w);
			~CTouchScreen();

			bool							ProcessMessage(MSG * m) override;
	};
}
