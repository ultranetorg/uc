#pragma once
#include "Core.h"
#include "LocalFileStream.h"

namespace uc
{
	class CAsyncFileStream : public CLocalFileStream
	{
		public:
			CCore *		Core;
			CThread *	Thread;
			CBuffer		Buffer;

			CAsyncFileStream(CCore * l, const CString & filepath, EFileMode mode) : CLocalFileStream(filepath, mode)
			{
				Core = l;
			}

			~CAsyncFileStream()
			{
			}

			void ReadAsync(std::function<void()> ondone)
			{
				Core->RunThread(Path,	[this]()
										{
											Buffer = Read();
										},
										[ondone]()
										{ 
											ondone();
										});
			}
	};
}

