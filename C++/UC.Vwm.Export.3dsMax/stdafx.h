#pragma once

#ifndef WINVER				// Allow use of features specific to Windows XP or later.
#define WINVER 0x0501		// Change this to the appropriate value to target other versions of Windows.
#endif

#ifndef _WIN32_WINNT		// Allow use of features specific to Windows XP or later.                   
#define _WIN32_WINNT 0x0501	// Change this to the appropriate value to target other versions of Windows.
#endif						

#ifndef _WIN32_WINDOWS		// Allow use of features specific to Windows 98 or later.
#define _WIN32_WINDOWS 0x0410 // Change this to the appropriate value to target Windows Me or later.
#endif

#ifndef _WIN32_IE			// Allow use of features specific to IE 6.0 or later.
#define _WIN32_IE 0x0600	// Change this to the appropriate value to target other versions of IE.
#endif

#define WIN32_LEAN_AND_MEAN		// Exclude rarely-used stuff from Windows headers

///////////////////////////////////////////////////////////////////////////////////////////////////////////
//#include <windows.h>
//#include <shellapi.h>

#include "../Uos/Include.h"

#include <unordered_map>

#include "Resources/resource.h"


#include "3dsMax/max.h"
#include "3dsMax/stdmat.h"
//#include "3dsMax/materials/NormalBump/normalrender.h"
//#include "3dsMax/iparamb2.h"
#include "3dsMax/MeshNormalSpec.h"
#include "3dsMax/decomp.h"
//#include "3dsMax/IPathConfigMgr.h"



#define UNO_3DSMAX_CLASS_ID	Class_ID(0x5ade5e45, 0x6b4f59e0)
#define UNO_VWM_EXPORTER		L"VWM Exporter"
#define UNO_VWM_DESCRIPTION	L"Virtual World Model"
