#pragma once
#include "View.h"

namespace uc
{
	class IViewStore
	{
		public:
			//virtual CString							Add(CMesh * t)=0;
			virtual CView *								Get(const CString & name)=0;

			virtual ~IViewStore(){}
	};
}



