#include "StdAfx.h"
#include "VwmMaterial.h"

#include "3dsMax/iparamb2.h"
#include "3dsMax/materials/NormalBump/normalrender.h"

namespace uos
{
	CVwmMaterial::CVwmMaterial(CSource * s,CProcessDlg * pd, Mtl * mtl, int c, TimeValue t, CWritePool * afm)
	{
		UI			= pd;
		Time		= t;
		MaxMtl		= mtl;
		WritePool	= afm;
		IsExported	= false;
		Source		= s;
		
		FileName = CString::Format(L"Material-%04d-%s.bon", c, MaxMtl->GetName().data());
	}
	
	CVwmMaterial::~CVwmMaterial()
	{
	}
	
	void CVwmMaterial::Export(IDirectory * d)
	{
		Directory = d;
	
		CBonDocument doc;
		///auto root = doc.Add(L"Material");
		ExportMaterial(&doc, MaxMtl);

		auto s = Directory->OpenWriteStream(FileName);
		doc.Save(&CXonBinaryWriter(s));
		Directory->Close(s);

		IsExported	= true;
	}
	
	CXon * CVwmMaterial::ExportMaterial(CXon * parent, Mtl * mtl)
	{
		Class_ID cid = mtl->ClassID();

		if(cid == Class_ID(DMTL_CLASS_ID, 0))
		{
			return ExportStandardMaterial(parent, mtl);
		}
		else if(cid == Class_ID(MIXMAT_CLASS_ID, 0))
		{
			return ExportBlendMaterial(parent, mtl);
		}
		else if(cid == Class_ID(BAKE_SHELL_CLASS_ID, 0))
		{
			return ExportShellMaterial(parent, mtl);
		}
		else
		{
			UI->ReportError(L"Material '%s' - Only Standard/Blend/Shell materials are currently supported.\r\n", mtl->GetFullName().data());
			return null;
		}
	}

	CXon * CVwmMaterial::ExportStandardMaterial(CXon * node, Mtl * material)
	{
		node->Add(L"Type")->Set(L"Standard");
		node->Add(L"Name")->Set(MaxMtl->GetName().data());

		StdMat * stdm = (StdMat *)material;
			
/*		if(material->GetSubTexmap(ID_AM) == null)
		{
			node->CreateNode(L"Ambient")->SetVector3Float32(CFloat3(stdm->GetAmbient(Time).r, stdm->GetAmbient(Time).g, stdm->GetAmbient(Time).b));
		} */
		if(material->GetSubTexmap(ID_DI) == null)
		{
			node->Add(L"Diffuse")->Set((ISerializable &)CFloat3(stdm->GetDiffuse(Time).r, stdm->GetDiffuse(Time).g, stdm->GetDiffuse(Time).b));
		}
		if(material->GetSubTexmap(ID_OP) == null && stdm->GetOpacity(Time) < 1.f)
		{
			node->Add(L"Alpha")->Set(stdm->GetOpacity(Time));
		}
		/*
		if(material->GetSubTexmap(ID_SP) == null)
		{
			node->CreateNode(L"Specular")->SetVector3Float32(CFloat3(stdm->GetSpecular(Time).r, stdm->GetSpecular(Time).g, stdm->GetSpecular(Time).b));
		}*/
		node->Add(L"Shininess")->Set(stdm->GetShininess(Time) + stdm->GetShinStr(Time));

		ExportMaps(material, node);
		
		return node;
	}

	CXon * CVwmMaterial::ExportBlendMaterial(CXon * node, Mtl * material)
	{
		node->Add(L"Type")->Set(L"Blend");

		for(int i=0; i<material->NumSubMtls(); i++)
		{
			ExportMaterial(node->Add(L"Material"), material->GetSubMtl(i));
		}

		IParamBlock2	*	pb	= material->GetParamBlock(0);
		ParamBlockDesc2 *	pbd = pb->GetDesc();

		int index = pbd->NameToIndex(L"mask");
		int id = pbd->IndextoID(index);
		Texmap * mask = pb->GetTexmap(id);

		if(mask->ClassID() == Class_ID(VCOL_CLASS_ID, 0))
		{
			node->Add(L"MaskType")->Set(L"VertexColor");
		}
		return node;
	}

	CXon * CVwmMaterial::ExportShellMaterial(CXon * node, Mtl * material)
	{
		IParamBlock2	*	pb	= material->GetParamBlock(0);
		ParamBlockDesc2 *	pbd = pb->GetDesc();

		int rmIndex = pb->GetInt(pbd->IndextoID(pbd->NameToIndex(L"renderMtlIndex")), Time);

		return ExportMaterial(node->Add(L"Material"), material->GetSubMtl(rmIndex));
	}

	void CVwmMaterial::ExportMaps(Mtl * material, CXon * materialNode)
	{
		ExportDiffuseMap(material, materialNode);
//		ExportBumpMap(material, materialNode);
		ExportOpacityMap(material, materialNode);
		ExportReflectionMap(material, materialNode);
//		ExportSelfIlluminationMap(material, materialNode);
	}

	void CVwmMaterial::ExportDiffuseMap(Mtl * material, CXon * materialNode)
	{
		Texmap * diffuse = material->GetSubTexmap(ID_DI);
		if(diffuse != null) 
		{
			if(diffuse->ClassID() == Class_ID(BMTEX_CLASS_ID, 0))
			{
				CXon * mapNode = materialNode->Add(L"DiffuseMap");
				ExportBitmapTexture(diffuse, mapNode);
			}
			else
			{
				UI->ReportError(L"Material %s - Diffuse map has incompatible properties\r\n", material->GetName().data());
			}
		}
	}

	void CVwmMaterial::ExportOpacityMap(Mtl * material, CXon * materialNode)
	{
		Texmap * opacity = material->GetSubTexmap(ID_OP);
		if(opacity != null) 
		{
			auto id = opacity->ClassID();
			if(id == Class_ID(BMTEX_CLASS_ID, 0))
			{
				CXon * mapNode = materialNode->Add(L"AlphaMap");
				ExportBitmapTexture(opacity, mapNode);
			}
			else
			{
				UI->ReportError(L"Material %s - Opacity map has incompatible properties\r\n", material->GetFullName().data());
			}
		}
	}

	void CVwmMaterial::ExportBumpMap(Mtl * material, CXon * materialNode)
	{
		Texmap * bump = material->GetSubTexmap(ID_BU);
		if(bump != null) 
		{

			if(bump->ClassID() == Class_ID(GNORMAL_CLASS_ID))
			{
				Gnormal *gn = (Gnormal *)bump;

				IParamBlock2	* pb		= bump->GetParamBlock(0);
				Texmap			* normapMap	= pb->GetTexmap(gn_map_normal, Time);

				if(normapMap->ClassID() == Class_ID(BMTEX_CLASS_ID, 0))
				{
					CXon * mapNode = materialNode->Add(L"NormalMap");
					ExportBitmapTexture(normapMap, mapNode);
				}			
			}
			else if(bump->ClassID() == Class_ID(BMTEX_CLASS_ID, 0))
			{
				CXon * mapNode = materialNode->Add(L"BumpMap");
				ExportBitmapTexture(bump, mapNode);
			}
			else
			{
				UI->ReportError(L"Material %s - Bump map has incompatible properties\r\n", material->GetFullName().data());
			}
		}
	}

	void CVwmMaterial::ExportReflectionMap(Mtl * material, CXon * materialNode)
	{
		Texmap * refl = material->GetSubTexmap(ID_RL);
		if(refl != null) 
		{
			if(refl->ClassID() == Class_ID(ACUBIC_CLASS_ID, 0))
			{
				CXon * mapNode = materialNode->Add(L"ReflectionMap");
				ExportReflectionTexture(refl, mapNode);
			}
			else
			{
				UI->ReportError(L"Material %s - Reflection map has incompatible properties\r\n", material->GetFullName().data());
			}
		}
	}

	void CVwmMaterial::ExportSelfIlluminationMap(Mtl * material, CXon * materialNode)
	{
		Texmap * selfillum = material->GetSubTexmap(ID_SI);
		if(selfillum != null) 
		{
			if(selfillum->ClassID() == Class_ID(BMTEX_CLASS_ID, 0))
			{
				CXon * mapNode = materialNode->Add(L"SelfIlluminationMap");
				ExportBitmapTexture(selfillum, mapNode);
			}
			else
			{
				UI->ReportError(L"Material %s - Self-Illumination map has incompatible properties\r\n", material->GetFullName().data());
			}
		}
	}
	
	void CVwmMaterial::ExportBitmapTexture(Texmap * tm, CXon * mapNode)
	{
		mapNode->Add(L"Type")->Set(L"BitmapTexture");

		BitmapTex * bmt = (BitmapTex *)tm;
		StdUVGen *uvg = bmt->GetUVGen();

		CString filepath = bmt->GetMapName();
		CXon * fileNode = AddTextureFile(tm, mapNode, L"File", CPath::GetFileName(filepath), filepath);

		int tiling = uvg->GetTextureTiling();
		if(tiling & U_WRAP)
			mapNode->Add(L"UAddrMode")->Set(L"Wrap");
		else if(tiling & U_MIRROR)
				mapNode->Add(L"UAddrMode")->Set(L"Mirror");
			else
				mapNode->Add(L"UAddrMode")->Set(L"Clamp");
				
		if(tiling & V_WRAP)
			mapNode->Add(L"VAddrMode")->Set(L"Wrap");
		else if(tiling & V_MIRROR)
				mapNode->Add(L"VAddrMode")->Set(L"Mirror");
			else
				mapNode->Add(L"VAddrMode")->Set(L"Clamp");

		Matrix3 m3;
		uvg->GetUVTransform(m3);
		CMaxExportHelper::ExportMatrix3(m3, mapNode->Add(L"UVTransformationMatrix"));
	}

	void CVwmMaterial::ExportReflectionTexture(Texmap * tm, CXon * mapNode)
	{
		mapNode->Add(L"Type")->Set(L"ReflectionTexture");

		IParamBlock2	*	pb	= tm->GetParamBlock(0);
		ParamBlockDesc2 *	pbd = pb->GetDesc();

		int source = pb->GetInt(pbd->IndextoID(pbd->NameToIndex(L"source")), Time);
		
		if(source == 1)
		{
			int id = pbd->IndextoID(pbd->NameToIndex(L"bitmapName"));
			CString up = pb->GetStr(id, Time, 0);
			CString dn = pb->GetStr(id, Time, 1);
			CString lf = pb->GetStr(id, Time, 2);
			CString rt = pb->GetStr(id, Time, 3);
			CString fr = pb->GetStr(id, Time, 4);
			CString bk = pb->GetStr(id, Time, 5);
			
			AddTextureFile(tm, mapNode, L"FileUP", CPath::GetFileName(up), up);
			AddTextureFile(tm, mapNode, L"FileDN", CPath::GetFileName(dn), dn);
			AddTextureFile(tm, mapNode, L"FileLF", CPath::GetFileName(lf), lf);
			AddTextureFile(tm, mapNode, L"FileRT", CPath::GetFileName(rt), rt);
			AddTextureFile(tm, mapNode, L"FileFR", CPath::GetFileName(fr), fr);
			AddTextureFile(tm, mapNode, L"FileBK", CPath::GetFileName(bk), bk);

			AddTextureFile(tm, mapNode, L"FileNoise", L"NoiseVolume.dds", CPath::ReplaceFileName(Source->MaxInterface->GetCurFilePath().data(), L"NoiseVolume.dds"));
		}
	}
	
	CXon * CVwmMaterial::AddTextureFile(Texmap * tm, CXon * mapNode, const CString & nodeName, const CString & dstPath, const CString & srcPath)
	{
		if(CPath::IsFile(srcPath) || WritePool->IsRelocated(srcPath))
		{
			WritePool->Add(dstPath, srcPath, UI);
		}
		else
		{
			UI->ReportError(L"File not found: %s\r\n", srcPath.c_str());
			return null;
		}
		
		auto n = mapNode->Add(nodeName);
		n->Set(dstPath);
		return n;
	}
}