#pragma once
#include "ExperimentalLevel.h"
#include "FileItemElement.h"

namespace uc
{
	class CStandardFileList : public CRectangle
	{
		public:
			CProtocolConnection<CImageExtractor>		FileExtractor;
			
			CString										Source;
			CList<CString>								Paths;

			CRefList<CFileItemElement *>				Items;
			CExperimentalLevel *						Level;
			CFileItemElement *							Current = null;
			CArray<float>								Columns;
			CRectangle *								Selector;
			CFloat2										Scroll = CFloat2(0,0);

			CSolidRectangleMesh *						IconMesh;

			CEvent<CString const &>						PathChanged;

			UOS_RTTI
			CStandardFileList(CExperimentalLevel * w);
			~CStandardFileList();

			void										SetSource(CString & u);
			void										OnDependencyDisconnecting(CServer *, IProtocol *, CString &);

			void										SetCurrent(CFileItemElement * i);
			void										Load(CString path);

			virtual void								PropagateLayoutChanges(CElement * n) override;

			void										OnMouse(CActive *, CActive *, CMouseArgs *);
			void										OnKeyboard(CActive * r, CActive * s, CKeyboardArgs * arg);
	};

}