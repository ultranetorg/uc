#pragma once
#include "Font.h"
#include "DirectSystem.h"

namespace uc
{
	class UOS_ENGINE_LINKING CFontFactory
	{
		public:
			CFont *										GetFont(const CString & family, float size, bool bold, bool italic);
			CFont *										GetFont(CFontDefinition & desc);

			CFontFactory(CEngineLevel * l, CDirectSystem * ge, CScreenEngine * se);
			~CFontFactory();
		
		private:
			CList<CFont *>								Fonts;
			CEngineLevel *								Level;
			CDirectSystem *								GraphicEngine;
			CScreenEngine *								ScreenEngine;
	};
}