#include "StdAfx.h"
#include "BrowserIcon.h"

using namespace uc;

CBrowserIcon::CBrowserIcon(CExperimentalLevel * l, CString const & name) : CIcon(l->World, l->Server, name)
{
	Level = l;
	UseRectangle();
}

CBrowserIcon::~CBrowserIcon()
{
}

void CBrowserIcon::UpdateLayout(CLimits const & l, bool apply)
{
	__super::UpdateLayout(l, apply);

	if(apply/* && Layout->Face*/)
	{
		CString n;

		auto w = Size.W;

		if(w <= 24)				n = L"Browser/Browser-24x24.png";	else 
		if(24 < w && w <= 48)	n = L"Browser/Browser-48x48.png";	else 
		if(48 < w && w <= 64)	n = L"Browser/Browser-64x64.png";	else
								n = L"Browser/Browser.png";

		if(n != CurrentFile)
		{
			SetContentFromImage(Server->MapReleasePath(n));
			CurrentFile = n;
		}
	}
}
