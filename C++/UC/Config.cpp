#include "stdafx.h"
#include "Config.h"
#include "Nexus.h"

using namespace uc;

CConfig::CConfig(IStorageProtocol * l, CString & durl, CString & curl)
{
	Storage = l;
	DefaultUri = durl;
	CustomUri = curl;

	auto ds = Storage->OpenReadStream(durl);
	DefaultDoc = new CTonDocument(CXonTextReader(ds));
	Storage->Close(ds);

	if(Storage->Exists(curl))
	{
		auto ds = Storage->OpenReadStream(durl);
		auto cs = Storage->OpenReadStream(curl);

		Root = new CTonDocument(CXonTextReader(ds), CXonTextReader(cs));
		
		Storage->Close(ds);
		Storage->Close(cs);
	}
	else
	{
		auto s = Storage->OpenReadStream(durl);
		Root = new CTonDocument(CXonTextReader(s));
		Storage->Close(s);
	}
}

CConfig::~CConfig()
{
	delete Root;
	delete DefaultDoc;
}

void CConfig::Save()
{
	auto cs = Storage->OpenWriteStream(CustomUri);

	Root->Save(&CXonTextWriter(cs, false), DefaultDoc);

	Storage->Close(cs);
}

CXon * CConfig::GetRoot()
{
	return Root;
}
