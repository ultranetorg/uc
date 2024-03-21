#include "stdafx.h"
#include "ReleaseAddress.h"

using namespace uc;

CReleaseAddress::CReleaseAddress()
{
}

CReleaseAddress::CReleaseAddress(CString const & author, CString const & product, CString const & platform, CVersion & version) : CRealizationAddress(author, product, platform)
{ 
	Version = version;
}

bool CReleaseAddress::operator == (const CReleaseAddress & u) const
{
	return __super::operator==(u) && Version == u.Version;
}

bool CReleaseAddress::operator != (const CReleaseAddress & u) const
{
	return !this->operator==(u);
	//return Domain != u.Domain || Hub != u.Hub;
}

CString CReleaseAddress::ToString()
{
	return __super::ToString() + SlashSeparator + Version.ToString();
}

CReleaseAddress CReleaseAddress::Parse(CString const & text)
{
	auto & c = text.Split(SlashSeparator);
	auto & p = c[0].Split(DotSeparator);

	return CReleaseAddress(p[0], p[1], c[1], CVersion(c[2]));
}

