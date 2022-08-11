#pragma once
#include "StorableObject.h"
#include "File.h"

namespace uc
{
	struct CStorageEntry
	{
		CString		Path;
		CString		Type;
		CString		NameOverride;

		CStorageEntry(){}
		CStorageEntry(CString const & p) : Path(p){}
		CStorageEntry(CString const & p, CString const & t) : Path(p), Type(t) {} 
	};

	struct CFSRegexItem
	{
		CString						Path;
		CString						Type;
		CArray<CString>				Matches;

		CFSRegexItem(){}
		CFSRegexItem(CString const & p) : Path(p){}
	};


	class IDirectory
	{
		public:
			virtual CList<CStorageEntry> 				Enumerate(CString const & mask) = 0;
			virtual CList<CFSRegexItem>					EnumerateByRegex(CString const & pattern)=0;
			virtual CStream *							OpenWriteStream(CString const & name) = 0;
			virtual CStream * 							OpenReadStream(CString const & name) = 0;
			virtual void								Close(CStream * s) = 0;
			virtual void								Delete()=0;

			virtual ~IDirectory(){}
	};

	class UOS_LINKING CDirectory : public CInterObject, public IDirectory
	{
		public:
			UOS_RTTI
			CDirectory(CServer * s, CString const & name) : CInterObject(s, name) {}
	};

	class UOS_LINKING CLocalDirectory : public CDirectory
	{
		public:
			CString										Path;

			UOS_RTTI
			CLocalDirectory(CServer * s, CString const & path, CString const & npath);
			~CLocalDirectory();

			CList<CStorageEntry>						Enumerate(CString const & mask) override;
			CList<CFSRegexItem>							EnumerateByRegex(CString const & pattern);
			virtual CStream *							OpenWriteStream(CString const & name) override;
			virtual CStream * 							OpenReadStream(CString const & name) override;
			virtual void								Close(CStream * s) override;
			void virtual								Delete() override;
	};
}
