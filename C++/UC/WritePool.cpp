#include "StdAfx.h"
#include "WritePool.h"
#include "LocalFileStream.h"

using namespace uc;

CWritePool::CWritePool(CString const & d)
{
	Directory = d;
}

CWritePool::~CWritePool()
{
}

void CWritePool::RelocateSource(const CString & path)
{
	for(auto & i : RelocatedFiles)
	{
		if(i.Path == path)
		{
			return;
		}
	}

	CRelocatedFile f;
	f.Path = path;
	f.Name = CNativePath::GetFileName(path);
	RelocatedFiles.push_back(f);
}

void CWritePool::Add(const CString & dst, const CString & src, IWriterProgress * p)
{
	CString name = CNativePath::GetFileName(src);
	
	CString srcReal = src;

	for(auto & i : RelocatedFiles)
	{
		if(i.Name == name)
		{
			srcReal = i.Path;
			break;
		}
	}

	for(auto & i : Files)
	{
		if(i.DestinationPath == dst && i.SourcePath == srcReal && i.Added)
		{
			return;
		}
	}
	
	CPoolFile pf;
	pf.Added			= true;
	pf.DestinationPath	= dst;
	pf.SourcePath		= srcReal;
	
	CLocalFileStream s(CNativePath::Join(Directory, dst), EFileMode::New);
	s.Write(&CLocalFileStream(srcReal, EFileMode::Open));
	
	Files.push_back(pf);
}

bool CWritePool::IsRelocated(const CString & src)
{
	CString name = CNativePath::GetFileName(src);
	
	for(auto & i : RelocatedFiles)
	{
		if(i.Name == name)
		{
			return true;
		}
	}
	return false;
}
