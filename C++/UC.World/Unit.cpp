#include "stdafx.h"
#include "Unit.h"
#include "WorldServer.h"

using namespace uc;

CUnit::CUnit(CWorldServer * l, CView * hview, CString const & name) : CArea(l, name)
{
	Level = l;
	HeaderView = hview;
}

CUnit::~CUnit()
{
}

CSpaceBinding<CVisualSpace> & CUnit::AllocateVisualSpace(CViewport * vp)
{
	auto & s = __super::AllocateVisualSpace(vp);

	if(Header && Header->Parent)
	{
		auto hs = Header->AllocateVisualSpace(vp);
		s.Space->AddFront(hs.Space);
	}

	return s;
}

CSpaceBinding<CActiveSpace> & CUnit::AllocateActiveSpace(CViewport * vp)
{
	auto & s = __super::AllocateActiveSpace(vp);

	if(Header && Header->Parent)
	{
		auto hs = Header->AllocateActiveSpace(vp);
		s.Space->AddFront(hs.Space);
	}

	return s;
}

void CUnit::DeallocateVisualSpace(CVisualSpace * s)
{
	if(Header && Header->Parent)
	{
		auto hs = Header->VisualSpaces.Find([s](auto & i){ return i.Space->Parent == s; });
		s->Remove(hs.Space);
		Header->DeallocateVisualSpace(hs.Space);
	}

	__super::DeallocateVisualSpace(s);
}

void CUnit::DeallocateActiveSpace(CActiveSpace * s)
{
	if(Header && Header->Parent)
	{
		auto hs = Header->ActiveSpaces.Find([s](auto & i){ return i.Space->Parent == s; });
		s->Remove(hs.Space);
		Header->DeallocateActiveSpace(hs.Space);
	}

	__super::DeallocateActiveSpace(s);
}

void CUnit::Save(CXon * x)
{
	__super::Save(x);

	x->Add(L"LastInteractiveMaster")->Set(LastInteractiveMaster);
	x->Add(L"LastNoninteractiveMaster")->Set(LastNoninteractiveMaster);
	x->Add(L"LastMaster")->Set(LastMaster);
}

void CUnit::Load(CXon * x)
{
	__super::Load(x);

	LastInteractiveMaster		= x->Get<CString>(L"LastInteractiveMaster");
	LastNoninteractiveMaster	= x->Get<CString>(L"LastNoninteractiveMaster");
	LastMaster					= x->Get<CString>(L"LastMaster");
}
