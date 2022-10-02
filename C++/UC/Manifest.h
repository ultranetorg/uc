#pragma once
#include "IType.h"
#include "ReleaseAddress.h"
#include "XonDocument.h"

namespace uc
{
	class CCompiledManifest : virtual public IType
	{
		public:
			CReleaseAddress					Address;
			CString							Channel;
			CArray<CCompiledManifest *>		Dependencies;

			UOS_RTTI
			CCompiledManifest(CReleaseAddress release, CTonDocument & d);
			~CCompiledManifest();
	};
}
