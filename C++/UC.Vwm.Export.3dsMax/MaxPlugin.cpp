#include "stdafx.h"
#include "MaxPlugin.h"

int LibNumberClasses()
{
	return 1;
}

ClassDesc * LibClassDesc(int i)
{
	switch (i)
	{
		case 0: 
			return &uos::MaxPlugin;
		default: 
			return 0;
	}
}

const TCHAR * LibDescription()
{
	return UO_NAME L" " UNO_VWM_EXPORTER;
}

ULONG LibVersion()
{
	return VERSION_3DSMAX;
}

namespace uos
{
	CMaxPlugin::CMaxPlugin()
	{
	}
	
	int CMaxPlugin::IsPublic()
	{
		return 1;
	}

	void * CMaxPlugin::Create(BOOL Loading)
	{
		return new CVwmSceneExport();
	}

	const TCHAR * CMaxPlugin::ClassName()
	{
		return UO_NAME L" " UNO_VWM_EXPORTER;
	}

	SClass_ID CMaxPlugin::SuperClassID()
	{
		return SCENE_EXPORT_CLASS_ID;
	}

	Class_ID CMaxPlugin::ClassID()
	{
		return UNO_3DSMAX_CLASS_ID;
	}

	const TCHAR	* CMaxPlugin::Category()
	{
		return _T("");
	}
}
