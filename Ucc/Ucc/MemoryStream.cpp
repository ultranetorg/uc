#include "StdAfx.h"
#include "MemoryStream.h"

using namespace uc;

CMemoryStream::CMemoryStream()
{
}

CMemoryStream::~CMemoryStream()
{
}

bool CMemoryStream::IsValid()
{
	return true;
}

int64_t CMemoryStream::GetSize()
{
	return Buffer.size();
}

int64_t CMemoryStream::Read(void * p, int64_t size)
{
	if(ReadPosition + size > (int64_t)Buffer.size())
	{
		throw CException(HERE, L"Length exceeded");
	}

	CopyMemory(p, Buffer.data() + ReadPosition, size);
	//
	// meStream.read((char *)p, size);
	//return Stream.gcount();

	ReadPosition += size;

	return size;
}

int64_t CMemoryStream::Write(const void * p, int64_t size)
{
	if(size > 0)
	{
		Buffer.insert(Buffer.end(), (const byte *)p, (const byte *)p + size);
		WritePosition += size;
	}

	return size;
}

int64_t CMemoryStream::GetPosition()
{
	return ReadPosition;
}

void CMemoryStream::ReadSeek(int64_t p)
{
	ReadPosition = p;
}

void CMemoryStream::WriteSeek(int64_t p)
{
	WritePosition = p;
}

bool CMemoryStream::IsEnd()
{
	return ReadPosition == Buffer.size() - 1;
}

void CMemoryStream::Clear()
{
	ReadPosition = 0;
	WritePosition = 0;

	Buffer.clear();
}

void * CMemoryStream::GetBuffer()
{
	return Buffer.data();
}

