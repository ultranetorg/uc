#include "stdafx.h"
#include "ReleaseAddress.h"

using namespace uc;

CReleaseAddress::CReleaseAddress()
{
}

CReleaseAddress::CReleaseAddress(CString const & author, CString const & product, CString const & platform, CVersion & version)
{
	Author = author;
	Product = product;
	Platform = platform;
	Version = version;
}

bool CReleaseAddress::operator == (const CReleaseAddress & u) const
{
	return Author == u.Author && Product == u.Product && Platform == u.Platform && Version == u.Version;
}

bool CReleaseAddress::operator != (const CReleaseAddress & u) const
{
	return !this->operator==(u);
	//return Domain != u.Domain || Hub != u.Hub;
}

CString CReleaseAddress::ToString()
{
	return Author + L"/" + Product + L"/" + Platform + L"/" + Version.ToString();
}

CReleaseAddress CReleaseAddress::Parse(CString const & text)
{
	auto & c = text.Split(L"/");

	return CReleaseAddress(c[0], c[1], c[2], CVersion(c[3]));
}

