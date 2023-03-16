#pragma once
#include "Array.h"
#include "Exception.h"
#include "ZipStream.h"
#include "NativePath.h"
#include "FileSystemEntry.h"
#include "Path.h"

namespace uc
{
	class UOS_LINKING CZipDirectory : public IDirectory
	{
		public:
			virtual CList<CFileSystemEntry>		Enumerate(CString const & regex) override;
			virtual CStream *					WriteFile(CString const & path) override;
			virtual CStream * 					ReadFile(CString const & path) override;
			virtual void						Close(CStream * s) override;
			virtual void						Delete(CString const & path) override;

			void								Add(const CString & dst, CStream * src);
			void								Add(const CString & dst, CStream * src, IWriterProgress * p);
			bool								Contains(const CString & filepath);

			int									GetSize(const CString & entrypath);
			CString								GetUri(const CString & entrypath);
			
			CString								GetUri();
			EFileMode							GetMode();
			void *								GetHandle();

			CZipDirectory(const CString & zippath, EFileMode mode);
			CZipDirectory(CStream * s, EFileMode mode);
			~CZipDirectory();


		private:
			CString								ZipPath;	
			void *								ZFile;
			EFileMode							Mode;
			CBuffer								Buffer;

	};


	class UOS_LINKING CZip
	{
		public:
			static std::vector<uint8_t>			Compress(void *in_data, size_t in_data_size);
			static CArray<char>					Decompress(void *in_data, size_t in_data_size);
	};
}
