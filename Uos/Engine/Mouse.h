#pragma once
#include "InputDevice.h"
#include "ScreenEngine.h"

namespace uc
{
	class CInputSystem;
	class CMouse;

	enum class EMouseAction
	{
		Null, Off, On, DoubleClick, Move, Rotation
	};

	enum class EMouseControl
	{
		Null			= 0,
		LeftButton		= VK_LBUTTON,
		RightButton		= VK_RBUTTON,
		MiddleButton	= VK_MBUTTON,
		Wheel,
		Cursor
	};

	class UOS_ENGINE_LINKING CMouseInput : public CInputMessage
	{
		public:
			CMouse *		Device = null;
			CScreen *		Screen = null;
			CFloat2			Position;
			CFloat2			PositionDelta;
			CFloat2			RotationDelta;
			EMouseAction	Action;
			EMouseControl	Control;

			CMouseInput()
			{
				Class = EInputClass::Mouse;
			}
	};

	class UOS_ENGINE_LINKING CMouse : public CEngineEntity, public CInputDevice, public INativeMessageHandler
	{
		public:
			CInputSystem *								InputEngine;
			CScreenEngine *								ScreenEngine;

			CFloat2										Position;		// viewport 2d position
			CScreen *									Screen = null;

			HCURSOR										Default;
			HCURSOR										Image;


			EMouseAction								GetKeyState(EMouseControl vk);
			bool										ProcessMessage(MSG * m);

			UOS_RTTI
			CMouse(CEngineLevel * ew, CScreenEngine * se, CInputSystem * ie);
			~CMouse();
			
			void ApplyCursor();
			void SetImage(HCURSOR c);
			void ResetImage();
	};
}
