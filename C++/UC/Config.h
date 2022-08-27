#pragma once
#include "IFileSystem.h"

namespace uc
{
	class UOS_LINKING CConfig : public IType
	{
		public:
			IFileSystem *		Storage;
			CString				DefaultUri;
			CString				CustomUri;

			CXonDocument *		Root = null;
			CXonDocument *		DefaultDoc = null;

			void				Save();

			CXon *				GetRoot();

			UOS_RTTI
			CConfig(IFileSystem * l, CString & durl, CString & curl);
			~CConfig();

	};
}