#include "StdAfx.h"
#include "Path.h"

using namespace uc;

CString CPath::GetName(CString const & n)
{
	return n.substr(n.find_last_of(L'/') + 1);
}

CString CPath::GetNameBase(CString const & u)
{
	auto n = u;
	auto i = n.find_last_of(L'/');

	if(i != CString::npos)
	{
		n = n.substr(i + 1);
	}

	auto j = n.find_last_of(L'.'); 
	if(j != 0 && j != CString::npos)
	{
		n = n.Substring(0, j);
	}

	return n;
}

CString CPath::Join(CString const & a, CString const & b, CString const & c)
{
	return Join(Join(a, b), c);
}

CString CPath::Join(CString const & a, CString const & b, CString const & c, CString const & d)
{
	return Join(Join(a, b, c), d);
}

CString CPath::Join(CString const & a, CString const & b)
{
	if(a.empty())
	{
		if(b.empty())
			return L"";

		auto s = std::count(b.begin(), b.end(), L'/');
		
		if(s > 0 && s == b.length()) /// b is "///..."
			return L"/";

		return b[0] == L'/' ? L'/' + b.Trim(L"/") : b.Trim(L"/");
	}

	if(b.empty())
	{
		if(a.empty())
			return L"";

		auto s = std::count(a.begin(), a.end(), L'/');
		
		if(s > 0 && s == a.length()) /// a is "///..."
			return L"/";

		return a[0] == L'/' ? L'/' + a.Trim(L"/") : a.Trim(L"/");
	}

	auto r = a.Trim(L"/") + L'/' + b.Trim(L"/");

	if(a[0] == L'/' && a != L"/")
	{
		r = L'/' + r;
	}

	return r;
}

CString CPath::ReplaceLast(CString & p, CString const & path)
{
	CString r = p;

	auto f = r.Substring(0, r.find_last_of(L'/'));

	return Join(f, path);
}

CString CPath::ReplaceExtension(CString const & p, CString const & ext)
{
	CString::size_type i = 0;

	CString r = p;

	if((i = r.find_last_of(L".")) != CString::npos)
	{
		r = r.replace(i + 1, r.size() - i - 1, ext);
	}
	else
		r +=  L"." + ext;

	return r;
}

CString CPath::GetDirectoryPath(const CString & addr)
{
	auto i = addr.find_last_of(L'/');
	if(i == 0)
		return L"/";
	else if(i == CString::npos)
		return L"";
	else
		return addr.Substring(0, i);
}

CString CPath::GetExtension(CString & f)
{
	auto i = f.find_last_of(L".");

	if(i != CString::npos)
	{
		return f.Substring(i + 1).ToLower();
	}

	return L"";
}

CString CPath::ReplaceMount(CString & p, CString const & mount)
{
	auto r = p;
	auto i = r.find(L"-");
	auto j = r.find(L"/", i);

	r.erase(i + 1, j - (i+1));
	r.insert(i + 1, mount);

	return r;
}

CString CPath::Universalize(CString const & path)
{
	auto p = path.Replace(L"\\", L"/");
	//p = p.Replace(L":/", L"/");
	return p;
}

CString CPath::Nativize(CString const & path)
{
	auto p = path.Replace(L"/", L"\\");

	//std::wregex pattern(L"^([a-zA-z])\\\\", std::wregex::icase);
	//
	//p = regex_replace(p, pattern, L"$1:\\");
	 
	return p;
}
