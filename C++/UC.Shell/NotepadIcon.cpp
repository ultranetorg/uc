#include "StdAfx.h"
#include "NotepadIcon.h"

using namespace uc;

CNotepadIcon::CNotepadIcon(CShellLevel * l, CString const & name) : CIcon(l->World, l->Server, name)
{
	Level = l;
}
	
CNotepadIcon::~CNotepadIcon()
{
}

void CNotepadIcon::SetEntity(CUol & e)
{
	__super::SetEntity(e);
	
	Level->ImageExtractor->GetIconMaterial(this, (CUrl)Level->Storage->ToUol(Entity->File), 48)->Done = [this](auto m){ SetContentFromMaterial(m); };
}

