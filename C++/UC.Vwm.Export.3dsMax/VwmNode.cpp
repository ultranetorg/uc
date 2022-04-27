#include "StdAfx.h"
#include "VwmNode.h"

using namespace uos;

CVwmNode::CVwmNode(CVwmNode * parent, INode * node, CProcessDlg * ui, CSource * source)
{
	MaxNode			= node;
	Source			= source;
	UI				= ui;
	IsBackCull		= node->GetBackCull()!=0;
	IsReceiveLight	= node->RcvGlobalIllum()!=0;

	Name = MaxNode->GetName();

	TimeValue t = Source->MaxInterface->GetTime();

	if(parent == null)
	{
		Matrix = node->GetNodeTM(t);
	}
	else
	{
		Matrix =  node->GetNodeTM(t) * Inverse(node->GetParentTM(t));
	}
		
	Matrix3 zy(	Point3(1, 0, 0), 
				Point3(0, 0, 1), 
				Point3(0, 1, 0), 
				Point3(0, 0, 0));
	Matrix = zy * Matrix * zy;
		
	AffineParts ap;
	decomp_affine(Matrix, &ap);
	Transformation.Position	= CFloat3		(ap.t.x, ap.t.y, ap.t.z);
	Transformation.Rotation	= CQuaternion	(-ap.q.x, -ap.q.y, -ap.q.z, ap.q.w);
	Transformation.Scale	= CFloat3		(ap.k.x, ap.k.y, ap.k.z);
		

	if(parent)
	{
		MSTR s;
		MaxNode->GetUserPropBuffer(s);

		Props = new CTonDocument(CXonTextReader(s.data()));
	}
}
		
CVwmNode::CVwmNode(CVwmNode * parent, CVwmMesh * mesh, Mtl * mtl, CMeshFactory * meshf, CMaterialFactory * mtlf, CProcessDlg * ui, CSource * source)
{
	MaxNode			= null;
	Source			= source;
	UI				= ui;
	IsBackCull		= parent->IsBackCull;
	IsReceiveLight	= parent->IsReceiveLight;

	Name			= L"<Sub>-"+parent->Name;

	Matrix.IdentityMatrix();
	Transformation = CTransformation::Identity;

	VwmMesh			= mesh; //meshf->GetVwmMesh(ownernode, to, mtlId, multi->NumSubs());
	VwmMaterial		= mtlf->GetVwmMaterial(mtl);
}

	
void CVwmNode::MakeVisual(CMeshFactory * meshf, CMaterialFactory * mtlf)
{
	Mtl * mtl = MaxNode->GetMtl();

	if(mtl!=null)
	{
		if(!mtl->IsMultiMtl()) // Use one
		{
			VwmMesh		= meshf->GetVwmMesh(MaxNode);
			VwmMaterial	= mtlf->GetVwmMaterial(mtl);
		}
		else
		{
			VwmMesh		= null;
			VwmMaterial	= null;

			MultiMtl * multi = static_cast<MultiMtl*>(mtl);

			CArray<CVwmMesh *> meshes = meshf->GetVwmMeshes(MaxNode, multi);
			for(int id=0; id<int(meshes.size()); id++)
			{
				if(meshes[id] != null)
				{
					Children.insert(new CVwmNode(this, meshes[id], multi->GetSubMtl(id), meshf, mtlf, UI, Source));
				}
			}
		}
	}
	else
	{
		VwmMesh		= meshf->GetVwmMesh(MaxNode);
		VwmMaterial	= null;
	}
}
	
void CVwmNode::MakeCamera()
{
	VwmCamera = new CVwmCamera(MaxNode, UI, Source);
}
	
void CVwmNode::MakeLight()
{
	VwmLight = new CVwmLight(MaxNode, UI, Source);
}
	
CVwmNode::~CVwmNode()
{
	delete VwmCamera;
	delete Props;
}
		
void CVwmNode::Export(CXon * graph, IDirectory * w)
{
	CXon * mwxNode = graph->Add(L"Node");

	mwxNode->Set(Name);
		
	mwxNode->Add(L"BackCull")->Set(IsBackCull);
	CMaxExportHelper::ExportMatrix3(Matrix, mwxNode->Add(L"Matrix"));
	mwxNode->Add(L"Transformation")->Set(Transformation);

	if(Props)
	{
		if(Props->One(L"Active"))
		{
			auto a = mwxNode->Add(L"Active");
			if(VwmMesh)
			{
				a->Add(L"Mesh")->Set(VwmMesh->FileName);
			}
		}
	}

	if(VwmMesh && VwmMaterial)
	{
		auto v = mwxNode->Add(L"Visual");
		if(VwmMesh)
		{
			v->Add(L"Mesh")->Set(VwmMesh->FileName);
		}
		if(VwmMaterial)
		{
			v->Add(L"Material")->Set(VwmMaterial->FileName);
		}
		v->Add(L"IsReceiveLight")->Set(IsReceiveLight);
	}

	if(VwmCamera)
	{
		mwxNode->Add(L"Camera")->Set(VwmCamera->FileName);
		VwmCamera->Export(w);
	}

	if(VwmLight)
	{
		mwxNode->Add(L"Light")->Set(VwmLight->FileName);
		VwmLight->Export(w);
	}

	for(auto i : Children)
	{
		i->Export(mwxNode, w);
	}
}

void CVwmNode::AddChild(CVwmNode * c)
{
	Children.insert(c);
}
