#include "stdafx.h"
#include "ProductAddress.h"

using namespace uc;

CProductAddress::CProductAddress()
{
}

CProductAddress::CProductAddress(CString const & author, CString const & product)
{
	Author = author;
	Product = product;
}

bool CProductAddress::operator == (const CProductAddress & u) const
{
	return Author == u.Author && Product == u.Product;
}

bool CProductAddress::operator != (const CProductAddress & u) const
{
	return !this->operator==(u);
	//return Domain != u.Domain || Hub != u.Hub;
}

CString CProductAddress::ToString()
{
	return Author + L"/" + Product;
}

CProductAddress CProductAddress::Parse(CString const & text)
{
	auto & c = text.Split(L"/");

	return CProductAddress(c[0], c[1]);
}

