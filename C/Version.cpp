#include "StdAfx.h"
#include "Version.h"
#include "Assembly.h"

using namespace uc;

CVersion::CVersion()
{
	Era			= 0;
	Release		= 0;
	Build		= 0;
}

CVersion::CVersion(const CString & v)
{
	std::wistringstream s(v);
	
	s>>Era;
	s.ignore(v.size(), L'.');
	s>>Release;
	s.ignore(v.size(), L'.');
	s>>Build;
}

CVersion::CVersion(uint e, uint r, uint b)
{
	Era			= e;
	Release		= r;
	Build		= b;
}

CVersion::~CVersion(void)
{
}

CString CVersion::ToString()
{
	return CString::Format(L"%d.%d.%d", Era, Release, Build);
}

CVersion CVersion::FromModule(HMODULE m)
{
	CVersion v;

	wchar_t s[4096];
	GetModuleFileName(m, s, 4096);
	int size = ::GetFileVersionInfoSize(s, null);
	if(size > 0)
	{
		void * p = malloc(size);
		::GetFileVersionInfo(s, 0, size, p);
		VS_FIXEDFILEINFO * vi;
		UINT l;
		VerQueryValue(p, L"\\", (void **)&vi, &l);

		v.Era		= HIWORD(vi->dwFileVersionMS);
		v.Release	= LOWORD(vi->dwFileVersionMS);
		v.Build		= HIWORD(vi->dwFileVersionLS);

		free(p);
	}
	return v;
}

CVersion CVersion::FromFile(const CString & path)
{
	CVersion v;

	int size = ::GetFileVersionInfoSize(path.c_str(), null);
	if(size > 0)
	{
		void * p = malloc(size);
		::GetFileVersionInfo(path.c_str(), 0, size, p);
		VS_FIXEDFILEINFO * vi;
		UINT l;
		VerQueryValue(p, L"\\", (void **)&vi, &l);

		v.Era		= HIWORD(vi->dwFileVersionMS);
		v.Release	= LOWORD(vi->dwFileVersionMS);
		v.Build		= HIWORD(vi->dwFileVersionLS);

		free(p);
	}
	return v;
}

CString CVersion::GetStringInfo(const CString & path, const CString & v)
{
	BOOL rc;
	DWORD *pdwTranslation;
	UINT nLength;
	CString out;

	int size = ::GetFileVersionInfoSize(path.c_str(), null);
	if(size > 0)
	{
		void * p = alloca(size);
		::GetFileVersionInfo(path.c_str(), 0, size, p);


		rc = ::VerQueryValue(p, L"\\VarFileInfo\\Translation", (void**) &pdwTranslation, &nLength);
		if (!rc)
			return L"";

		TCHAR szKey[2000];
		wsprintf(szKey, L"\\StringFileInfo\\%04x%04x\\%s", LOWORD (*pdwTranslation), HIWORD (*pdwTranslation),	v.c_str());	


		UINT l;
		wchar_t * b;
		VerQueryValue(p, szKey, (void **)&b, &l); 
		out = b;

		//free(p);
	}
	return out;
}	

bool CVersion::operator==(CVersion const & v) const 
{
	return Era == v.Era && Release == v.Release && Build == v.Build;
}

bool CVersion::operator!=(CVersion const & v) const 
{
	return !(*this == v);
}


bool CVersion::operator > (CVersion & v)
{
	if(Era > v.Era) return true;
	if(Era < v.Era) return false;

	if(Release > v.Release)	return true;
	if(Release < v.Release)	return false;

	if(Build > v.Build)	return true;
	if(Build < v.Build)	return false;

	return false;
}

bool CVersion::operator < (CVersion & v)
{
	if(Era < v.Era) return true;
	if(Era > v.Era) return false;

	if(Release < v.Release)	return true;
	if(Release > v.Release)	return false;

	if(Build < v.Build)	return true;
	if(Build > v.Build)	return false;

	return false;
}

bool CVersion::IsGreaterER(const CVersion & v)
{
	if(Era > v.Era) return true;
	if(Era < v.Era) return false;

	if(Release > v.Release)	return true;
	if(Release < v.Release)	return false;

	return false;
}

bool CVersion::IsGreaterOrEqER(const CVersion & v)
{
	if(Era > v.Era) return true;
	if(Era < v.Era) return false;

	if(Release > v.Release)	return true;
	if(Release < v.Release)	return false;

	return true;
}

CVersion CVersion::GetFrameworkVerision()
{
	return CVersion(VERSION_MAJOR, VERSION_MINOR, VERSION_BUILDNO);
}
