#include "StdAfx.h"
#include "LinkProperties.h"

using namespace uc;

CLinkProperties::CLinkProperties(CShellLevel * l, const CString & name) : CEnvironmentWindow(l->World, l->Server, l->Style, name), Sizer(l->World)
{
/*
	Level = l;
	Level->Server->Destroying += ThisHandler(OnDependencyDestroying);

	//Active->EnableListening(true);

	Content = new CRectangle(l->World);
	Content->Visual->SetMaterial(Level->World->Materials->GetMaterial(L"0 0.1 0.1"));
	Content->Express(L"P", []{ return CFloat6(20.f); });
	Content->Express(L"M", []{ return CFloat6(0.f); });
	Content->Express(L"B", []{ return CFloat6(2.f); });
	Content->Express(L"W", []{ return 600.f; });
	Content->Express(L"H", []{ return 200.f; });
	SetContent(Content);

	Table = new CGrid(l->World, l->Style);
	Table->Spacing = CFloat2(10);
	Content->AddNode(Table);

	TitleText = new CText(l->World, l->Style);
	TitleText->SetText(L"Title");
	TitleText->SetWrap(true);
	TitleEdit = new CTextEdit(l->World, l->Style, false);
	TitleEdit->Express(L"P", []{ return CFloat6(3.f); });
	TitleEdit->Express(L"B", []{ return CFloat6(1.f); });
	TitleEdit->Express(L"W", [this]{ return TitleEdit->Limits.Smax.W * 0.5f; });
	Table->SetCell(0, 0, TitleText);
	Table->SetCell(0, 1, TitleEdit);

	TargetText = new CText(l->World, l->Style);
	TargetText->SetText(L"Target");
	TargetText->SetWrap(true);
	TargetEdit = new CTextEdit(l->World, l->Style, false);
	TargetEdit->Express(L"P", []{ return CFloat6(3.f); });
	TargetEdit->Express(L"B", []{ return CFloat6(1.f); });
	TargetEdit->Express(L"W", [this]{ return TargetEdit->Limits.Smax.W; });
	Table->SetCell(1, 0, TargetText);
	Table->SetCell(1, 1, TargetEdit);

	ExecutorText = new CText(l->World, l->Style);
	ExecutorText->SetText(L"Open with");
	ExecutorText->SetWrap(true);
	ExecutorEdit = new CTextEdit(l->World, l->Style, false);
	ExecutorEdit->Express(L"P", []{ return CFloat6(3.f); });
	ExecutorEdit->Express(L"B", []{ return CFloat6(1.f); });
	ExecutorEdit->Express(L"W", [this]{ return ExecutorEdit->Limits.Smax.W; });
	Table->SetCell(2, 0, ExecutorText);
	Table->SetCell(2, 1, ExecutorEdit);

	Table->Free();
	TitleText->Free();
	TitleEdit->Free();
	TargetText->Free();
	TargetEdit->Free();
	ExecutorText->Free();
	ExecutorEdit->Free();
	
	OkButton = new CButton(l->World, l->Style);
	OkButton->SetText(L"OK");
	OkButton->Express(L"W", [this](){ return max(OkButton->CW, 100); });
	//OkButton->SetExpression(L"EElementProp::H", [this](auto n){ OkButton->H = 24; });

	OkButton->Pressed += ThisHandler(OnOK);
	Content->AddNode(OkButton);
	OkButton->Free();

	Sizer.SetNode(Content);
	Sizer.Min = CSize(400, 200, 0);*/
}

CLinkProperties::~CLinkProperties()
{
/*
	OnDependencyDestroying(Entity);
	OnDependencyDestroying(Level->Server);

	Table->RemoveNode(TitleText);
	Table->RemoveNode(TitleEdit);
	Table->RemoveNode(TargetText);
	Table->RemoveNode(TargetEdit);
	Table->RemoveNode(ExecutorText);
	Table->RemoveNode(ExecutorEdit);

	Content->Free();*/
}

void CLinkProperties::SetEntity(CUol & e)
{
	Entity = Server->FindObject(e);
	Entity->Destroying += ThisHandler(OnDependencyDestroying);

	///Entity->SetTitle(L"Link Properties - " + Entity->Title);

	TitleEdit->SetText(Entity->Title);
	TargetEdit->SetText(Entity->Target.ToString());
	ExecutorEdit->SetText(Entity->Executor);
}

void CLinkProperties::SaveInstance()
{
	CTonDocument d;

	d.Add(L"ContentArea")->Set(Content->Size);

	SaveGlobal(d, GetClassName() + L".xon");
}

void CLinkProperties::OnDependencyDestroying(CInterObject * o)
{
	if(Entity && Entity == o)
	{
		Entity->Destroying -= ThisHandler(OnDependencyDestroying);
		Entity.Clear();
	}
}

void CLinkProperties::LoadInstance()
{
	CTonDocument d; LoadGlobal(d, GetClassName() + L".xon");

///	Sizer.SetSize(d.Get<CSize>(L"ContentArea"));
}

void CLinkProperties::OnOK(CButton * b)
{
	if(Entity)
	{
		Entity->SetTitle(TitleEdit->GetText());
		Entity->SetTarget(CUrl(TargetEdit->GetText()));
		Entity->Executor = ExecutorEdit->GetText();
	}
	
	Level->World->Hide(Unit, null);
}


void CLinkProperties::UpdateLayout(CLimits const & l, bool apply)
{
	__super::UpdateLayout(l, apply);

	OkButton->Transform(OkButton->Slimits.Smax.W - OkButton->W, 0, Z_STEP); 
}

void CLinkProperties::DetermineSize(CSize & smax, CSize & s)
{
	throw CException(HERE, L"Not implemented");
}

CTransformation CLinkProperties::DetermineTransformation(CPositioning * ps, CPick & pk, CTransformation & t)
{
	throw CException(HERE, L"Not implemented");
}
