#pragma once
#include "IStorageProtocol.h"

namespace uc
{
	class UOS_LINKING CConfig : public IType
	{
		public:
			IStorageProtocol *	Storage;
			CString				DefaultUri;
			CString				CustomUri;

			CXonDocument *		Root = null;
			CXonDocument *		DefaultDoc = null;

			void				Save();

			CXon *				GetRoot();

			UOS_RTTI
			CConfig(IStorageProtocol * l, CString & durl, CString & curl);
			~CConfig();

	};
}