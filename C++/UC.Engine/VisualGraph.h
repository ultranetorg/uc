#pragma once
#include "VisualSpace.h"
#include "RenderTarget.h"
#include "PipelineFactory.h"

namespace uc
{
	class CVisualGraph : public CVisualSpace
	{
		public:
			CDiagnostic *								Diagnostic;
			CDiagGrid									DiagGrid;
			CDirectPipelineFactory *					PipelineFactory;
		
			virtual void								Render(CRenderTarget * t, CViewport * vp, CVisualSpace * s)=0;
	
			void										AssignPipeline(CVisual * v, CMap<CDirectPipeline *, int> & pipelines);

			void										OnDiagnosticsUpdate(CDiagnosticUpdate & a);

			UOS_RTTI
			CVisualGraph(CEngineLevel * l, CDirectPipelineFactory * pf);
			~CVisualGraph();
	};
}