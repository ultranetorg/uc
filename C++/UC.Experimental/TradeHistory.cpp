#include "stdafx.h"
#include "TradeHistory.h"

using namespace uc;
using namespace std;

CTradeHistory::CTradeHistory(CExperimentalLevel * l, CString const & name) : CTrade(l, name)
{
	Level = l;
	Provider = l->Bitfinex;

	SetDirectories(MapRelative(L""));

	Level->Core->AddWorker(this);
}

CTradeHistory::~CTradeHistory()
{
	Save();

	if(Request)
	{
		delete Request;
	}

	Level->Core->RemoveWorker(this);
}

void CTradeHistory::OnServerDestructing(CPersistentObject * o)
{
	Level->Server->DestroyObject(this, true);
}

void CTradeHistory::LoadInstance()
{
	__super::LoadInstance();

	Refresh();
}

void CTradeHistory::Refresh()
{
	lock_guard<mutex> guard(Lock);

	if(Request)
	{
		delete Request;
	}

	Request = new CHttpRequest(Level->Core, L"https://api.bitfinex.com/v2/candles/trade:" + Interval + L":" + Symbol + L"/hist?limit=2000");

	LastCheck = CDateTime::Min;
}

void CTradeHistory::DoIdle()
{
	lock_guard<mutex> guard(Lock);

	if(Request)
	{
		auto now = CDateTime::UtcNow();
	
		if(now - LastCheck  > 60)
		{
			LastCheck  = now;
	
			Request->Recieved = [this]()
								{
									Values.clear();

									auto b = Request->Stream.Read();
						
									auto t = CAnsiString((const char *)b.GetData(), (int)b.GetSize());

									try
									{
										auto j = nlohmann::json::parse(t);

										if(!j.empty())
										{
											Begin = j.back()[0].get<int64_t>();
											End = j.front()[0].get<int64_t>();

											for(auto i = j.rbegin(); i != j.rend(); i++)
											{
												Values.push_back((*i)[3].get<float>());
											}

											Max = Values.Max();
											Min = Values.Min();

											Level->Log->ReportMessage(this, L"Bitfinex: received   %s   %lld bytes", Symbol, b.GetSize());

											Retrieved();
										}
									}
									catch(std::exception &)
									{
										Level->Log->ReportError(this, L"Bitfinex: json output parse failed");
										return;
									}
					
								};
			Request->Send();
		}
	}
}
