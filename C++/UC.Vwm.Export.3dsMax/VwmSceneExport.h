#pragma once
#include "stdafx.h"
#include "VwmExporter.h"

namespace uos
{
	class CVwmSceneExport : public SceneExport
	{
		public:
			int											ExtCount();
			const TCHAR*								Ext(int i);
			const TCHAR*								LongDesc();
			const TCHAR*								ShortDesc();
			const TCHAR*								AuthorName();
			const TCHAR*								CopyrightMessage();
			const TCHAR*								OtherMessage1();
			const TCHAR*								OtherMessage2();
			unsigned int								Version();
			void										ShowAbout(HWND hWnd);
			BOOL										SupportsOptions(int ext, DWORD options);
			int											DoExport(const TCHAR *name, ExpInterface *ei, Interface *i, BOOL suppressPromts = FALSE, DWORD options = 0);

			CVwmSceneExport();
			~CVwmSceneExport();
	};
}
