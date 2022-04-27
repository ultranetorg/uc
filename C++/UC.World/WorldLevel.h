#pragma once
#include "Style.h"

namespace uc
{
	struct CWorldLevel : public CLevel2
	{
		CServer *										Server;
		CEngine *										Engine;
		CMaterialPool *									Materials;
		CStyle *										Style;
		CStorage *										Storage;

		CWorldLevel(CLevel2 * l) : CLevel2(*l){}
	};
}