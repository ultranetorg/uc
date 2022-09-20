#include "stdafx.h"
#include "Manifest.h"
#include "XonDocument.h"
#include "Int64.h"
#include "Hex.h"

using namespace uc;

CManifest::CManifest(CReleaseAddress release, CTonDocument & xon)
{

	Address					= release;
	Channel					= xon.Get<CString>(L"Channel");
	PreviousVersion			= CVersion(xon.Get<CString>(L"PreviousVersion"));

	CompleteSize			= xon.Get<CInt64>(L"CompleteSize");
	CompleteHash			= CHex::ToBytes(xon.Get<CString>(L"CompleteHash"));

	//if(auto d = xon.One(L"CompleteDependencies"))
	//{
	//	CompleteDependencies = d->Children.Select<CString>([](auto i){ return i->Name; });
	//}
}

CManifest::~CManifest()
{

}

