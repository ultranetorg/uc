#pragma once
#include "Converter.h"
#include "Version.h"

namespace uc
{
	struct CProductInfo
	{
		CString			HumanName;
		CString			Name;
		CString			Description;
		CString			Stage;
		CString			Build;
		CString			AuthorTitle;
		CString			AuthorAbbreviation;
		CString			Copyright;
		CString			WebPageHome;
		CString			WebPageSupport;
		CVersion		Version;
		CString			Platform;

		CString ToString(CString const & a)
		{	
			CString o;
			for(auto i : a)
			{
				if(!o.empty())
					o += L" ";

				if(i == L'N')
					o += HumanName;
				if(i == L'I')
					o += Name;
				if(i == L'V')
					o += Version.ToString();
				if(i == L'S')
					o += Stage;
				if(i == L'P')
					o += Platform;
				if(i == L'B')
					o += Build;
			}
			return o;
		}

	};
}
	
