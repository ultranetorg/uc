#pragma once
#include "BaseNexusObject.h"
#include "XonDocument.h"
#include "FileStream.h"
#include "NativeDirectory.h"

namespace uc
{
	class UOS_LINKING CNexusObject : public CBaseNexusObject//, public virtual IProtocol
	{
		public:
			CString				GlobalDirectory;
			CString				LocalDirectory;
			CXonDocument *		InfoDoc = null;

			UOS_RTTI
			~CNexusObject();

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
			CNexusObject(CServer * s, CString const & name);

			virtual void		SaveInstance();
			virtual void		LoadInstance();
	};

}