#pragma once
#include "targetver.h"

#include "../Engine/Include.h"

#ifdef WORLD_EXPORT_DLL
	#define UOS_WORLD_LINKING __declspec(dllexport)
#endif

#include "Globals.h"
#include "resource.h"

#ifdef _DEBUG
	#define D3D_DEBUG_INFO

	#define _CRTDBG_MAP_ALLOC
	#include <stdlib.h>
	#include <crtdbg.h>
#endif