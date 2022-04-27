#include "StdAfx.h"
#include "MeshFactory.h"

namespace uos
{
	CMeshFactory::CMeshFactory(CSource * s, CProcessDlg * pd, CWritePool * wp)
	{
		Source		= s;
		UI			= pd;
		WritePool	= wp;
		IdCounter	= 0;
	}

	CMeshFactory::~CMeshFactory()
	{
		Clear();
	}

	bool CMeshFactory::AskIsValidTriObj(INode * node)
	{
		if(node->IsHidden() != 0)
		{
			return false;
		}

		Object *obj = node->EvalWorldState(Source->MaxInterface->GetTime()).obj;
		if(obj==null || obj->CanConvertToType(Class_ID(TRIOBJ_CLASS_ID,0)) == 0)
		{
			return false;
		}

		return true;
	}

	TriObject * CMeshFactory::GetTriObjFromNode(INode *node)
	{
		Object *obj = node->EvalWorldState(Source->MaxInterface->GetTime()).obj;
		if(obj->CanConvertToType(Class_ID(TRIOBJ_CLASS_ID,0)))
		{
			TriObject *to = (TriObject *) obj->ConvertToType(Source->MaxInterface->GetTime(), Class_ID(TRIOBJ_CLASS_ID, 0));

			if(obj != to && TriObjs.find(to) == TriObjs.end())
			{
				TriObjs.insert(to);
			}
			return to;
		}
		else 
		{
			return NULL;
		}
	}

	CVwmMesh * CMeshFactory::GetVwmMesh(INode * node)
	{
		TriObject * to = GetTriObjFromNode(node);
		Object *obj = node->EvalWorldState(Source->MaxInterface->GetTime()).obj;

		if(to != null)
		{
			for(auto i : Meshes) // найти меш который уже содержит заданный obj
			{
				if(i->MaxObject == obj && !i->IsSyntesized)
				{
					return i;
				}
			}
			CVwmMesh * vwmMesh = new CVwmMesh(node, to, UI, IdCounter, Source);
			Meshes.push_back(vwmMesh);
			IdCounter++;
			return vwmMesh;
		}
		else
			return null;
	}

	CArray<CVwmMesh *> CMeshFactory::GetVwmMeshes(INode * node, MultiMtl* multi)
	{
		TriObject * to = GetTriObjFromNode(node);

		int mtlNum		= multi->NumSubs();
		int facesNum	= to->mesh.getNumFaces();

		CArray<CVwmMesh *> a(mtlNum, nullptr);

		for(int i=0; i<facesNum; i++)
		{
			int mtlId = to->mesh.getFaceMtlIndex(i) % mtlNum;
			if(a[mtlId] == null)
			{
				Mtl * subMtl = multi->GetSubMtl(mtlId);
				if(subMtl != null)
				{
					CVwmMesh * vwmMesh = new CVwmMesh(node, to, mtlId, mtlNum, UI, IdCounter, Source);
					Meshes.push_back(vwmMesh);
					a[mtlId] = vwmMesh;
				}/*
				else
				{
					UI->ReportError(L"The %d`th face from '%s' node has no material\r\n", i, node->GetName());
					continue;
				}*/
			}
			UI->SetProgressCurrent(i*100/(to->mesh.numFaces-1));
		}

		IdCounter++;

		return a;
	}
	
	void CMeshFactory::Clear()
	{
		IdCounter = 0;
		for(auto i : Meshes)
		{
			delete i;
		}
		Meshes.clear();

		for(auto i : TriObjs)
		{
			i->DeleteMe();
		}
		TriObjs.clear();
	}

	void CMeshFactory::Export(IDirectory * w, int start, int len)
	{
		int c=1;
		for(auto i : Meshes)
		{
			i->Export(w);
			UI->SetProgressOverall(start+(len*c)/(int)Meshes.size());
			c++;
		}
	}

/*
	bool CMeshFactory::AskIsCamera(INode * node)
	{
		if(node->IsHidden() != 0)
		{
			return false;
		}

		Object *obj = node->EvalWorldState(Source->MaxInterface->GetTime()).obj;
		if(obj->CanConvertToType(Class_ID(CAMERA_CLASS_ID,0)) == 0)
		{
			return false;
		}

		return true;
	}*/

}
