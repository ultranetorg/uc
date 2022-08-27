#pragma once
#include "InterObject.h"
#include "XonDocument.h"
#include "LocalFileStream.h"
#include "NativeDirectory.h"

namespace uc
{
	class UOS_LINKING CStorableObject : public CInterObject
	{
		public:
			CString				GlobalDirectory;
			CString				LocalDirectory;
			CXonDocument *		Info = null;

			UOS_RTTI
			~CStorableObject();

			virtual void		SetDirectories(CString const & path);
			void				Load();
			void				Save();
			bool				IsSaved();
			void				Delete();
			void				SaveGlobal(CTonDocument & d, CString const & path);
			void				LoadGlobal(CTonDocument & d, CString const & path);

			CString				AddGlobalReference(CUol & r);
			void				RemoveGlobalReference(CUol & l, CString const & t);

			CXon *				GetInfo(CUol & r);
			CXon *				AddInfo(CUol & r);

			void				LoadInfo(CStream * s);
			void				SaveInfo(CStream * s);

		protected:
			CStorableObject(CString const & scheme, CServer * server, CString const & name);

			virtual void		SaveInstance();
			virtual void		LoadInstance();
	};

}