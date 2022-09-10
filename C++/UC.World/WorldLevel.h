#pragma once
#include "Style.h"

namespace uc
{
	struct CWorldLevel
	{
		//CConfig *								Config;
		CPersistentServer *						Server;
		CCore *									Core;
		CNexus *								Nexus;
		CLog *									Log;
		CEngine *								Engine;
		CMaterialPool *							Materials;
		CStyle *								Style;
		CProtocolConnection<CFileSystem>		Storage;
	};
}