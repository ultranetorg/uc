#include "stdafx.h"
#include "RealizationAddress.h"

using namespace uc;

CRealizationAddress::CRealizationAddress()
{
}

CRealizationAddress::CRealizationAddress(CString const & author, CString const & product, CString const & platform) : CProductAddress(author, product)
{
	Platform = platform;
}

bool CRealizationAddress::operator == (const CRealizationAddress & u) const
{
	return __super::operator ==(u) && Platform == u.Platform;
}

bool CRealizationAddress::operator != (const CRealizationAddress & u) const
{
	return !this->operator==(u);
}

CString CRealizationAddress::ToString()
{
	return CProductAddress::ToString() + Separator + Platform;
}

CRealizationAddress CRealizationAddress::Parse(CString const & text)
{
	auto c = text.Split(Separator);

	return CRealizationAddress(c[0], c[1], c[2]);
}

