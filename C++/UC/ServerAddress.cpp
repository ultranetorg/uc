#include "stdafx.h"
#include "ServerAddress.h"

using namespace uc;

CServerAddress::CServerAddress()
{
}

CServerAddress::CServerAddress(CString const & author, CString const & product, CString const & platform, CVersion & version, CString const & server) : CReleaseAddress(author, product, platform, version)
{
	Server = server;
}

bool CServerAddress::operator == (const CServerAddress & u) const
{
	return __super::operator==(u) && Server == u.Server;
}

bool CServerAddress::operator != (const CServerAddress & u) const
{
	return !this->operator==(u);
	//return Domain != u.Domain || Hub != u.Hub;
}

CString CServerAddress::ToString()
{
	return __super::ToString() + L"/" + Server;
}

CServerAddress CServerAddress::Parse(CString const & text)
{
	auto c = text.Split(L"/");

	return CServerAddress(c[0], c[1], c[2], CVersion(c[3]), c[4]);
}

