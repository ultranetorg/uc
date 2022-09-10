#pragma once
#include "FileSystem.h"

namespace uc
{
	class UOS_LINKING CConfig : public IType
	{
		public:
			CFileSystem *		Storage;
			CString				DefaultUri;
			CString				CustomUri;

			CXonDocument *		Root = null;
			CXonDocument *		DefaultDoc = null;

			void				Save();

			CXon *				GetRoot();

			UOS_RTTI
			CConfig(CFileSystem * l, CString & durl, CString & curl);
			~CConfig();

	};
}