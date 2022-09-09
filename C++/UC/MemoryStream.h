#pragma once
#include "Stream.h"
#include "Exception.h"
#include "Array.h"

namespace uc
{
	class UOS_LINKING CMemoryStream : public CStream
	{
		public:
			CMemoryStream();
			~CMemoryStream();

			using CStream::Write;
			using CStream::Read;

			bool				IsValid();
			int64_t				Read(void * p, int64_t size) override;
			int64_t				Write(const void * p, int64_t size) override;
			int64_t				GetSize();
			int64_t				GetPosition();
			void				ReadSeek(int64_t n);
			void				WriteSeek(int64_t n);
			virtual bool		IsEnd() override;
			void				Clear();

			void *				GetBuffer();

		private:
			CArray<byte>		Buffer;
			int64_t				ReadPosition = 0;
			int64_t				WritePosition = 0;
	};
}
