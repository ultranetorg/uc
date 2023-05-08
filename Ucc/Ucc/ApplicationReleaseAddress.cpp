#include "stdafx.h"
#include "ApplicationReleaseAddress.h"

using namespace uc;

CApplicationReleaseAddress::CApplicationReleaseAddress()
{
}

CApplicationReleaseAddress::CApplicationReleaseAddress(CString const & author, CString const & product, CString const & platform, CVersion & version, CString const & application) : CReleaseAddress(author, product, platform, version)
{
	Application = application;
}

bool CApplicationReleaseAddress::operator == (const CApplicationReleaseAddress & u) const
{
	return __super::operator==(u) && Application == u.Application;
}

bool CApplicationReleaseAddress::operator != (const CApplicationReleaseAddress & u) const
{
	return !this->operator==(u);
	//return Domain != u.Domain || Hub != u.Hub;
}

CString CApplicationReleaseAddress::ToString()
{
	return __super::ToString() + SlashSeparator + Application;
}

CApplicationReleaseAddress CApplicationReleaseAddress::Parse(CString const & text)
{
	auto & c = text.Split(SlashSeparator);
	auto & p = c[0].Split(DotSeparator);

	return CApplicationReleaseAddress(p[0], p[1], c[1], CVersion(c[2]), c[3]);
}

bool CApplicationReleaseAddress::Empty()
{
	return Author.empty() && Product.empty() && Platform.empty() && Application.empty();
}

