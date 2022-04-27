#pragma once
#include "TradeHistory.h"

using namespace std;

namespace uc
{
	class CChartElement : public CRectangle
	{
		public:
			CExperimentalLevel *						Level;
			CTradeHistory *								Chart = null;
			CFont *										Font;

			CChartElement(CExperimentalLevel * l, CStyle * s) : CRectangle(l->World, GetClassName())
			{
				Level = l;

				Font = l->Style->GetFont(L"Text/Font");

				UseCanvas(Level->Engine->PipelineFactory->DiffuseTextureShader);
			}

			~CChartElement()
			{
			}

			void SetEntity(CTradeHistory * e)
			{
				Chart = e;
				Chart->Retrieved += ThisHandler(Draw);

				if(Chart && !Chart->Values.empty())
				{
					Draw();
				}
			}

			void Draw() override
			{
				if(Chart->Values.empty() || IH < 1 || IW < 1)
				{
					return;
				}

				auto c = Texture->BeginDraw(Level->Engine->ScreenEngine->Scaling);
				{
					c->Clear(CFloat4(0, 0, 0, 0));
														   					 				  
					auto white	= c->CreateSolidBrush(CFloat4(0.9f, 0.9f, 0.9f, 1));
					auto gray	= c->CreateSolidBrush(CFloat4(0.5f, 0.5f, 0.5f, 1));

					auto xtickh = 9.f;
					auto xtexth = Font->Height;
					auto ytickw = 9.f;
					auto ytextw = 35.f;
									
					CRect chart(0, xtexth + xtickh, c->W - ytextw - ytickw, c->H - xtexth - xtickh);
					CRect xruler(0, 0, chart.W, xtexth + xtickh);
					CRect yruler(chart.GetRight(), chart.GetBottom(), ytextw, chart.H);

					c->DrawLine(chart.GetLB(), chart.GetRB(), 1, gray);
					c->DrawLine(chart.GetRB(), chart.GetRT(), 1, gray);

					DrawXRuler(c, chart, xruler, gray, Font);
					DrawYRuler(c, chart, yruler, gray, Font, white);
					DrawGraph(c, chart, white);
					
					gray->Free();
					white->Free();
				}
				Texture->EndDraw();
			}

			void DrawGraph(CCanvas * c, CRect & chart, CSolidColorBrush * brush)
			{
				CFloat2 a, b;

				auto max = Chart->Max;
				auto min = Chart->Min;

				for(int i = 0; i < Chart->Values.Count() - 1; i++)
				{
					a.x = chart.W	* i / (Chart->Values.Count()); 
					a.y = chart.H	* (Chart->Values[i] - min)/(max - min);

					b.x = chart.W	* (i+1) / (Chart->Values.Count()); 
					b.y = chart.H	* (Chart->Values[i+1] - min)/(max - min);

					a = a + chart.GetLB();
					b = b + chart.GetLB();

					c->DrawLine(a, b, 1, brush);
				}
			}

			void DrawXRuler(CCanvas * c, CRect & chart, CRect & xruler, CSolidColorBrush * brush, CFont * font)
			{
				time_t begin = Chart->Begin/1000;
				tm tbegin;
				gmtime_s(&tbegin, &begin);

				time_t end = Chart->End/1000;
				tm tend;
				gmtime_s(&tend, &end);

				auto xscale = chart.W / (end - begin);

				auto mins	= (Chart->End - Chart->Begin)/(1000 * 60);

				auto pph	= chart.W/(mins/60);
				auto ppd	= 24 * pph;
				auto ppm	= 30.5 * ppd;
				auto ppy	= 12 * ppm;

				auto off = -begin % 3600;

				CString text;

				int y, m, d, h;
				y = m = d = h =0;

				if(20 < ppy && ppy < chart.W)
				{	
					y = 2; m = 1;
				}
				else if(20 < ppm && ppm < chart.W)
				{	
					m = 2; d = 1;
				}
				else if(20 < ppd && ppd < chart.W)
				{	
					d = 2; h = 1;
				}
				else
					h = 2;

				if(!(20 < ppm && ppm < chart.W))
					m = 0;
				if(!(20 < ppd && ppd < chart.W))
					d = 0;
				if(!(20 < pph && pph < chart.W))
					h = 0;

				for(auto i = off + begin; i<=end; i+=3600)
				{
					if(i < begin)
						continue;

					std::tm t;
					gmtime_s(&t, &i);

					CFloat2 a(0, xruler.GetTop());
					CFloat2 b(0, xruler.GetTop());

					a.x = b.x = xscale * (i - begin);

					int o = 0;

					if(y && t.tm_hour == 0 && t.tm_min == 0 && t.tm_sec == 0 && t.tm_mday == 1 && t.tm_mon == 0) // year
					{
						o = y;
						text = CInt32::ToString(1900 + t.tm_year);
					}
					else if(m && t.tm_mday == 1 && t.tm_hour == 0 && t.tm_min == 0 && t.tm_sec == 0) // month
					{
						o = m;

						if(40 < ppm)
						{
							wchar_t buffer[32];
							wcsftime(buffer, sizeof(buffer), L"%b", &t);
							text = buffer;
						}
					}
					else if(d && t.tm_hour == 0 && t.tm_min == 0 && t.tm_sec == 0) // days
					{
						o = d;

						if(30 < ppd)
							text = CInt32::ToString(t.tm_mday);
					}
					else if(h && t.tm_min == 0 && t.tm_sec == 0) // hours
					{
						o = h;

						if(50 < pph)
							text.Printf(L"%d:%02d", t.tm_hour, t.tm_min);
						else if(t.tm_hour == 12)
							text = L"12:00";
					}

					if(o != 0)
					{
						b.y -= o*3;
						c->DrawLine(a, b, 1, brush);
					}

					if(!text.empty())
					{
						auto r = Font->Measure(text, FLT_MAX, FLT_MAX, false);
						c->DrawText(text, font, brush, CRect(floor(a.x - r.W/2), floor(xruler.X + ((2 - o)*3)), FLT_MAX, r.H));
						text.clear();
					}
				}
			}

			void DrawYRuler(CCanvas * c, CRect & chart, CRect & yruler, CSolidColorBrush * brush, CFont * font, CSolidColorBrush * lastbrush)
			{
				auto max = Chart->Max;
				auto min = Chart->Min;

				auto d = (max - min);
				auto scale = chart.H / d;

				float s;
				for(s=1e30f; s>=1e-30f; s/=10.f)
				{
					if(max/s > 1)
					{
						break;
					}
				}

				float step;
				for(step=1e30f; step>=1e-30f; step/=10.f)
				{
					if(d/step > 1)
					{
						break;
					}
				}

				auto ppt = step * scale;

				if(ppt > 300)
					step /= 5;
				else if(ppt > 100)
					step /= 2;

				CString f = L"  %";
				auto n = log10(step);

				if(n < 0)
				{	
					f += CString::Format(L".%d", int(ceil(abs(n))));
				}
				else
					f += L".0";

				f += L"f";

				auto tickmin = min - fmod(min, s);

				CFloat2 a(yruler.X, 0);
				CFloat2 b(yruler.X + 3, 0);

				for(auto i=tickmin; i<=max; i+=step)
				{
					if(i < min)
						continue;

					a.y = b.y = yruler.Y + scale * (i - min);
					c->DrawLine(a, b, 1, brush);

					auto t = CString::Format(f, i);
					auto r = Font->Measure(t, FLT_MAX, FLT_MAX, false);
					c->DrawText(t, font, brush, CRect(floor(b.x), floor(b.y - r.H/2), FLT_MAX, r.H));
				}

				b.y = yruler.Y + scale * (Chart->Values.back() - min);

				f = L"  %";
				if(n < 0)
				{	
					f += CString::Format(L".%d", int(ceil(abs(n)) + 1));
				}
				else
					f += L".1";
				f += L"f";

				auto t = CString::Format(f, Chart->Values.back());
				auto r = Font->Measure(t, FLT_MAX, FLT_MAX, false);
				c->DrawText(t, font, lastbrush, CRect(b.x, b.y - r.H/2, FLT_MAX, r.H));
				
			}

			virtual void UpdateLayout(CLimits const & l, bool apply) override
			{
				__super::UpdateLayout(l, apply);

				if(apply && Chart && !Chart->Values.empty())
				{
					Draw();
				}
			}

			void OnMoveInput(CActive * r, CActive * s, CMouseArgs * a)
			{
			}

			void AddMenus(IMenuSection * section)
			{
				auto s = new CRectangleSectionMenuItem(Level->World, Level->Style, L"Symbols");
				section->AddItem(s);
				s->Free();

				for(auto & i : Chart->MarketProvider->Markets)
				{
					auto m = new CRectangleSectionMenuItem(Level->World, Level->Style, i.Name);
					s->Section->AddItem(m);
					m->Free();

					for(auto & j : i.Symbols)
					{
						m->Section->AddItem(j.second)->Clicked	=	[this, j](auto, auto mi)
																	{ 
																		//VChart->Mesh->Clear();
																		Chart->SetSymbol(j.first); 
																		Chart->Refresh();
																	};
					}
				}

				s = new CRectangleSectionMenuItem(Level->World, Level->Style, L"Intervals");
				section->AddItem(s);
				s->Free();

				for(auto i : Chart->MarketProvider->Intervals)
				{
					s->Section->AddItem(i.second)->Clicked	=	[this, i](auto, auto mi)
																{ 
																	//VChart->Mesh->Clear();
																	Chart->SetInterval(i.first); 
																	Chart->Refresh();
																};
				}
			}
	};
}