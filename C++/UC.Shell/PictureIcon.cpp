#include "StdAfx.h"
#include "PictureIcon.h"

using namespace uc;

CPictureIcon::CPictureIcon(CShellLevel * l, CString const & name) : CIcon(l->World, l->Server, name)
{
	Level = l;
}
	
CPictureIcon::~CPictureIcon()
{
}

void CPictureIcon::SetEntity(CUol & e)
{
	__super::SetEntity(e);
	
	Level->ImageExtractor->GetIconMaterial(this, (CUrl)Level->Storage->ToUol(CFile::GetClassName(), Entity->File), 48)->Done = [this](auto m){ SetContentFromMaterial(m); };
}
