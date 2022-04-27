#include "stdafx.h"

//typedef void (WINAPI *PGNSI)(LPSYSTEM_INFO);
//typedef BOOL (WINAPI *PGPI)(DWORD, DWORD, DWORD, DWORD, PDWORD);

BOOL GetOSDisplayString(LPTSTR pszOS, int len)
{
	int size = ::GetFileVersionInfoSize(L"kernel32.dll", null);
	if(size > 0)
	{
		void * p = malloc(size);
		::GetFileVersionInfo(L"kernel32.dll", 0, size, p);
		VS_FIXEDFILEINFO * vi;
		UINT l;
		VerQueryValue(p, L"\\", (void **)&vi, &l);

		StringCbPrintf(pszOS, len, L"%d.%d.%d.%d", HIWORD(vi->dwFileVersionMS), LOWORD(vi->dwFileVersionMS), HIWORD(vi->dwFileVersionLS), LOWORD(vi->dwFileVersionLS));

		free(p);
	}
	return TRUE;
}
