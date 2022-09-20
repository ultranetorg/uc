#pragma once
#include "WorldLevel.h"

namespace uc
{
	class UOS_WORLD_LINKING CWorldView : public IType, public CView
	{
		public:
			CWorldLevel *								Level;
			CString										Name;
			//CSize										Area;

			CString &									GetName();
			CCamera *									GetCamera(CViewport * s);

			CCamera *									AddCamera(CViewport * vp, float fov, float znear, float zfar);
			CCamera *									AddCamera(CViewport * vp, float znear, float zfar);
			
			UOS_RTTI
			CWorldView(CWorldLevel * l2, const CString & name);
			~CWorldView();
	};
}