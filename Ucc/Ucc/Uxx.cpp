#include "stdafx.h"
#include "Uxx.h"

using namespace uc;

const std::wstring	CUol::TypeName = L"uol";

/*
const CString		CUsl::Protocol = UOS_OBJECT_PROTOCOL;
const std::wstring	CUsl::TypeName = L"usl";

CUsl::CUsl()
{
}

CUsl::CUsl(CString const & u) : CUsl(CUrl(u)) 
{
}

CUsl::CUsl(const CUrl & u)
{
	Domain = u.Domain;
	Server = u.Path.Substring(u.Path.find(L'/') + 1);
}

CUsl::CUsl(CString const & d, CString const & s)
{
	Domain = d;
	Server = s;
}

CString CUsl::ToString() const
{
	CString e;

	if(Server.empty() && Domain.empty())
	{
		return e;
	}

	if(Server.empty())
	{
		throw CException(HERE, L"Server can not be empty");
	}

	e += Protocol + L"://" + Domain + L"/" + Server;

	return e;

	//return Protocol + L"://" + Domain + L"/" + Hub + L"?origin=" + Origin;
}

bool CUsl::operator != (const CUsl & u) const
{
	return !const_cast<CUsl &>(*this).Equals(u);
	//return Domain != u.Domain || Hub != u.Hub;
}

bool CUsl::operator == (const CUsl & u) const
{
	return const_cast<CUsl &>(*this).Equals(u);
	//return Domain == u.Domain && Hub == u.Hub;
}

bool CUsl::IsUsl(const CUrl & u)
{
	return u.Protocol == Protocol && !u.Path.empty();
}

CUsl::operator CUrl() const
{
	CUrl u;
	u.Protocol = Protocol;
	u.Domain = Domain;
	u.Path = Server;

	return u;
}

std::wstring CUsl::GetTypeName()
{
	return TypeName;
}

void CUsl::Read(CStream * s)
{
	CString t;
	t.Read(s);

	CUrl::Read(t, null, &Domain, &Server, null);

	///Server = Server.Substring(L'/', 0);
}

int64_t CUsl::Write(CStream * s)  
{
	return ToString().Write(s);
}

void CUsl::Write(std::wstring & s)
{
	s += ToString();
}

void CUsl::Read(const std::wstring & addr)
{
	CUrl::Read(addr, null, &Domain, &Server, null);

	///Server = Server.Substring(L'/', 0);
}

ISerializable * CUsl::Clone()
{
	return new CUsl(*this);
}

bool CUsl::Equals(const ISerializable & a) const
{
	return Domain == static_cast<CUsl const &>(a).Domain && Server == static_cast<CUsl const &>(a).Server;
	//return CUsl::operator==(static_cast<const CUsl&>(a));
}

bool CUsl::IsEmpty()
{
	return Domain.empty() && Server.empty();
}

//////////////////////// Uol /////////////////////////////////////
*/

CUol::CUol(CString const & protocol, CString const & server, CString const & object)
{
	Scheme = protocol;
	Server = server;
	Object = object;
}

CUol::CUol(CString const & protocol, CString const & server, CString const & object, CMap<CString, CString> & parameters) : CUol(protocol, server, object)
{
	Parameters = parameters;
}

CUol::CUol()
{
}

CString ExtractServer(CString const & path)
{
	return path.Substring(0, path.find(L'/'));
}

CString ExtractObject(CString const & path)
{
	return path.Substring(path.find(L'/') + 1);
}

CUol::CUol(const CUrl & addr)
{
	auto u = const_cast<CUrl &>(addr);
	
	Scheme	= u.Scheme;
	Parameters	= u.Query;

	Server	= ExtractServer(addr.Path);
	Object	= ExtractObject(addr.Path);
}

bool CUol::operator==(const CUol & a) const
{
	return Equals(a);
}

bool CUol::operator!=(const CUol & a) const
{
	return !Equals(a);
}

CUol & CUol::operator=(CUrl & addr)
{
	this->CUol::CUol(addr);

	return *this;
}

CUol::operator CUrl() const
{
	CUrl u;
	u.Scheme = Scheme;
	u.Path = Server + L'/' + Object;
	u.Query = Parameters;

	return u;
}

bool CUol::IsEmpty() const
{
	return Server.empty() && Object.empty() && Parameters.empty();// && Params.empty();
}

CString CUol::ToString() const
{
	if(IsEmpty())
		return CString();

	return Scheme + L":///" + Server + L'/' + Object + (Parameters.empty() ? L"" : L'?' + CString::Join(Parameters, [](auto & i){ return i.first + L"=" + i.second; }, L"&"));
}

CString CUol::GetObjectClass(CString const & o)
{
	return o.Substring(0, o.find('-'));
}

CString CUol::GetObjectId(CString const & o)
{
	return o.Substring(o.find('-') + 1);
}

CString CUol::GetObjectClass()
{
	return GetObjectClass(Object);
}

CString CUol::GetObjectId()
{
	return GetObjectId(Object);
}

bool CUol::IsValid(const CUrl & u)
{
	return !u.Scheme.empty() && !u.Path.empty();
}

std::wstring CUol::GetTypeName()
{
	return TypeName;
}

void CUol::Read(CStream * s)
{
	CString t;
	t.Read(s);

	Read(t);
}

int64_t CUol::Write(CStream * s)  
{
	return ToString().Write(s);
}

void CUol::Write(std::wstring & s)
{
	s += ToString();
}

void CUol::Read(const std::wstring & addr)
{
	CString path;

	CUrl::Read(addr, &Scheme, null, &path, &Parameters);

	Server	= ExtractServer(path);
	Object	= ExtractObject(path);
}

ISerializable * CUol::Clone()
{
	return new CUol(*this);
}

bool CUol::Equals(const ISerializable & serializable) const
{
	auto & a = static_cast<CUol const &>(serializable);

	if(Scheme != a.Scheme || Server != a.Server || Object != a.Object)
		return false;

	auto & ap = a.Parameters.begin();
	auto & bp = Parameters.begin();

	while(ap != a.Parameters.end() && bp != Parameters.end())
	{
		if(ap->second != bp->second)
		{
			return false;
		}

		ap++;
		bp++;
	}

	return true;
}

