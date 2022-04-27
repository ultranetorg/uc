#include "StdAfx.h"
#include "VwmLight.h"

namespace uos
{
	CVwmLight::CVwmLight(INode * node, CProcessDlg * pd, CSource * s)
	{
		UI			= pd;
		Source		= s;

		TimeValue t = s->MaxInterface->GetTime();
		
		MaxLight = (LightObject *)node->EvalWorldState(t).obj;

		Matrix3 m = node->GetObjTMAfterWSM(t);
		m.NoScale();
		m.NoTrans();
		Direction = (Point3(0, 0, -1) * m).Normalize();

		DiffuseIntensity = MaxLight->GetIntensity(t);

		FileName = CString::Format(L"Light-%s.bon", node->GetName());
	}

	CVwmLight::~CVwmLight()
	{
	}

	bool CVwmLight::AskIsLight(INode * node, CSource * s)
	{
		if(node->IsHidden() != 0)
		{
			return false;
		}

		LightObject * obj = (LightObject *)node->EvalWorldState(s->MaxInterface->GetTime()).obj;
		if(obj == null || obj->CanConvertToType(Class_ID(DIR_LIGHT_CLASS_ID, 0)) == 0)
		{
			return false;
		}

		return true;
	}
	

	void CVwmLight::Export(IDirectory * d)
	{
		CBonDocument doc;

		if(MaxLight->ClassID() == Class_ID(DIR_LIGHT_CLASS_ID, 0))
		{
			doc.Add(L"Type")->Set(L"Directional");
			doc.Add(L"Direction")->Set((ISerializable &)CFloat3(Direction.x, Direction.z, Direction.y));
			doc.Add(L"DiffuseIntensity")->Set((float)DiffuseIntensity);
		}

		auto s = d->OpenWriteStream(FileName);
		doc.Save(&CXonBinaryWriter(s));
		d->Close(s);
	}
}