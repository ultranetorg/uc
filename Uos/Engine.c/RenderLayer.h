#pragma once
#include "DirectVisualGraph.h"
#include "ScreenViewport.h"

namespace uc
{
	class CScreenRenderLayer : public CEngineEntity
	{
		public:
			CDirectVisualGraph *					Graph;
			CVisualSpace *								Space;
			CScreenViewport *							Viewport;
			
			UOS_RTTI
			CScreenRenderLayer(CEngineLevel * l) : CEngineEntity(l){}
			~CScreenRenderLayer(){}
	};
}
