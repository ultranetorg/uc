#include "stdafx.h"
#include "ApplicationAddress.h"

using namespace uc;

CApplicationAddress::CApplicationAddress()
{
}

CApplicationAddress::CApplicationAddress(CString const & author, CString const & product, CString const & application) : CProductAddress(author, product)
{
	Application = application;
}

bool CApplicationAddress::operator == (const CApplicationAddress & u) const
{
	return __super::operator==(u) && Application == u.Application;
}

bool CApplicationAddress::operator != (const CApplicationAddress & u) const
{
	return !this->operator==(u);
	//return Domain != u.Domain || Hub != u.Hub;
}

CString CApplicationAddress::ToString()
{
	return __super::ToString() + SlashSeparator + Application;
}

CApplicationAddress CApplicationAddress::Parse(CString const & text)
{
	auto & c = text.Split(SlashSeparator);
	auto & p = c[0].Split(DotSeparator);

	return CApplicationAddress(p[0], p[1], c[2]);
}

bool CApplicationAddress::Empty()
{
	return Author.empty() && Product.empty() && Application.empty();
}

