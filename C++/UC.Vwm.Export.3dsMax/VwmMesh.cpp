#include "StdAfx.h"
#include "VwmMesh.h"

namespace uos
{
	void CVwmMesh::ConstructCommon(INode * node, TriObject * to, CProcessDlg * ui, CSource * s)
	{
		MaxObject		= node->EvalWorldState(s->MaxInterface->GetTime()).obj;
		MaxTriObj		= to;
		MaxMesh			= &to->GetMesh();
		Failed			= false;

		MaxMesh->checkNormals(true);

		UI				= ui;
		HasNormals		= MaxMesh->normalCount > 0;
		HasTVertexes	= MaxMesh->numTVerts > 0;
		HasCVertexes	= MaxMesh->numCVerts > 0;

		Matrix3 nodeTMi = node->GetNodeTM(s->MaxInterface->GetTime());
		nodeTMi.Invert();
		
		OffsetMatrix = node->GetObjectTM(s->MaxInterface->GetTime()) * nodeTMi;
		
		OffsetMatrixForNormals = OffsetMatrix;
		OffsetMatrixForNormals.Orthogonalize();
		OffsetMatrixForNormals.NoScale();
		OffsetMatrixForNormals.NoTrans();

		IsMatrixFlipped =  node->GetNodeTM(s->MaxInterface->GetTime()).Parity()!=0;
		//IsMatrixFlipped = false;

		SpecifiedNormals = MaxMesh->GetSpecifiedNormals();
		if(SpecifiedNormals != null)
		{
			SpecifiedNormals->CheckNormals();
		}

		Box3 b = MaxMesh->getBoundingBox();
		b.pmin = b.pmin * OffsetMatrix;
		b.pmax = b.pmax * OffsetMatrix;
		BBox = CAABB(CFloat3(b.pmin.x, b.pmin.z, b.pmin.y), CFloat3(b.pmax.x, b.pmax.z, b.pmax.y));
	}

	CVwmMesh::CVwmMesh(INode * node, TriObject * to, CProcessDlg * ui, int id, CSource * s)
	{
		ConstructCommon(node, to, ui, s);
	
		IsSyntesized	= false;
		FileName		= CString::Format(L"Mesh-%08d.bon", id);
		
		for(int i=0; i<MaxMesh->numFaces; i++)
		{
			AddFace(i);
			UI->SetProgressCurrent(((i+1)*100)/MaxMesh->numFaces);
		}
	}
	
	CVwmMesh::CVwmMesh(INode * node, TriObject * to, int mtlId, int subMtlN, CProcessDlg * ui, int id, CSource * s)
	{
		ConstructCommon(node, to, ui, s);

		IsSyntesized	= true;
		FileName		= CString::Format(L"Mesh-%08d-%d.bon", id, mtlId);

		for(int i=0; i<MaxMesh->numFaces; i++)
		{
			int mid = MaxMesh->getFaceMtlIndex(i) % subMtlN;
			if(mid == mtlId)
			{
				AddFace(i);
			}
			UI->SetProgressCurrent(((i+1)*100)/MaxMesh->numFaces);
		}
	}

	CVwmMesh::~CVwmMesh()
	{
	}
	
	bool CVwmMesh::AskIsValid()
	{
		if(Failed)
		{
			return false;
		}
		if(Positions.empty() || Indexes.empty())
		{
			return false;
		}
		return true;
	}
	
	void CVwmMesh::AddFace(int fi)
	{
		Point3		v;
		Point3		n;
		UVVert		tv;
		VertColor	cv;
		CVertexKey	vk;
//		int			face[3];

		auto count = Indexes.size();
		Indexes.resize(count+3);
		for(int j=0; j<3; j++)
		{
			int vi	= MaxMesh->faces[fi].v[j];

			v = MaxMesh->verts[vi] * OffsetMatrix;
			vk.Position.x = v.x;
			vk.Position.y = v.z;		//
			vk.Position.z = v.y;		// 3dsMax -> DirectX coordinate system

			if(HasNormals)
			{
				n = GetVertexNormal(fi, j);
				vk.Normal.x = n.x;	
				vk.Normal.y = n.z;	//
				vk.Normal.z = n.y;	// 3dsMax -> DirectX coordinate system
			}

			if(HasTVertexes)
			{
				int ti	= MaxMesh->tvFace[fi].t[j];
				tv		= MaxMesh->tVerts[ti];
				vk.UV.x = tv.x;
				vk.UV.y = 1.0f - tv.y;
			}

			if(HasCVertexes)
			{
				int ci = MaxMesh->vcFace[fi].t[j];
				cv	= MaxMesh->vertCol[ci];
				vk.VColor = VertColorToDWORD(cv);
			}

			int	index = -1;
			VertexMap::iterator fi = Hashmap.find(vk);
			if(fi != Hashmap.end())
			{
				index = fi->second;
			}
			else
			{
				index = (int)Positions.size();
				if(index == INT_MAX)
				{
					Failed = true;
					UI->ReportError(L"Some mesh have exceeded vertex limit\r\n");
				}

				Positions.push_back(vk.Position);
				if(HasNormals)		Normals.push_back(vk.Normal);
				if(HasTVertexes)	UVs.push_back(vk.UV);
				if(HasCVertexes)	VColors.push_back(vk.VColor);

				Hashmap[vk] = index;
			}
			Indexes[IsMatrixFlipped ? count+j : count+2-j] = index;
		}
	}
	 
	Point3 CVwmMesh::GetVertexNormal(int fi, int j)
	{
		Point3 out;
	
		bool specified = false;
		if(SpecifiedNormals != null)
		{
			int normalID = SpecifiedNormals->GetNormalIndex(fi, j);
			if(normalID!=-1 && SpecifiedNormals->GetNormalExplicit(normalID))
			{
				out = SpecifiedNormals->GetNormal(fi, j) * OffsetMatrixForNormals;
				specified = true;
			}
		}

		if(!specified)
		{
			auto f		= &MaxMesh->faces[fi];
			auto smg	= f->smGroup;
			auto rv		= MaxMesh->getRVertPtr(f->v[j]);
	
			if(rv && rv->rFlags & SPECIFIED_NORMAL)
			{
				out = rv->rn.getNormal() * OffsetMatrixForNormals;
			}
			else if(rv && smg)
			{
				int numNormals	= rv->rFlags & NORCT_MASK;

				if(numNormals == 1)
				{
					out = rv->rn.getNormal() * OffsetMatrixForNormals;
				}
				else if(numNormals>1)
				{
					int i;
					for(i=0; i<numNormals; i++)
					{
						if((rv->ern[i].getSmGroup() & smg)== smg)
						{
							out = rv->ern[i].getNormal() * OffsetMatrixForNormals;
							break;
						}
					}
					if(i==numNormals)
					{
						Failed = true;
						UI->ReportError(L"Normal for %d smooth group not found\r\n", smg);
					}
				}
			}
			else
			{
				// нормаль вершины не задана (не указан Smooth, например для сферы)
				out =  MaxMesh->getFaceNormal(fi); // не умножать на матрицу(проблема отвертки)!!!!!!!!!!!!!!!!!!!!
			}
		}

		out = out.Normalize();
		return out;
	}	

	int CVwmMesh::VertColorToDWORD(VertColor & vc)
	{
		BYTE r = (BYTE)(vc.x * 255.0f);
		BYTE g = (BYTE)(vc.y * 255.0f);
		BYTE b = (BYTE)(vc.z * 255.0f);

		return ((int) (((0x00) << 24) | ((r) << 16) | ((g) << 8) | (b)));
	}

	void CVwmMesh::Export(IDirectory * d)
	{	
		if(AskIsValid())
		{
			CBonDocument doc;
			
			doc.Add(L"Indexes")->Set(Indexes);
			doc.Add(L"Vertexes")->Set(Positions);
			CArray<CFloat3> bb;
			bb.push_back(BBox.Min);
			bb.push_back(BBox.Max);
			doc.Add(L"BBox")->Set(bb);
			
			if(HasNormals)
			{
				doc.Add(L"Normals")->Set(Normals);
			}
			
			if(HasTVertexes)	
			{
				doc.Add(L"UVs")->Set(UVs);
			}
			
			if(HasCVertexes)
			{
				doc.Add(L"VColors")->Set(VColors);
			}
	
			auto s = d->OpenWriteStream(FileName);
			doc.Save(&CXonBinaryWriter(s));
			d->Close(s);
		}
	}
}
