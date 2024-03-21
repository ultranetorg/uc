#include "StdAfx.h"
#include "VwmCamera.h"

namespace uos
{
	CVwmCamera::CVwmCamera(INode * node, CProcessDlg * pd, CSource * s)
	{
		UI			= pd;
		Source		= s;
		
		MaxCamera = (CameraObject *)node->EvalWorldState(s->MaxInterface->GetTime()).obj;
//		MaxCamera = (Camera *) obj->ConvertToType(s->MaxInterface->GetTime(), Class_ID(CAMERA_CLASS_ID, 0));

		FileName = CString::Format(L"Camera-%s.bon", node->GetName());
	}

	CVwmCamera::~CVwmCamera()
	{
	}

	bool CVwmCamera::AskIsCamera(INode * node, CSource * s)
	{
		if(node->IsHidden() != 0)
		{
			return false;
		}

		CameraObject *obj = (CameraObject *)node->EvalWorldState(s->MaxInterface->GetTime()).obj;
		if(obj==null || obj->CanConvertToType(Class_ID(SIMPLE_CAM_CLASS_ID, 0)) == 0)
		{
			return false;
		}

		return true;
	}
	

	void CVwmCamera::Export(IDirectory * d)
	{
		CBonDocument doc;

		doc.Add(L"Fov")->Set(MaxCamera->GetFOV(Source->MaxInterface->GetTime()));

		auto s = d->OpenWriteStream(FileName);
		doc.Save(&CXonBinaryWriter(s));
		d->Close(s);
	}
}