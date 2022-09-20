#pragma once
#include "IType.h"
#include "ReleaseAddress.h"
#include "XonDocument.h"

namespace uc
{
	class CManifest : virtual public IType
	{
		public:
			CReleaseAddress			Address;
			CString					Channel;
			CVersion				PreviousVersion;

			long long				CompleteSize;
			CArray<byte>			CompleteHash;
			CArray<CManifest *>		CompleteDependencies;

			long long				IncrementalSize;
			CArray<byte>			IncrementalHash;
			CVersion				IncrementalMinimalVersion;
			CArray<CManifest *>		IncrementalDependencies;

			unsigned char			Hash[32];

			UOS_RTTI
			CManifest(CReleaseAddress release, CTonDocument & d);
			~CManifest();
	};
}
