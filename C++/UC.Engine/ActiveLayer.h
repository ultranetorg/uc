#pragma once
#include "ActiveSpace.h"
#include "ScreenViewport.h"

namespace uc
{
	class CActiveLayer : public CEngineEntity
	{
		public:
			CScreenViewport *							Viewport;
			CActiveSpace *								Space;
			
			UOS_RTTI
			CActiveLayer(CEngineLevel * l) : CEngineEntity(l){}
			~CActiveLayer(){}
	};
}
