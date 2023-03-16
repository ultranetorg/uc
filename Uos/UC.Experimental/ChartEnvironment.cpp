#include "StdAfx.h"
#include "ChartEnvironment.h"

using namespace uc;

CChartEnvironment::CChartEnvironment(CExperimentalLevel * l, const CString & name) : CEnvironmentWindow(l->World, l->Server, l->Style, name), Sizer(l->World)
{
	Level = l;

	Sizer.SetTarget(ChartElement);
}

CChartEnvironment::~CChartEnvironment()
{
	ChartElement->Free();

	OnDependencyDestroying(Entity);
}

void CChartEnvironment::SetEntity(CUol & e)
{
	Entity = Server->FindObject(e);
	Entity->Destroying += ThisHandler(OnDependencyDestroying);

	if(e.GetObjectClass() == CTradeHistory::GetClassName())
	{
		ChartElement = new CChartElement(Level, Level->Style);
		ChartElement->As<CChartElement>()->SetEntity(Entity->As<CTradeHistory>());
	}

	if(e.GetObjectClass() == CTradingview::GetClassName())
	{
		ChartElement = new CTradingviewElement(Level, Level->Style);
		ChartElement->As<CTradingviewElement>()->SetEntity(Entity->As<CTradingview>());
	}

	ChartElement->Active->Index = 0;
	ChartElement->Visual->SetMaterial(Level->World->Materials->GetMaterial(L"0 0 0"));
	ChartElement->Express(L"P", [this]{ return CFloat6(5.f); });
	ChartElement->Express(L"M", [this]{ return CFloat6(5.f); });
	ChartElement->Express(L"B", [this]{ return CFloat6(2.f); });
	ChartElement->Express(L"W", [this]{ return Slimits.Smax.W * 0.5f; });
	ChartElement->Express(L"H", [this]{ return Slimits.Smax.H * 0.8f; });

	SetContent(ChartElement);

	
	Entity->Retitled += ThisHandler(OnTitleChanged);
	OnTitleChanged(Entity);
}

void CChartEnvironment::OnDependencyDestroying(CInterObject * o)
{
	if(Entity && Entity == o)
	{
		Entity->Retitled -= ThisHandler(OnTitleChanged);
		Entity->Destroying -= ThisHandler(OnDependencyDestroying);
		Entity.Clear();
	}
}

void CChartEnvironment::OnTitleChanged(CWorldEntity * e)
{
	///SetTitle(Entity->Title);
}

void CChartEnvironment::SaveInstance()
{
	__super::SaveInstance();

	CTonDocument d;

	d.Add(L"Entity")->Set(Entity->Url);
	d.Add(L"ContentArea")->Set(ChartElement->Size);

	SaveGlobal(d, GetClassName() + L".xon");
}

void CChartEnvironment::LoadInstance()
{
	__super::LoadInstance();

	CTonDocument d; LoadGlobal(d, GetClassName() + L".xon");
		
	SetEntity(d.One(L"Entity")->Get<CUol>());
	auto a = d.One(L"ContentArea")->Get<CSize>();

	ChartElement->Express(L"W", [a]{ return a.W; });
	ChartElement->Express(L"H", [a]{ return a.H; });
}
