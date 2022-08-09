#pragma once
#include "Core.h"
#include "MemoryStream.h"

namespace uc
{
	class UOS_LINKING CHttpRequest
	{
		public:
			CString										Url;
			CCore *										Level;
			CMemoryStream 								Stream;
			CThread *									Thread;
			CList<CString>								Headers;
			CAnsiString									Content;
			CString										Method = L"GET";
			bool										Caching = false;
			bool										FollowLocatiion = true;
			int											Timeout = 10;

			CURLcode									Result;

			std::function<void()>						Recieved;
			std::function<void()>						Failed;

			CHttpRequest(CCore * l, CString const & url);
			~CHttpRequest();

			void Send();

			static size_t WriteCallback(void *contents, size_t size, size_t nmemb, void *userp);

	};

}


