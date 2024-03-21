#include "stdafx.h"
#include "MaxExportHelper.h"

namespace uos
{
	void CMaxExportHelper::ExportMatrix3(Matrix3 & m, CXon * node)
	{
		CMatrix o = CMatrix::Identity;

		for(int r=0; r<4; r++)
		{
			for(int c=0; c<3; c++)
			{
				o.m[r][c] = m.GetRow(r)[c];
			}
		}
		node->Set(o);
	}
}