#pragma once
#include "Tradingview.h"
#include "CefElement.h"

namespace uc
{
	class CTradingviewElement : public CCefElement
	{
		public:
			CExperimentalLevel *	Level;
			CTradingview *			Entity;

			CTradingviewElement(CExperimentalLevel * l, CStyle * s) : CCefElement(l, s, GetClassName())
			{
				Level = l;
			}

			~CTradingviewElement()
			{
			}

			void SetEntity(CTradingview * e)
			{
				Entity = e;
								
				Cef->Loaded +=	[this]()
								{
									ExecuteJavascript(L"document.body.style.overflow = 'hidden'");
								};
				Refresh();
			}

			void Refresh()
			{
				auto id = CGuid::Generate64();

				auto tv =	L"<!DOCTYPE html><html style='height: 100%'><head></head><body style='height: 100%; margin: 0px;'>"
							L"<div class=\"tradingview-widget-container\" style='height: 100%'>"
							L"	<div id=\"tradingview_" + id +  L"\" style='height: 100%'></div>"
							L"	<div class=\"tradingview-widget-copyright\">TradingView</div>"
							L"	<script type=\"text/javascript\" src=\"https://s3.tradingview.com/tv.js\"></script>"
							L"	<script type=\"text/javascript\">"
							L"		new TradingView.widget("
							L"		{"
							L"			\"autosize\": true,"
							L"			\"symbol\": \"" + Entity->Symbol + "\","
							L"			\"interval\": \"" + Entity->Interval + "\","
							L"			\"timezone\": \"Etc/UTC\","
							L"			\"theme\": \"Dark\","
							L"			\"style\": \"" + Entity->Style +  "\","
							L"			\"locale\": \"en\","
							L"			\"toolbar_bg\": \"#f1f3f6\","
							L"			\"enable_publishing\": false,"
							L"			\"hide_top_toolbar\": true,"
							L"			\"allow_symbol_change\": false,"
							L"			\"container_id\": \"tradingview_" + id +  L"\","
							L"			\"padding\": 0,"
							L"			\"hidevolume\": \"true\""
							L"		}"
							L"		);"
							L"	</script>"
							L"</div>"
							L"<script type=\"text/javascript\"> document.body.style.overflow = 'hidden' </script>"
							L"</body></html>";

				///auto tv = L"<!-- TradingView Widget BEGIN -->"
				///			"<div class=\"tradingview-widget-container\">"
				///			"  <div id=\"tradingview_4251c\"></div>"
				///			"  <div class=\"tradingview-widget-copyright\"><a href=\"https://www.tradingview.com/symbols/NASDAQ-AAPL/\" rel=\"noopener\" target=\"_blank\"><span class=\"blue-text\">AAPL Chart</span></a> by TradingView</div>"
				///			"  <script type=\"text/javascript\" src=\"https://s3.tradingview.com/tv.js\"></script>"
				///			"  <script type=\"text/javascript\">"
				///			"  new TradingView.widget("
				///			"  {"
				///			"  \"width\": 980,"
				///			"  \"height\": 610,"
				///			"  \"symbol\": \"NASDAQ:AAPL\","
				///			"  \"interval\": \"D\","
				///			"  \"timezone\": \"Etc/UTC\","
				///			"  \"theme\": \"light\","
				///			"  \"style\": \"1\","
				///			"  \"locale\": \"en\","
				///			"  \"toolbar_bg\": \"#f1f3f6\","
				///			"  \"enable_publishing\": false,"
				///			"  \"allow_symbol_change\": true,"
				///			"  \"container_id\": \"tradingview_4251c\""
				///			"}"
				///			"  );"
				///			"  </script>"
				///			"</div>"
				///			"<!-- TradingView Widget END -->;";
				///
				
				Navigate(CString(L"data:text/html,") + tv);
				///Navigate(L"https://ultranet.org/tv.htm");
				///Navigate(L"https://www.tradingview.com/chart/?symbol=" +  Entity->Symbol + L"&interval=" + Entity->Interval + L"&style=" + Entity->Style +  L"&theme=dark&hide_top_toolbar=true&hidevolume=true");
			}

			void AddMenus(IMenuSection * section)
			{
				auto s = new CRectangleSectionMenuItem(Level->World, Level->Style, L"Symbol");
				section->AddItem(s);
				s->Free();


				for(auto & i : Level->Tradingview->Markets)
				{
					auto m = new CRectangleSectionMenuItem(Level->World, Level->Style, i.Name);
					s->Section->AddItem(m);
					m->Free();

					for(auto & j : i.Symbols)
					{
						m->Section->AddItem(j.second)->Clicked	=	[this, j](auto, auto mi)
																	{ 
																		Entity->SetSymbol(j.first); 
																		Refresh();
																	};
					}
				}
				
				s = new CRectangleSectionMenuItem(Level->World, Level->Style, L"Interval");
				section->AddItem(s);
				s->Free();

				for(auto i : Level->Tradingview->Intervals)
				{
					s->Section->AddItem(i.second)->Clicked	=	[this, i](auto, auto mi)
																{ 
																	Entity->SetInterval(i.first); 
																	Refresh();
																};
				}

				s = new CRectangleSectionMenuItem(Level->World, Level->Style, L"Style");
				section->AddItem(s);
				s->Free();

				for(auto i : Level->Tradingview->Styles)
				{
					s->Section->AddItem(i.second)->Clicked	=	[this, i](auto, auto mi)
																{ 
																	Entity->SetStyle(i.first); 
																	Refresh();
																};
				}
			}
	};
}
