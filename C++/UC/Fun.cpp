#include "stdafx.h"
#include "Fun.h"

using namespace uc;

CFunClient::CFunClient(CNexus * nexus, CClientInstance * instance) : CClient(instance)
{
	Nexus = nexus;
}

IInterface * CFunClient::Connect(CString const & iface)
{
	if(iface == CFun::InterfaceName)
	{
		Http = new CHttpClient(Nexus->Core);

		return this;
	}

	throw CException(HERE, L"Not supported interface");
}

void CFunClient::Disconnect(IInterface * iface)
{
	cleandelete(Http);
}

CFunSettings CFunClient::GetSettings()
{
	auto r = L"	{\
					\"Version\":\"1\",\
					\"AccessKey\":\"password\",\
				}";

	CFunSettings s;

	bool done = false;

	Http->Send(L"http://127.0.0.1:3090/Settings", L"GET", {}, r, false,	[&](CHttpRequest * r)
																		{

																			auto b = r->Stream.Read();
																			auto t = CAnsiString((const char *)b.GetData(), (int)b.GetSize());
										
																			nlohmann::json j;

																			try
																			{
																				j = nlohmann::json::parse(t);
																			}
																			catch(nlohmann::detail::parse_error &)
																			{
																				//Level->Log->ReportError(this, L"Json parse error: %s", Request->Url);
																				return;
																			}

																			s.ProfilePath = j["ProfilePath"];

																			done = true;
																		},
																		[&](CHttpRequest * r)
																		{
																			done = true;
																		});

	Nexus->Core->RunThread(L"GetSettings", [&done]{ while(!done){ Sleep(1); } }, []{});

	return s;
}
