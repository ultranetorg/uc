#include "stdafx.h"
#include "ApplicationAddress.h"

using namespace uc;

CApplicationAddress::CApplicationAddress()
{
}

CApplicationAddress::CApplicationAddress(CString const & author, CString const & product, CString const & platform, CString const & application) : CRealizationAddress(author, product, platform)
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
	return __super::ToString() + L"/" + Application;
}

CApplicationAddress CApplicationAddress::Parse(CString const & text)
{
	auto c = text.Split(L"/");

	return CApplicationAddress(c[0], c[1], c[2], c[4]);
}

bool CApplicationAddress::Empty()
{
	return Author.empty() && Product.empty() && Platform.empty() && Application.empty();
}

