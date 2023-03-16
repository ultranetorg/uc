#pragma once
#include "HttpRequest.h"

namespace uc
{
	class CHttpClient
	{
		public:
			CList<CHttpRequest *>					Requests;
			CCore *									Level;

			CHttpClient(CCore * l)
			{
				Level = l;
			}

			~CHttpClient()
			{
			}

			void Send(const CString & url, const CString & method, CList<CString> headers, const CString & content, bool caching, std::function<void(CHttpRequest *)> ok, std::function<void(CHttpRequest *)> failed)
			{
				auto r = new CHttpRequest(Level, url);
				r->Method = method;
				r->Headers = headers;
				r->Content = content.ToAnsi();
				r->Caching = caching;
				
				r->Recieved =	[r, ok]
								{
									ok(r);
									delete r;
								};

				r->Failed =		[r, failed]
								{
									failed(r);
									delete r;
								};
				r->Send();
			}
	};
}


