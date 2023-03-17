#pragma once
#include "VwmMaterial.h"

namespace uos
{
	typedef	CMap<Mtl *, CVwmMaterial *>		CMaxVwmMaterialMap;

	class CMaterialFactory
	{
		public:
			CVwmMaterial *								GetVwmMaterial(Mtl * mtl);
			void										Clear();
			void										Export(IDirectory * w, int start, int len);

			CMaterialFactory(CSource * s, CProcessDlg * pd, CWritePool * wp);
			~CMaterialFactory();
			
		private:
			CSource *									Source;
			CMaxVwmMaterialMap							Materials;
			CWritePool *								WritePool;
			CProcessDlg	*								ProcessDlg;
	};
}
