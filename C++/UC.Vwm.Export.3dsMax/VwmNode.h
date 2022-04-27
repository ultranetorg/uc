#pragma once
#include "VwmMesh.h"
#include "MaterialFactory.h"
#include "MeshFactory.h"
#include "ProcessDlg.h"
#include "MaxExportHelper.h"
#include "VwmCamera.h"
#include "VwmLight.h"

namespace uos
{
	class CVwmNode;
	typedef	std::set<CVwmNode *>	CVwmNodeSet;

	class CVwmNode : public IType
	{
		public:
			void										Export(CXon * graph, IDirectory * w);
			void										AddChild(CVwmNode * c);
			void										MakeVisual(CMeshFactory * meshf, CMaterialFactory * mtlf);
			void										MakeCamera();
			void										MakeLight();

			Matrix3										Matrix;
			CTransformation								Transformation;
	
			UOS_RTTI
			CVwmNode(CVwmNode * parent, INode * node, CProcessDlg * ui, CSource * source);
			~CVwmNode();

		private:
			CVwmMesh *									VwmMesh = null;
			CVwmMaterial *								VwmMaterial = null;
			CVwmCamera *								VwmCamera = null;
			CVwmLight *									VwmLight = null;

			CVwmNodeSet									Children;
			CSource *									Source;
			CProcessDlg *								UI;
			INode *										MaxNode;
			CString										Name;
			bool										IsBackCull;
			bool										IsReceiveLight;
			CXonDocument *								Props = null;

			CVwmNode(CVwmNode * parent, CVwmMesh * mesh, Mtl * mtl, CMeshFactory * meshf, CMaterialFactory * mtlf, CProcessDlg * ui, CSource * source);
	};
}
