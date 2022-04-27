#pragma once
#include "ScreenEngine.h"
#include "PipelineFactory.h"
#include "RenderLayer.h"
#include "ScreenViewport.h"

namespace uc
{
	class UOS_ENGINE_LINKING CRenderer : public CEngineEntity
	{
		public:
			CDirectPipelineFactory *					PipelineFactory;
			IPerformanceCounter *						PcUpdate;
			CDiagnostic *								Diagnostic;
			CDirectSystem *								GraphicEngine;
			CList<CScreenRenderLayer *>					Layers;
			CList<CScreenRenderTarget *>				ScreenTargets;

			UOS_RTTI
			CRenderer(CEngineLevel * l, CDirectSystem * ge, CDirectPipelineFactory * sf, CMaterialFactory * mf);
			~CRenderer();

			CScreenRenderLayer *						AddLayer(CScreenViewport * w, CVisualGraph * g, CVisualSpace * space);
			CScreenRenderLayer *						AddLayer(CDisplayDevice * dd);
			void										RemoveLayer(CScreenRenderLayer * t);
		
			void										Update();
			void										RenderSpace(CRenderTarget * t, CViewport * vp, CVisualGraph * g, CVisualSpace * s);

			void										OnDiagnosticsUpdate(CDiagnosticUpdate & a);
	};
}