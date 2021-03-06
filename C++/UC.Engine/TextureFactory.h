#pragma once
#include "Texture.h"

namespace uc
{
	class UOS_ENGINE_LINKING CTextureFactory : public CEngineEntity
	{
		public:
			UOS_RTTI
			CTextureFactory(CEngineLevel * l, CDirectSystem * ge);
			~CTextureFactory();

			CTexture *									CreateTexture();
			
			bool										IsSupported(CStream * s);
			
			CList<std::pair<CString, CString>>			Formats;

			CSize										GetDimension(CStream * s);

		private:
			void										CopyTexture(CTexture * s, CTexture * d);
			CDirectSystem *								GraphicEngine;
			//int										MipLevelNumber;
			

//			TextureFile * 								Find(CString path);

	};
}