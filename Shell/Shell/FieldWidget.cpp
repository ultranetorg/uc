#include "StdAfx.h"
#include "FieldWidget.h"

using namespace uc;

CFieldWidget::CFieldWidget(CShellLevel * l, const CString & name) : CFieldAvatar(l, name)
{
	ShowGrid(false);
}

