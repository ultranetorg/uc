#pragma once
#include "ExperimentalLevel.h"

namespace uc
{
	class CTrade : public CWorldEntity
	{
		public:
			CEvent<>										Retrieved;
			CEvent<>										Changed;
			CString										Symbol;
			CString										Interval;
			CString										Style;
			CTradeProvider *							MarketProvider;

			UOS_RTTI
			CTrade(CExperimentalLevel * l, CString const & name) : CWorldEntity(l->Server, name)
			{
				SetDefaultInteractiveMaster(AREA_MAIN);
			}

			void SaveInstance()
			{
				CTonDocument d;

				d.Add(L"Symbol")->Set(Symbol);
				d.Add(L"Interval")->Set(Interval);
				d.Add(L"Style")->Set(Style);

				SaveGlobal(d, GetClassName() + L".xon");
			}

			void LoadInstance()
			{
				CTonDocument d; LoadGlobal(d, GetClassName() + L".xon");

				SetSymbol(d.Get<CString>(L"Symbol"));
				SetInterval(d.Get<CString>(L"Interval"));
				SetStyle(d.Get<CString>(L"Style"));
			}

			void SetSymbol(CString const & name)
			{
				Symbol = name;
				Changed();
				UpdateTitle();
			}

			void SetInterval(CString const & name)
			{
				Interval = name;
				Changed();
				UpdateTitle();
			}

			void SetStyle(CString const & name)
			{
				Style = name;
				Changed();
			}

			void UpdateTitle()
			{
				CString i = MarketProvider->Intervals.Contains(Interval) ? MarketProvider->Intervals(Interval) : Interval;
				CString s = MarketProvider->FindSymbolName(Symbol);

				SetTitle(MarketProvider->Name + L" - " + s + L" - " + i);
			}
	};
}
