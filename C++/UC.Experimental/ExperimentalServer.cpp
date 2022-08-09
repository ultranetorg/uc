#include "stdafx.h"
#include "ExperimentalServer.h"

#include "CommanderEnvironment.h"
#include "CommanderWidget.h"
#include "CommanderIcon.h"

#include "BrowserEnvironment.h"
#include "BrowserWidget.h"
#include "BrowserIcon.h"

#include "ChartEnvironment.h"
#include "ChartWidget.h"
#include "ChartIcon.h"

#include "TradingviewWidget.h"

#include "EarthIcon.h"
#include "EarthWidget.h"
#include "EarthEnvironment.h"

#include "EmailAccountEnvironment.h"
#include "EmailWidget.h"


using namespace uc;

static CExperimentalServer * This = null;

CServer * StartUosServer(CNexus * l, CServerInfo * info)
{
	This = new CExperimentalServer(l, info);
	return This;
}

void StopUosServer()
{
	delete This;
}

CExperimentalServer::CExperimentalServer(CNexus * l, CServerInfo * si) : CStorableServer(l, si)
{
	Server	= this;
	Nexus	= Server->Nexus;
	Core	= Nexus->Core;
	Log		= l->Core->Supervisor->CreateLog(Url.Server);
///	Storage		= l->Storage;
}

CExperimentalServer::~CExperimentalServer()
{
	while(auto i = Objects.Find([](auto j){ return j->Shared;  }))
	{
		DestroyObject(i);
	}

	if(World)
	{
		World.Server->Disconnecting -= ThisHandler(OnDisconnecting);
		Nexus->Disconnect(World);
	}

	delete Style;
	delete Bitfinex;
	delete Tradingview;
	delete GeoStore;

	if(Storage)
	{
		Storage.Server->Disconnecting -= ThisHandler(OnDisconnecting);
		Nexus->Disconnect(Storage);
	}
}

void CExperimentalServer::EstablishConnections()
{
	if(!World)
	{
		World = Nexus->Connect(this, WORLD_PROTOCOL);
		World.Server->Disconnecting += ThisHandler(OnDisconnecting);

		Engine	= World->Engine;
		Style	= World->Style->Clone();

		Bitfinex	= new CBitfinexProvider(this);
		Tradingview = new CTradingviewProvider(this);
		GeoStore	= new CGeoStore(this);
	}

	if(!Storage)
	{
		Storage = CStorableServer::Storage = Nexus->Connect(this, UOS_STORAGE_PROTOCOL);
		Storage.Server->Disconnecting += ThisHandler(OnDisconnecting);
	}

}

void CExperimentalServer::OnDisconnecting(CServer * s, IProtocol * p, CString & pn)
{
	if(	p == World && pn == WORLD_PROTOCOL ||
		p == Storage && pn == UOS_STORAGE_PROTOCOL)
	{
		Nexus->StopServer(this); // THE END
	}
}

IProtocol * CExperimentalServer::Connect(CString const & prot)
{
	return this;
}

void CExperimentalServer::Disconnect(IProtocol * p)
{
}

CBaseNexusObject * CExperimentalServer::CreateObject(CString const & name)
{
	EstablishConnections();

	CNexusObject * o = null;

	auto type = name.substr(0, name.find(L'-'));

	if(type == CCommander	::GetClassName())	o = new CCommander(this, name); else 
	if(type == CBrowser		::GetClassName())	o = new CBrowser(this, name); else
	if(type == CTradeHistory::GetClassName())	o = new CTradeHistory(this, name); else
	if(type == CTradingview	::GetClassName())	o = new CTradingview(this, name); else
	if(type == CEarth		::GetClassName())	o = new CEarth(this, name); else
	if(type == CEmailAccount::GetClassName())	o = new CEmailAccount(this, name);  else
	if(type == CEmail		::GetClassName())	o = new CEmail(this, name);  

	return o;
}

void CExperimentalServer::Start(EStartMode sm)
{
	EstablishConnections();

	auto shell	= Nexus->Connect<CShell>(this, SHELL_PROTOCOL);

	auto main	= shell->FindField(CUol(shell.Server->Url, SHELL_FIELD_MAIN));
	auto work	= shell->FindField(CUol(shell.Server->Url, SHELL_FIELD_WORK));
	auto home	= shell->FindField(CUol(shell.Server->Url, SHELL_FIELD_HOME));

	if(sm == EStartMode::Initialization)
	{
		TCHAR szPath[MAX_PATH];
		PWSTR ppath;
	
		if(main)
		{
			if(SUCCEEDED(SHGetFolderPath(NULL, CSIDL_PROFILE, NULL, 0, szPath))) 
			{
				auto c = new CCommander(this, COMMANDER_AT_HOME_1);
				c->SetRoot(Nexus->MapPath(UOS_MOUNT_LOCAL, CPath::Universalize(szPath)));
				Server->RegisterObject(c, true);
				c->Free();
				
				auto fia = main->Add(c->Url, AVATAR_WIDGET);
			}

			if(SUCCEEDED(SHGetKnownFolderPath(FOLDERID_Downloads, 0, null, &ppath))) 
			{
				auto c = new CCommander(this, COMMANDER_AT_HOME_2);
				c->SetRoot(Nexus->MapPath(UOS_MOUNT_LOCAL, CPath::Universalize(ppath)));
				Server->RegisterObject(c, true);
				c->Free();
		
				auto fib = main->Add(c->Url, AVATAR_WIDGET);

				CoTaskMemFree(ppath);
			}
		}

		if(work)
		{
			auto th = new CTradeHistory(this);
			th->SetSymbol(L"tBTCUSD");
			th->SetInterval(L"1h");
			th->Refresh();
			Server->RegisterObject(th, true);
			th->Free();
			work->Add(th->Url, AVATAR_WIDGET);

			auto dj = new CTradingview(this);
			dj->SetSymbol(L"US30");
			dj->SetInterval(L"W");
			dj->SetStyle(L"1");
			Server->RegisterObject(dj, true);
			dj->Free();
			work->Add(dj->Url, AVATAR_WIDGET);

			auto snp = new CTradingview(this);
			snp->SetSymbol(L"US500");
			snp->SetInterval(L"W");
			snp->SetStyle(L"1");
			Server->RegisterObject(snp, true);
			snp->Free();
			work->Add(snp->Url, AVATAR_WIDGET);

			auto nq = new CTradingview(this);
			nq->SetSymbol(L"US100");
			nq->SetInterval(L"W");
			nq->SetStyle(L"1");
			Server->RegisterObject(nq, true);
			nq->Free();
			work->Add(nq->Url, AVATAR_WIDGET);

			auto e = new CEmail(this);
			Server->RegisterObject(e, true);
			e->Free();
			work->Add(e->Url, AVATAR_WIDGET);
		}

		// COMMANDER_1
		auto c1 = CreateObject(COMMANDER_1)->As<CCommander>();
		c1->SetRoot(L"/");
		Server->RegisterObject(c1, true);
		c1->Free();

		// BROWSER_1
		auto b1 = CreateObject(BROWSER_1)->As<CBrowser>();
		b1->SetAddress(CUrl(UO_WEB_HOME));
		Server->RegisterObject(b1, true);
		b1->Free();

		// EARTH_1
		auto e1 = CreateObject(EARTH_1)->As<CEarth>();
		Server->RegisterObject(e1, true);
		e1->Free();

		// GROUP_1
		auto c = CreateObject(COMMANDER_AT_GROUP)->As<CCommander>();
		c->SetRoot(L"/");
		Server->RegisterObject(c, true);
		c->Free();

		auto b = CreateObject(BROWSER_AT_GROUP)->As<CBrowser>();
		b->SetAddress(CUrl(UO_WEB_HOME));
		Server->RegisterObject(b, true);
		b->Free();

		auto e = CreateObject(EARTH_AT_GROUP)->As<CEarth>();
		Server->RegisterObject(e, true);
		e->Free();
		
		auto g = World->CreateGroup(GROUP_1);
		g->SetTitle(L"Group #1");
		g->Entities.push_back(c);
		g->Entities.push_back(b);
		g->Entities.push_back(e);
		g->Free();

		if(home)
		{
			home->Add(CUol(Url, COMMANDER_1),			AVATAR_ICON2D);
			home->Add(CUol(Url, BROWSER_1),				AVATAR_ICON2D);
			home->Add(CUol(Url, EARTH_1),				AVATAR_ICON2D);
			home->Add(CUol(L"", WORLD_SERVER, GROUP_1), AVATAR_ICON2D);
		}
	}

	if(World->Initializing)
	{
		if(World->Complexity == AVATAR_ENVIRONMENT)
		{
			auto de	= shell->FindFieldEnvironmentByEntity(main->Url);

			if(de)
			{
				auto def = de->GetField();
		
				auto m = def->GetMetrics(AVATAR_WIDGET, ECardTitleMode::Top, CSize::Nan);
				m.FaceSize = CSize(m.FaceSize.H, m.FaceSize.H, 0);
		
				CFieldItemElement * fiea = null;
				CFieldItemElement * fieb = null;
		
				if(auto fia = main->FindByObject(CUol(Url, COMMANDER_AT_HOME_1)))
				{
					fiea = def->Find(fia->Url);
					fiea->SetMetrics(m);
					fiea->SetTitleMode(ECardTitleMode::Top);
					fiea->UpdateLayout(def->Slimits, true);
					//def->MoveAvatar(fiea->Avatar, CTransformation(def->IW - def->IW * 0.2f - m.FaceSize.W, def->IH * 0.2f, def->ItemZ));
				}
				
				if(auto fib = main->FindByObject(CUol(Url, COMMANDER_AT_HOME_2)))
				{
					fieb = def->Find(fib->Url);
					fieb->SetMetrics(m);
					fieb->SetTitleMode(ECardTitleMode::Top);
					fieb->UpdateLayout(def->Slimits, true);
					//def->MoveAvatar(fieb->Avatar, CTransformation(def->IW - def->IW * 0.2f - m.FaceSize.W, fiea->Transformation.Position.y + fiea->H, def->ItemZ));
				}
			}
		}

		auto sp = new CShowParameters();
		sp->PlaceOnBoard = true;
		
		auto c = World->OpenEntity(CUol(Url, COMMANDER_1), AREA_MAIN, sp);
		auto b = World->OpenEntity(CUol(Url, BROWSER_1), AREA_MAIN, sp);
		auto e = World->OpenEntity(CUol(Url, EARTH_1), AREA_MAIN, sp);
		auto g = World->OpenEntity(CUol(L"", WORLD_SERVER, GROUP_1), AREA_MAIN, sp);

		if(World->BackArea)
		{
			World->Show(c, AREA_BACKGROUND, null);
			World->Show(b, AREA_BACKGROUND, null);
			World->Show(e, AREA_BACKGROUND, null);
			World->Show(g, AREA_BACKGROUND, null);
		}
		else
		{
			World->Hide(c, null);
			World->Hide(b, null);
			World->Hide(e, null);
			World->Hide(g, null);
		}

		sp->Free();
	}

	Nexus->Disconnect(shell);
}

CBaseNexusObject * CExperimentalServer::GetEntity(CUol & e)
{
	return Server->FindObject(e);
}

CList<CUol> CExperimentalServer::GenerateSupportedAvatars(CUol & e, CString const & type)
{
	CList<CUol> l;

	auto p = CMap<CString, CString>{{L"entity", e.ToString()}, {L"type", type}};

	if(e.GetType() == CCommander::GetClassName())
	{
		if(type == AVATAR_ICON2D)		l.push_back(CUol(Url, CGuid::Generate64(CCommanderIcon::GetClassName()), p)); else
		if(type == AVATAR_WIDGET)		l.push_back(CUol(Url, CGuid::Generate64(CCommanderWidget::GetClassName()), p)); else
		if(type == AVATAR_ENVIRONMENT)	l.push_back(CUol(Url, CGuid::Generate64(CCommanderEnvironment::GetClassName()), p));
	}

	if(e.GetType() == CBrowser::GetClassName())
	{
		if(type == AVATAR_ICON2D)		l.push_back(CUol(Url, CGuid::Generate64(CBrowserIcon::GetClassName()), p)); else 
		if(type == AVATAR_WIDGET)		l.push_back(CUol(Url, CGuid::Generate64(CBrowserWidget::GetClassName()), p)); else
		if(type == AVATAR_ENVIRONMENT)	l.push_back(CUol(Url, CGuid::Generate64(CBrowserEnvironment::GetClassName()), p));
	}

	if(e.GetType() == CTradeHistory::GetClassName())
	{
		if(type == AVATAR_ICON2D)		l.push_back(CUol(Url, CGuid::Generate64(CChartIcon::GetClassName()), p)); else
		if(type == AVATAR_WIDGET)		l.push_back(CUol(Url, CGuid::Generate64(CChartWidget::GetClassName()), p)); 
		//if(type == AVATAR_ENVIRONMENT)	l.push_back(CUol(Url, CGuid::Generate(CChartEnvironment::GetClassName())));
	}

	if(e.GetType() == CTradingview::GetClassName())
	{
		if(type == AVATAR_ICON2D)		l.push_back(CUol(Url, CGuid::Generate64(CChartIcon::GetClassName()), p)); else
		if(type == AVATAR_WIDGET)		l.push_back(CUol(Url, CGuid::Generate64(CTradingviewWidget::GetClassName()), p));
		//if(type == AVATAR_ENVIRONMENT)	l.push_back(CUol(Url, CGuid::Generate(CTradingviewEnvironment::GetClassName())));
	}

	if(e.GetType() == CEarth::GetClassName())
	{
		if(type == AVATAR_ICON2D)		l.push_back(CUol(Url, CGuid::Generate64(CEarthIcon::GetClassName()), p));
		if(type == AVATAR_WIDGET)		l.push_back(CUol(Url, CGuid::Generate64(CEarthWidget::GetClassName()), p));
		if(type == AVATAR_ENVIRONMENT)	l.push_back(CUol(Url, CGuid::Generate64(CEarthEnvironment::GetClassName()), p));
	}

	if(e.GetType() == CEmailAccount::GetClassName())
	{
		if(type == AVATAR_ENVIRONMENT)	l.push_back(CUol(Url, CGuid::Generate64(CEmailAccountEnvironment::GetClassName()), p));
	}
	
	if(e.GetType() == CEmail::GetClassName())
	{
		if(type == AVATAR_WIDGET)	l.push_back(CUol(Url, CGuid::Generate64(CEmailWidget::GetClassName()), p));
	}

	return l;
}

CAvatar * CExperimentalServer::CreateAvatar(CUol & u)
{
	EstablishConnections();

	CAvatar * a = null;
	
	if(u.Server == Url.Server)
	{
		if(u.GetType() == CCommanderIcon::GetClassName())			a = new CCommanderIcon(this, u.Object); else
		if(u.GetType() == CCommanderEnvironment::GetClassName())	a = new CCommanderEnvironment(this, u.Object); else 
		if(u.GetType() == CCommanderWidget::GetClassName())			a = new CCommanderWidget(this, u.Object); else 

		if(u.GetType() == CBrowserIcon::GetClassName())				a = new CBrowserIcon(this, u.Object); else 
		if(u.GetType() == CBrowserWidget::GetClassName())			a = new CBrowserWidget(this, u.Object); else 
		if(u.GetType() == CBrowserEnvironment::GetClassName())		a = new CBrowserEnvironment(this, u.Object); else 

		if(u.GetType() == CChartIcon::GetClassName())				a = new CChartIcon(this, u.Object); else 
		if(u.GetType() == CChartWidget::GetClassName())				a = new CChartWidget(this, u.Object); else 
		if(u.GetType() == CTradingviewWidget::GetClassName())		a = new CTradingviewWidget(this, u.Object); else 

		if(u.GetType() == CEarthIcon::GetClassName())				a = new CEarthIcon(this, u.Object); else
		if(u.GetType() == CEarthWidget::GetClassName())				a = new CEarthWidget(this, GeoStore, u.Object); else
		if(u.GetType() == CEarthEnvironment::GetClassName())		a = new CEarthEnvironment(this, GeoStore, u.Object); 

		if(u.GetType() == CEmailAccountEnvironment::GetClassName())	a = new CEmailAccountEnvironment(this, u.Object); else
		if(u.GetType() == CEmailWidget::GetClassName())				a = new CEmailWidget(this, u.Object); 
	}
	
	if(a)
	{
		a->Url = u;
		Server->RegisterObject(a, false);
		a->Free();
	}
	
	return a;
}

void CExperimentalServer::DestroyAvatar(CAvatar * a)
{
	Server->DestroyObject(a, false);
}

IMenuSection * CExperimentalServer::CreateNewMenu(CFieldElement * fe, CFloat3 & p, IMenu * m)
{
	auto f = dynamic_cast<CFieldWorld *>(fe);
		
	auto types = m->CreateSection(L"ExperimentalActions:MenuSection");


	types->AddItem(L"Commander")->Clicked =	[this, f, p, fe](auto, auto mi)
											{
												auto list = Core->Os->OpenFileDialog(FOS_PICKFOLDERS);

												if(!list.empty())
												{
													auto c = new CCommander(this);
													c->SetRoot(Nexus->NativeToUniversal(list.front()));
													Server->RegisterObject(c, true);
													c->Free();
			
													auto fi = fe->Entity->Add(c->Url, AVATAR_WIDGET);
					
													auto fie = fe->Find(fi->Url);
					
													auto m = fe->GetMetrics(AVATAR_WIDGET, ECardTitleMode::Top, CSize::Nan);
													m.FaceSize = CSize(m.FaceSize.H * 0.75f, m.FaceSize.H, 0);
													fie->SetMetrics(m);
													fie->SetTitleMode(ECardTitleMode::Top);
								
													fe->MoveAvatar(fie->Avatar, CTransformation(p));
							
													fie->UpdateLayout();
												}
											};

	types->AddItem(L"Browser")->Clicked =	[this, f, p, fe](auto, auto mi)
											{
												auto c = new CBrowser(this);
												c->SetAddress(CUrl(UO_WEB_HOME));
												Server->RegisterObject(c, true);
												c->Free();
			
												auto fi = fe->Entity->Add(c->Url, AVATAR_WIDGET);
					
												auto fie = fe->Find(fi->Url);
					
												auto m = fe->GetMetrics(AVATAR_WIDGET, ECardTitleMode::Top, CSize::Nan);
												m.FaceSize = CSize(m.FaceSize.H * 0.75f, m.FaceSize.H, 0);
												fie->SetMetrics(m);
												fie->SetTitleMode(ECardTitleMode::Top);
								
												fe->MoveAvatar(fie->Avatar, CTransformation(p));
							
												fie->UpdateLayout();
											};

	
	types->AddItem(L"Bitfinex Chart")->Clicked =	[this, f, p, fe](auto, auto mi)
													{
														auto c = new CTradeHistory(this);
														c->SetSymbol(L"tBTCUSD");
														c->SetInterval(L"1m");
														c->Refresh();
														Server->RegisterObject(c, true);
														c->Free();
			
														auto fi = fe->Entity->Add(c->Url, AVATAR_WIDGET);
					
														auto fie = fe->Find(fi->Url);
					
														auto m = fe->GetMetrics(AVATAR_WIDGET, ECardTitleMode::Top, CSize::Nan);
														m.FaceSize = CSize(m.FaceSize.H * 0.75f, m.FaceSize.H, 0);
														fie->SetMetrics(m);
														fie->SetTitleMode(ECardTitleMode::Top);
								
														fe->MoveAvatar(fie->Avatar, CTransformation(p));
							
														fie->UpdateLayout();
													};

	types->AddItem(L"TradingView Chart")->Clicked =	[this, f, p, fe](auto, auto mi)
													{
														auto c = new CTradingview(this);
														c->SetSymbol(L"COINBASE:BTCUSD");
														c->SetInterval(L"1");
														c->SetStyle(L"1");
														//c->Refresh();
														Server->RegisterObject(c, true);
														c->Free();
			
														auto fi = fe->Entity->Add(c->Url, AVATAR_WIDGET);
					
														auto fie = fe->Find(fi->Url);
					
														auto m = fe->GetMetrics(AVATAR_WIDGET, ECardTitleMode::Top, CSize::Nan);
														m.FaceSize = CSize(m.FaceSize.H * 0.75f, m.FaceSize.H, 0);
														fie->SetMetrics(m);
														fie->SetTitleMode(ECardTitleMode::Top);
								
														fe->MoveAvatar(fie->Avatar, CTransformation(p));
							
														fie->UpdateLayout();
													};

		types->AddItem(L"Email Client")->Clicked =	[this, f, p, fe](auto, auto mi)
													{
														auto c = new CEmail(this);
														//c->SetCurrent(CUrl::Empty);
														Server->RegisterObject(c, true);
														c->Free();
														
														auto fi = fe->Entity->Add(c->Url, AVATAR_WIDGET);
														
														auto fie = fe->Find(fi->Url);
														
														auto m = fe->GetMetrics(AVATAR_WIDGET, ECardTitleMode::Top, CSize::Nan);
														//m.FaceSize = CSize(m.FaceSize.H * 0.75f, m.FaceSize.H, 0);
														fie->SetMetrics(m);
														fie->SetTitleMode(ECardTitleMode::Top);
														
														fe->MoveAvatar(fie->Avatar, CTransformation(p));
														
														fie->UpdateLayout();
													};

	return types;
}

CRefList<CMenuItem *> CExperimentalServer::CreateActions()
{
	CRefList<CMenuItem *> actions;

	auto root = new CMenuItem(L"Experimental");
	
	CUrl a = Server->Url;
	a[L"Action"] = L"Create";

	a[L"Class"] = CCommander::GetClassName();
	root->Items.AddNew(new CMenuItem(L"Commander",	[this, a](auto args)
													{
														Nexus->Execute(a, sh_new<CShowParameters>(args, Style)); 
													}));

	a[L"Class"] = CBrowser::GetClassName();
	root->Items.AddNew(new CMenuItem(L"Browser",	[this, a](auto args)
													{
														Nexus->Execute(a, sh_new<CShowParameters>(args, Style)); 
													}));

	a[L"Class"] = CEarth::GetClassName();
	root->Items.AddNew(new CMenuItem(L"Earth", [this, a](auto args){ Nexus->Execute(a, sh_new<CShowParameters>(args, Style)); }));

	actions.AddNew(root);

	return actions;
}

bool CExperimentalServer::CanExecute(const CUrq & u)
{
	return u.GetSystem() == Server->Url.Server;
}

void CExperimentalServer::Execute(const CUrq & u, CExecutionParameters * ep)
{
	EstablishConnections();

	if(CUol::IsValid(u))
	{
		World->OpenEntity(CUol(u), AREA_MAIN, dynamic_cast<CShowParameters *>(ep));
	}
	else if(u.Query.Contains(L"Action"))
	{
		if(u.Query(L"Action") == L"Create")
		{
			if(u.Query.Contains(L"Class", CTradeHistory::GetClassName()))
			{
				auto c = new CTradeHistory(this);
				Server->RegisterObject(c, true);
				c->Free();
				
				World->OpenEntity(c->Url, AREA_MAIN, dynamic_cast<CShowParameters *>(ep));
			}

			if(u.Query.Contains(L"Class", CCommander::GetClassName()))
			{
				auto c = new CCommander(this);
				c->SetRoot(L"/");
				Server->RegisterObject(c, true);
				c->Free();

				World->OpenEntity(c->Url, AREA_MAIN, dynamic_cast<CShowParameters *>(ep));
			}

			if(u.Query.Contains(L"Class", CBrowser::GetClassName()))
			{
				auto c = new CBrowser(this);
				c->SetAddress(CUrl(UO_WEB_HOME));
				Server->RegisterObject(c, true);
				c->Free();

				World->OpenEntity(c->Url, AREA_MAIN, dynamic_cast<CShowParameters *>(ep));
			}

			if(u.Query.Contains(L"Class", CEarth::GetClassName()))
			{
				auto d = new CEarth(this);
				Server->RegisterObject(d, true);
				d->Free();

				World->OpenEntity(d->Url, AREA_MAIN, dynamic_cast<CShowParameters *>(ep));
			}
		}
	}
	else
		Log->ReportError(this, L"Wrong command: %s", u.ToString());
}

CElement * CExperimentalServer::CreateElement(CString const & name, CString const & type)
{
	if(type == CBrowser::GetClassName())		return new CCefElement(this, Style, name); else

	return null;
}
