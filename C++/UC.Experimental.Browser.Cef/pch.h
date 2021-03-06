#ifndef PCH_H
#define PCH_H

#define WIN32_LEAN_AND_MEAN
#define VC_EXTRALEAN

#include <SDKDDKVer.h>

#ifdef _DEBUG
	#define _CRTDBG_MAP_ALLOC
	#include <stdlib.h>
	#include <crtdbg.h>
#endif

#include <windows.h>

#include <include/cef_app.h>
#include <include/cef_browser.h>
#include <include/cef_client.h>
#include <include/cef_version.h>

#endif //PCH_H
