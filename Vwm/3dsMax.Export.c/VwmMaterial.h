#pragma once
#include "ProcessDlg.h"
#include "MaxExportHelper.h"

namespace uos
{
	class CVwmMaterial
	{
		public:
			Mtl	*							MaxMtl;
			TimeValue						Time;
			CWritePool	*					WritePool;
			IDirectory	*					Directory;
			bool							IsExported;
//			MultiMtl *						MaxParentMultiMtl;
//			CXon *						MwxNode;
			CString							FileName;
			
			CProcessDlg	*					UI;
			CSource *						Source;

			CVwmMaterial(CSource * s, CProcessDlg * pd, Mtl * mtl, int c, TimeValue t, CWritePool * afm);
			~CVwmMaterial();
			
			void							Export(IDirectory * w);
			CXon *							ExportMaterial(CXon * root, Mtl * material);
			CXon *							ExportStandardMaterial(CXon * root, Mtl * material);
			CXon *							ExportBlendMaterial(CXon * root, Mtl * material);
			CXon *							ExportShellMaterial(CXon * root, Mtl * material);

			void							ExportMaps(Mtl * m, CXon * materialNode);
			void							ExportDiffuseMap(Mtl * m, CXon * materialNode);
			void							ExportBumpMap(Mtl * m, CXon * materialNode);
			void							ExportOpacityMap(Mtl * m, CXon * materialNode);
			void							ExportReflectionMap(Mtl * m, CXon * materialNode);
			void							ExportSelfIlluminationMap(Mtl * material, CXon * materialNode);
			
			void							ExportBitmapTexture(Texmap * tm, CXon * mapNode);
			void							ExportReflectionTexture(Texmap * tm, CXon * mapNode);

			void							ReportProgress(int p);
			CXon *							AddTextureFile(Texmap * tm, CXon * mapNode, const CString & nodeName, const CString & dstPath, const CString & srcPath);
	};
}