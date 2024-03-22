#include "StdAfx.h"
#include "VwmSceneExport.h"

HINSTANCE g_hVwmExporterInstance;

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
	switch (ul_reason_for_call)
	{
		case DLL_PROCESS_ATTACH:
			g_hVwmExporterInstance = hModule;
			break;
		case DLL_THREAD_ATTACH:
		case DLL_THREAD_DETACH:
		case DLL_PROCESS_DETACH:
			break;
	}
	return TRUE;
}

namespace uos
{
	CVwmSceneExport::CVwmSceneExport()
	{
	}

	CVwmSceneExport::~CVwmSceneExport()
	{
	}

	int CVwmSceneExport::ExtCount()
	{
		return 1; 
	}

	const TCHAR* CVwmSceneExport::Ext(int i)
	{
		if (i == 0)
		{
			return L"vwm"; 
		}
		else 
		{
			return L"";
		}
	}

	const TCHAR* CVwmSceneExport::LongDesc()
	{	
		return UNO_VWM_EXPORTER; 
	}

	const TCHAR* CVwmSceneExport::ShortDesc()
	{
		return UNO_VWM_EXPORTER;
	}

	const TCHAR* CVwmSceneExport::AuthorName()
	{	
		static auto s = CString(UO_NAME);
		return s.data();
	}

	const TCHAR* CVwmSceneExport::CopyrightMessage()
	{
		static auto s = CString(UO_COPYRIGHT);
		return s.data();
	}

	const TCHAR* CVwmSceneExport::OtherMessage1()
	{
		return L"";
	}

	const TCHAR* CVwmSceneExport::OtherMessage2()
	{
		return L"";
	}

	unsigned int CVwmSceneExport::Version()
	{
		return 100;
	}

	void CVwmSceneExport::ShowAbout(HWND hWnd)
	{
		MessageBox(hWnd, UO_NAME L" " UNO_VWM_EXPORTER, L"About", MB_OK);
	}

	BOOL CVwmSceneExport::SupportsOptions(int ext, DWORD options)
	{
		return FALSE;
	}

	int CVwmSceneExport::DoExport(const TCHAR * name, ExpInterface * ei, Interface * i, BOOL suppressPromts, DWORD options)
	{
		CSource s;
		
		s.Instance			= g_hVwmExporterInstance;
		s.DestFilePath		= name;
		s.MaxInterface		= i;
		s.MaxExpInterface	= ei;
		s.SuppressPromts	= suppressPromts;
		s.Options			= options;

		s.DestFilePath.resize(s.DestFilePath.size()-3);
		s.DestFilePath += L"vwm";

		CVwmExporter e(&s);
		e.Start();


/*		char s[_4K]="";
	
		const char * p =  IPathConfigMgr::GetPathConfigMgr()->GetDir(APP_MAX_SYS_ROOT_DIR);
		sprintf_s(s, sizeof(s), "%s%s", p, "plugins\\Mightywill.3dsMax.VwmExporter.dll");
	
		HMODULE module = LoadLibrary(s);
		if(module != NULL)
		{
			void (* pDoExport)(TCHAR *, ExpInterface *, Interface *, BOOL, DWORD);
			pDoExport = (void (*)(TCHAR *, ExpInterface *, Interface *, BOOL, DWORD))GetProcAddress(module, "DoExport");

			if(pDoExport != NULL)
			{
				pDoExport((TCHAR *)name, ei, i, suppressPromts, options);
			}
			else
			{
				MessageBox(i->GetMAXHWnd(), "No export interface\nTry to reinstall VwmExporter", "Mightywill.3dsMax.VwmExporter Error", MB_OK);
			}

			FreeLibrary(module);
		}
		else
		{
			sprintf_s(s, sizeof(s), "Dll load failed.\nError: %d\nTry to reinstall VwmExporter", GetLastError());
			MessageBox(i->GetMAXHWnd(), s, "Mightywill.3dsMax.VwmExporter Error", MB_OK);
		}*/
		
		return IMPEXP_SUCCESS;
	}
}
