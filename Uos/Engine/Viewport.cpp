#include "StdAfx.h"
#include "Viewport.h"

using namespace uc;

CViewport::CViewport(CEngineLevel * l) : CEngineEntity(l)
{
}

CViewport::~CViewport()
{
}

CFloat2 CViewport::TargetToViewport(CFloat2 & tp)
{
	return CFloat2(tp.x * W/TW, tp.y * H/TH);
}
