#pragma once
#include "StorableObject.h"

namespace uc
{
	class UOS_LINKING CFile : public CInterObject
	{
		public:
			int											Users = 0;

			UOS_RTTI
			CFile(CServer * s, CString const & name) : CInterObject(s, name){}
	};

	class UOS_LINKING CLocalFile : public CFile
	{
		public:
			CString										Path;
			CList<CStream *>							Streams;

			UOS_RTTI
			CLocalFile(CServer * s, CString const & path, CString const & name);
			~CLocalFile();
			
			static void									Delete(CString const & path);
			static bool									Exists(CString const  & path);
	};
}
