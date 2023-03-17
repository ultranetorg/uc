#pragma once
#include "VwmSceneExport.h"

HINSTANCE hInstance;

namespace uos
{
	class CMaxPlugin : public ClassDesc  
	{
		public:
			int					IsPublic();
			void			*	Create(BOOL Loading = FALSE);
			const TCHAR		*	ClassName();
			SClass_ID			SuperClassID();
			Class_ID			ClassID();
			const TCHAR		*	Category();
			void				DeleteThis();

			CMaxPlugin();
	};
	
	static CMaxPlugin MaxPlugin;
}


__declspec(dllexport) int				LibNumberClasses();
__declspec(dllexport) ClassDesc		*	LibClassDesc(int i);
__declspec(dllexport) const TCHAR	*	LibDescription();
__declspec(dllexport) ULONG				LibVersion();
