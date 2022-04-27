#pragma once
#include "VwmMesh.h"

namespace uos
{
	class CMeshFactory
	{
		public:
			CVwmMesh *									GetVwmMesh(INode * node);
			CArray<CVwmMesh *>							GetVwmMeshes(INode * node, MultiMtl* multi);

			bool										AskIsValidTriObj(INode * node);
//			bool										AskIsCamera(INode * node);
			void										Clear();
			void										Export(IDirectory * w, int start, int len);


			CMeshFactory(CSource * s, CProcessDlg * pd, CWritePool * wp);
			~CMeshFactory();

		private:
			int											IdCounter;
			CSource *									Source;
			CArray<CVwmMesh *>							Meshes;
			CWritePool *								WritePool;
			CProcessDlg	*								UI;
			
			std::set<TriObject *>						TriObjs;
	
			TriObject *									GetTriObjFromNode(INode *node);
	};
}
