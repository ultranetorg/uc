#pragma once
#include "ProcessDlg.h"
#include "MaxExportHelper.h"

namespace uos
{
	class CVwmCamera
	{
		public:
			CameraObject *								MaxCamera;
			CString										FileName;
			CSource *									Source;
			
			CProcessDlg	*								UI;

			void										Export(IDirectory * w);
			bool static									AskIsCamera(INode * n, CSource * s);

			CVwmCamera(INode * n, CProcessDlg * pd, CSource * s);
			~CVwmCamera();
	};
}