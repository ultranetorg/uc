#pragma once
#include "FileSystemProtocol.h"

namespace uc
{
	class UOS_LINKING CConfig : public IType
	{
		public:
			CFileSystemProtocol *	Storage;
			CString					DefaultUri;
			CString					CustomUri;

			CXonDocument *			Root = null;
			CXonDocument *			DefaultDoc = null;

			void					Save();

			CXon *					GetRoot();

			UOS_RTTI
			CConfig(CFileSystemProtocol * l, CString & durl, CString & curl);
			~CConfig();

	};
}