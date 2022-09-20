#include "stdafx.h"
#include "SunClient.h"

using namespace uc;

CSunClient::CSunClient(CNexus * nexus, CClientInstance * instance) : CClient(instance)
{
	Nexus = nexus;
}

IProtocol * CSunClient::Connect(CString const & iface)
{
	if(iface == CSunProtocol::InterfaceName)
	{
		Http = new CHttpClient(Nexus->Core);

		return this;
	}

	throw CException(HERE, L"Not supported interface");
}

void CSunClient::Disconnect(IProtocol * iface)
{
	cleandelete(Http);
}

void CSunClient::GetSettings(std::function<void(CSunSettings &)> ok)
{
	auto r = L"	{\
					\"Version\":\"1\",\
					\"AccessKey\":\"password\"\
				}";


	Send(L"Settings", r,	[ok](nlohmann::json & j)
							{
								CSunSettings s;

								s.ProfilePath = CString::FromAnsi(j["ProfilePath"].get<std::string>());

								ok(s);
							},
							[]{}
		);
}

void CSunClient::QueryRelease(CList<CReleaseAddress> & releases, std::function<void(nlohmann::json)> ok, std::function<void()> failure)
{
	CString r = L"	{\
					\"Version\":\"1\",\
					\"AccessKey\":\"password\",\
					\"Queries\":[";

	for(auto i : releases)
	{
		r += CString::Format(L"{\"Author\":\"%s\",", i.Author);
		r += CString::Format(L"\"Product\":\"%s\",", i.Product);
		r += CString::Format(L"\"Platform\":\"%s\",", i.Platform);
		r += CString::Format(L"\"Version\":\"%s\",", i.Version.ToString());

		r +=  L"\"VersionQuery\": 2,\
              \"Channel\": \"Stable\"}";

		if(i != releases.Last())
			r += L",";
	}

	r += L"]",
	r += L"}",

	Send(L"QueryRelease", r, ok, failure);
}

void CSunClient::Send(CString const & method, CString const & json, std::function<void(nlohmann::json)> ok, std::function<void()> failure)
{
	Http->Send(L"http://127.0.0.1:30901/" + method, L"GET", {}, json, false,[&, ok, failure](CHttpRequest * r)
																			{
																				nlohmann::json j;

																				try
																				{
																					auto  b = (char *)r->Stream.GetBuffer();
																					j = nlohmann::json::parse(b, b + r->Stream.GetSize());
																			
																					ok(j);
																				}
																				catch(nlohmann::detail::parse_error & e)
																				{
																					failure();
																				}
																			},
																			[&](CHttpRequest * r)
																			{
																				failure();
																			});
}