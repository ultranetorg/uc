#pragma once
#include "TradeProvider.h"

namespace uc
{
	class CBitfinexProvider : public CTradeProvider, public virtual IType
	{
		public:
			CExperimentalLevel *	Level;
			CHttpRequest *			Request;
			
			UOS_RTTI
			CBitfinexProvider(CExperimentalLevel * l)
			{
				Name = L"Bitfinex";

				Intervals[L"1m"]	= L"1 min";
				Intervals[L"5m"]	= L"5 min";
				Intervals[L"15m"]	= L"15 min";
				Intervals[L"30m"]	= L"30 min";
				Intervals[L"1h"]	= L"1 hour";
				Intervals[L"3h"]	= L"3 hours";
				Intervals[L"6h"]	= L"6 hours";
				Intervals[L"12h"]	= L"12 hours";
				Intervals[L"1D"]	= L"1 days";
				Intervals[L"7D"]	= L"7 days";
				Intervals[L"14D"]	= L"14 days";
				Intervals[L"1M"]	= L"1 month";

				Level = l;
				Request = new CHttpRequest(Level->Core, L"https://api.bitfinex.com/v2/tickers?symbols=ALL"); 
				Request->Recieved =	[this]()
									{
										Markets.clear();

										auto b = Request->Stream.Read();
										auto t = CAnsiString((const char *)b.GetData(), (int)b.GetSize());
										
										nlohmann::json j;

										try
										{
											j = nlohmann::json::parse(t);
										}
										catch(nlohmann::detail::parse_error &)
										{
											Level->Log->ReportError(this, L"Json parse error: %s", Request->Url);
											return;
										}

										if(j.find("error") == j.end())
										{
											CMarket & m = CMarket();
										
											for(auto i : j)
											{
												CString s;
												
												s = CString::FromAnsi(i[0].get<std::string>());
	
												CString n = s.substr(1, 3);
	
												if(s[0] == 't')
												{
												
													if(Markets.Has([&n](auto & i){ return i.Name == n; }))
														m = Markets.Find([&n](auto & i){ return i.Name == n; });
													else
													{	
														Markets.push_back(CMarket{n});
														m = Markets.back();
													}
																					   
													Markets.back().Symbols[s] = s.substr(1); // skip 't'
												}
											}
	
											Markets.Sort([](auto & a, auto & b){ return a.Name < b.Name; });
										}
									};
				Request->Send();
			}
			~CBitfinexProvider()
			{
				delete Request;
			}
	};
}
