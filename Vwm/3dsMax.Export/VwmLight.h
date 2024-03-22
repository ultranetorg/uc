#pragma once
#include "ProcessDlg.h"
#include "MaxExportHelper.h"

namespace uos
{
	class CVwmLight
	{
		public:
			LightObject *								MaxLight;
			CString										FileName;
			CSource *									Source;
			CProcessDlg	*								UI;
			Point3										Direction;
			float										DiffuseIntensity;

			void										Export(IDirectory * w);
			bool static									AskIsLight(INode * n, CSource * s);

			CVwmLight(INode * n, CProcessDlg * pd, CSource * s);
			~CVwmLight();
	};
}