#include "StdAfx.h"
#include "WorldView.h"

using namespace uc;

CWorldView::CWorldView(CWorldLevel * l2, const CString & name)
{
	Level = l2;
	Name = name;
}

CWorldView::~CWorldView()
{
	for(auto i : Cameras)
	{
		delete i;
	}
}

CCamera * CWorldView::AddCamera(CViewport * vp, float fov, float znear, float zfar)
{
	CCamera * c = new CCamera(Level->Engine->Level, vp, L"Camera", fov, znear, zfar);
	Cameras.push_back(c);

	if(Cameras.size() == 1)
	{
		PrimaryCamera = c;
	}

	return c;
}

CCamera * CWorldView::AddCamera(CViewport * vp, float znear, float zfar)
{
	CCamera * c = new CCamera(Level->Engine->Level, vp, L"Camera", znear, zfar);
	Cameras.push_back(c);

	if(Cameras.size() == 1)
	{
		PrimaryCamera = c;
	}

	return c;
}

CCamera * CWorldView::GetCamera(CViewport * s)
{
	return Cameras.Find([s](auto i){ return i->Viewport == s; });
}

//CArray<CViewport *> CWorldView::GetViewports()
//{
//	CArray<CViewport *> a;
//	for(auto i : Cameras)
//	{
//		a.push_back(i->Viewport);
//	}
//	return a;
//}

CString & CWorldView::GetName()
{
	return Name;
}
