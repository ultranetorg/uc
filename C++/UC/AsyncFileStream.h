#pragma once
#include "Core.h"

namespace uc
{
	class CAsyncFileStream : public CFileStream
	{
		public:
			CCore *		Core;
			CThread *	Thread;
			CBuffer		Buffer;

			CAsyncFileStream(CCore * l, const CString & filepath, EFileMode mode) : CFileStream(filepath, mode)
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

