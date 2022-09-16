#pragma once
#include "targetver.h"

#include "../UC.World/Include.h"

#include <psapi.h>

#ifdef SHELL_EXPORT_DLL
	#define UOS_SHELL_IMPORTEXPORT __declspec(dllexport)
#endif

namespace uc
{
	#define SHELL_HUD									L"Hud"
	#define SHELL_HUD_1									L"Hud-1111111111111111"
	#define SHELL_FIELD_MAIN							L"Field-1111111111111111"
	#define SHELL_FIELD_PICTURES						L"Field-2222222222222222"
	#define SHELL_FIELD_WORK							L"Field-3333333333333333"

	#define SHELL_BOARD_1								L"Board-1111111111111111"
	#define SHELL_HISTORY_1								L"History-1111111111111111"
	#define SHELL_TRAY_1								L"Tray-1111111111111111"
	#define SHELL_APPLICATIONSMENU_1					L"ApplicationsMenu-1111111111111111"
	#define SHELL_THEME_1								L"Theme-1111111111111111"

	#define SHELL_FIELD_HOME							L"Field-aaaaaaaaaaaaaaaa"
}

using namespace uc;

#ifdef _DEBUG
	#define D3D_DEBUG_INFO

	#define _CRTDBG_MAP_ALLOC
	#include <stdlib.h>
	#include <crtdbg.h>
#endif