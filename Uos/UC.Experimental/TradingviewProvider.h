#pragma once
#include "TradeProvider.h"

namespace uc
{
	class CTradingviewProvider : public CTradeProvider, public virtual IType
	{
		public:
			CExperimentalLevel *	Level;
			CList<CHttpRequest *>	Requests;
			
			UOS_RTTI
			CTradingviewProvider(CExperimentalLevel * l)
			{
				Level = l;
				Name = L"TradingView";
							   			
				Markets.clear();
				FetchSymbols(L"bitcoin", L"Crypto");
				FetchSymbols(L"forex", L"Forex");
				FetchSymbols(L"stock", L"Stock");
				FetchSymbols(L"cfd", L"CFD");
				FetchSymbols(L"index", L"Index");

				Styles[L"0"] = L"Bars";
				Styles[L"1"] = L"Candles";
				Styles[L"2"] = L"Line";
				Styles[L"3"] = L"Area";
				Styles[L"4"] = L"Renko";
				Styles[L"5"] = L"Kagi";
				Styles[L"6"] = L"Point and Figure";
				Styles[L"7"] = L"Line Break";
				Styles[L"8"] = L"Heikin Ashi";
				Styles[L"9"] = L"Hollow Candles";

				Intervals[L"1"]		= L"1 min";
				Intervals[L"3"]		= L"3 min";
				Intervals[L"5"]		= L"5 min";
				Intervals[L"15"]	= L"15 min";
				Intervals[L"30"]	= L"30 min";
				Intervals[L"60"]	= L"1 hour";
				Intervals[L"120"]	= L"2 hours";
				Intervals[L"180"]	= L"3 hours";
				Intervals[L"240"]	= L"4 hours";
				Intervals[L"D"]		= L"1 day";
				Intervals[L"W"]		= L"1 week";
			}

			void FetchSymbols(CString const & type, CString const & name)
			{
				Markets.push_back(CMarket{name});
				auto m = &Markets.back();

				auto r = new CHttpRequest(Level->Core, L"https://symbol-search.tradingview.com/symbol_search/?text=&exchange=&type=" + type + L"&hl=true&lang=en&domain=production");
				r->Headers  = {	L"Host: symbol-search.tradingview.com",
								L"Origin: https://www.tradingview.com",
								L"Referer: https://www.tradingview.com/"};

				r->Recieved =	[this, r, m]()
								{
									auto b = r->Stream.Read();
									auto t = CAnsiString((const char *)b.GetData(), (int)b.GetSize());
							
									nlohmann::json j;

									try
									{
										j = nlohmann::json::parse(t);
									}
									catch(nlohmann::detail::parse_error &)
									{
										Level->Log->ReportError(this, L"Json parse error: %s", r->Url);
										return;
									}

									for(auto i : j)
									{
										auto pr = i.find("prefix") != i.end() ? i["prefix"].get<std::string>() : i["exchange"].get<std::string>();
								
										auto k = pr + ":" + i["symbol"].get<std::string>();
										auto v = i["exchange"].get<std::string>() + " - " + i["symbol"].get<std::string>();
																		
										m->Symbols[CString::FromAnsi(k)] = CString::FromAnsi(v);
									}
								};
				r->Send();

				Requests.push_back(r);
			}

			~CTradingviewProvider()
			{
				for(auto i : Requests)
				{
					delete i;
				}
			}
	};
}
