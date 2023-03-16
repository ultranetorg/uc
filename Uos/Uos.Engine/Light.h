#pragma once
#include "Shader.h"
#include "EngineLevel.h"

namespace uc
{
	class CDirectPipeline;

	class CLight
	{
		public:
			virtual void								BuildShaderCode(CShader & c)=0;

			virtual void								Apply(CDirectPipeline * p)=0;
		
			CLight(CEngineLevel * e);
			virtual ~CLight();
	};
}