#include "StdAfx.h"
#include "LocalFileStream.h"

using namespace uc;


CLocalFileStream::CLocalFileStream(const CString & path, EFileMode mode)
{
/*
	if(mode == EFileMode::Open && !CPath::IsExists(path))
	{
		throw CException(HERE, L"File not found: %s", path.c_str());
	}
*/
	Path = path;
	Mode = mode;
	
	std::ios::openmode m=0;
	switch(mode)
	{
		case EFileMode::Open :
			m = std::ios::in | std::ios::binary;
			break;
		case EFileMode::New :
			m = std::ios::out | std::ios::binary;
			break;
	}
	if(m != 0)
	{
		Open(m);
	}
}

CLocalFileStream::~CLocalFileStream()
{
	if(Stream.is_open())
	{
		Stream.close();
	}
}

void CLocalFileStream::Open(std::ios::openmode mode)
{
	auto p = CNativePath::IsUNCServer(Path) ? Path : (L"\\\\?\\" + Path);
	Stream.open(p.c_str(), mode);

	if(Stream.fail())
	{
		throw CFileException(HERE, CString::Format(L"File opening failed: %s", Path));
	}
}

bool CLocalFileStream::IsValid()
{
	return !Stream.fail();
}

int64_t CLocalFileStream::GetSize()
{
	auto p = Stream.tellg();

	Stream.seekg(0, std::ios::end);
	auto size = Stream.tellg();
	Stream.seekg(p, std::ios::beg);
	return size;
}

int64_t CLocalFileStream::Read(void * p, int64_t size)
{
	Stream.read((char *)p, size);
	return (int)Stream.gcount();
}

int64_t CLocalFileStream::Write(const void * p, int64_t size)
{
	if(size > 0)
	{
		if(Mode == EFileMode::NewIfNeeded)
		{
			Open(std::ios::out|std::ios::binary);
		}

		Stream.write((const char *)p, size);

		return size;
	}
	else
		return 0;
}

int64_t CLocalFileStream::GetPosition()
{
	return Stream.tellg();
}

void CLocalFileStream::ReadSeek(int64_t n)
{
	Stream.seekg(n, std::ios::cur);
}

void CLocalFileStream::WriteSeek(int64_t n)
{
	Stream.seekp(n, std::ios::cur);
}

bool uc::CLocalFileStream::IsEnd()
{
	return Stream.eof();
}

