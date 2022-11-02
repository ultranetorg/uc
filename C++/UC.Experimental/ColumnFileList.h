#pragma once
#include "FileItemElement.h"

namespace uc
{
	struct CFileListColumn
	{
		CRefList<CFileItemElement *>					Items;
		CFileItemElement *								Current = null;
		float											Width;
		float											X = 0.f;
		float											Y = 0.f;
		CExperimentalLevel *							Level;
		CString											Path;
		float											LastWidth = 0.f;

		CFileListColumn(CExperimentalLevel * l, CString const & p)
		{
			Level = l;
			Path = p;
		}

		~CFileListColumn()
		{
		}
	};

	class CColumnFileList : public CRectangle
	{
		public:
			CString										Root;
			
			CArray<CFileListColumn *>					Columns;
			CExperimentalLevel *						Level;
			CRectangle *								Selector;
			CFloat2										Scroll = {0, 0};

			CSolidRectangleMesh *						IconMesh;

			CEvent<CString const &>						PathChanged;
						
			UOS_RTTI
			CColumnFileList(CExperimentalLevel * w);
			~CColumnFileList();

			void										SetRoot(CString & u);
			void										SetPath(CString const & u);

			void										SetCurrent(CFileItemElement * i);
			void										OnKeyboard(CActive *, CActive *, CKeyboardArgs *);
			void										OnMouse(CActive *, CActive *, CMouseArgs *);
			void										Clear();
			void										AddColumn(const CString & path);
			void										RemoveColumn(CFileListColumn * c);

			void										Save(CXon * n);
			void										Load(CXon * n);
	};
}