#include "StdAfx.h"
#include "MaterialFactory.h"

namespace uos
{
	CMaterialFactory::CMaterialFactory(CSource * s, CProcessDlg * pd, CWritePool * wp)
	{
		Source		= s;
		ProcessDlg	= pd;
		WritePool	= wp;
	}
	
	CMaterialFactory::~CMaterialFactory()
	{
		Clear();
	}

	CVwmMaterial * CMaterialFactory::GetVwmMaterial(Mtl * mtl)
	{
		CMaxVwmMaterialMap::iterator mi = Materials.find(mtl);
		if(mi != Materials.end())
		{
			return mi->second;
		}

		CVwmMaterial * vwmMat = new CVwmMaterial(Source, ProcessDlg, mtl, (int)Materials.size(), Source->MaxInterface->GetTime(), WritePool);
		Materials.insert(std::pair<Mtl *, CVwmMaterial *>(mtl, vwmMat));
		return vwmMat;
	}

	void CMaterialFactory::Clear()
	{
		for(auto & i : Materials)
		{
			delete i.second;
		}
		Materials.clear();
	}

	void CMaterialFactory::Export(IDirectory * w, int start, int len)
	{
		int c=1;
		for(auto & i : Materials)
		{
			i.second->Export(w);
			ProcessDlg->SetProgressOverall(start+(len*c)/(int)Materials.size());
			c++;
		}
	}
}
