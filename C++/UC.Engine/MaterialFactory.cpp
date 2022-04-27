#include "StdAfx.h"
#include "MaterialFactory.h"

using namespace uc;

CMaterialFactory::CMaterialFactory(CEngineLevel * l, CDirectSystem * ge, CDirectPipelineFactory * sf) : CEngineEntity(l)
{
	GraphicEngine = ge;
	PipelineFactory = sf;
}

CMaterialFactory::~CMaterialFactory()
{
}
