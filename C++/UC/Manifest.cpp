#include "stdafx.h"
#include "Manifest.h"
#include "XonDocument.h"
#include "Int64.h"
#include "Hex.h"

using namespace uc;

CCompiledManifest::CCompiledManifest(CReleaseAddress release, CTonDocument & xon)
{

	Address					= release;
	//Channel					= xon.Get<CString>(L"Channel");

	//if(auto d = xon.One(L"CompleteDependencies"))
	//{
	//	CompleteDependencies = d->Children.Select<CString>([](auto i){ return i->Name; });
	//}
}

CCompiledManifest::~CCompiledManifest()
{

}

