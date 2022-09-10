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

static CExperimentalServer * Server = null;

CServer * CreateUosServer(CNexus * l, CServerInstance * info)
{
	Server = new CExperimentalServer(l, info);
	return Server;
}

void DestroyUosServer(CServer *)
{
	delete Server;
}

CClient * CreateUosClient(CNexus * nexus, CClientInstance * instance)
{
	return new CExperimentalClient(nexus, instance, Server);
}

void DestroyUosClient(CClient * client)
{
	delete client;
}

CExperimentalServer::CExperimentalServer(CNexus * l, CServerInstance * si) : CPersistentServer(l, si)
{
	Server	= this;
	Nexus	= Server->Nexus;
	Core	= Nexus->Core;
	Log		= l->Core->Supervisor->CreateLog(Instance->Name);
}

CExperimentalServer::~CExperimentalServer()
{
	while(auto i = Objects.Find([](auto j){ return j->Shared;  }))
	{
		DestroyObject(i, true);
	}

	if(World)
	{
		Nexus->Disconnect(World);
	}

	delete Style;
	delete Bitfinex;
	delete Tradingview;
	delete GeoStore;

	if(Storage)
	{
		Nexus->Disconnect(Storage);
	}
}

void CExperimentalServer::EstablishConnections(bool storage, bool world)
{
	if(!Bitfinex)
		Bitfinex = new CBitfinexProvider(this);

	if(!Tradingview)
		Tradingview = new CTradingviewProvider(this);

	if(storage && !Storage)
	{
		Storage = CPersistentServer::Storage = Nexus->Connect(this, CFileSystem::InterfaceName, [&]{ Nexus->Stop(Instance); });
		GeoStore = new CGeoStore(this);
	}

	if(world && !World)
	{
		World = Nexus->Connect(this, WORLD_PROTOCOL, [&]{ Nexus->Stop(Instance); });

		Engine	= World->Engine;
		Style	= World->Style->Clone();
	}
}

void CExperimentalServer::Initialize()
{
	EstablishConnections(true, true);

	auto shell	= Nexus->Connect<CShell>(this);
	
	auto main	= shell->FindField(CUol(CWorldEntity::Scheme, shell.Client->Instance->Name, SHELL_FIELD_MAIN));
	auto work	= shell->FindField(CUol(CWorldEntity::Scheme, shell.Client->Instance->Name, SHELL_FIELD_WORK));
	auto home	= shell->FindField(CUol(CWorldEntity::Scheme, shell.Client->Instance->Name, SHELL_FIELD_HOME));

	TCHAR szPath[MAX_PATH];
	PWSTR ppath;
	
	if(main)
	{
		if(SUCCEEDED(SHGetFolderPath(NULL, CSIDL_PROFILE, NULL, 0, szPath))) 
		{
			auto c = new CCommander(this, COMMANDER_AT_HOME_1);
			c->SetRoot(CPath::Join(CFileSystem::This, CPath::Universalize(szPath)));
			Server->RegisterObject(c, true);
			c->Free();
				
			auto fia = main->Add(c->Url, AVATAR_WIDGET);
		}

		if(SUCCEEDED(SHGetKnownFolderPath(FOLDERID_Downloads, 0, null, &ppath))) 
		{
			auto c = new CCommander(this, COMMANDER_AT_HOME_2);
			c->SetRoot(CPath::Join(CFileSystem::This, CPath::Universalize(ppath)));
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
		home->Add(CUol(CWorldEntity::Scheme, Instance->Name, COMMANDER_1),	AVATAR_ICON2D);
		home->Add(CUol(CWorldEntity::Scheme, Instance->Name, BROWSER_1),	AVATAR_ICON2D);
		home->Add(CUol(CWorldEntity::Scheme, Instance->Name, EARTH_1),		AVATAR_ICON2D);
		home->Add(CUol(CWorldEntity::Scheme, WORLD_SERVER,	 GROUP_1),		AVATAR_ICON2D);
	}

	Nexus->Disconnect(shell);
}

void CExperimentalServer::Start()
{
	if(World->Initializing)
	{
		auto shell	= Nexus->Connect<CShell>(this);

		auto main	= shell->FindField(CUol(CWorldEntity::Scheme, shell.Client->Instance->Name, SHELL_FIELD_MAIN));
		//auto work	= shell->FindField(CUol(CWorldEntity::Scheme, shell.Server->Instance->Name, SHELL_FIELD_WORK));
		//auto home	= shell->FindField(CUol(CWorldEntity::Scheme, shell.Server->Instance->Name, SHELL_FIELD_HOME));

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
		
				if(auto fia = main->FindByObject(CUol(CWorldEntity::Scheme, Instance->Name, COMMANDER_AT_HOME_1)))
				{
					fiea = def->Find(fia->Url);
					fiea->SetMetrics(m);
					fiea->SetTitleMode(ECardTitleMode::Top);
					fiea->UpdateLayout(def->Slimits, true);
					//def->MoveAvatar(fiea->Avatar, CTransformation(def->IW - def->IW * 0.2f - m.FaceSize.W, def->IH * 0.2f, def->ItemZ));
				}
				
				if(auto fib = main->FindByObject(CUol(CWorldEntity::Scheme, Instance->Name, COMMANDER_AT_HOME_2)))
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
		
		auto c = World->OpenEntity(CUol(CWorldEntity::Scheme, Instance->Name, COMMANDER_1), CArea::Main, sp);
		auto b = World->OpenEntity(CUol(CWorldEntity::Scheme, Instance->Name, BROWSER_1), CArea::Main, sp);
		auto e = World->OpenEntity(CUol(CWorldEntity::Scheme, Instance->Name, EARTH_1), CArea::Main, sp);
		auto g = World->OpenEntity(CUol(CWorldEntity::Scheme, WORLD_SERVER, GROUP_1), CArea::Main, sp);

		if(World->BackArea)
		{
			World->Show(c, CArea::Background, null);
			World->Show(b, CArea::Background, null);
			World->Show(e, CArea::Background, null);
			World->Show(g, CArea::Background, null);
		}
		else
		{
			World->Hide(c, null);
			World->Hide(b, null);
			World->Hide(e, null);
			World->Hide(g, null);
		}

		sp->Free();
	
		Nexus->Disconnect(shell);
	}
}

IInterface * CExperimentalServer::Connect(CString const & prot)
{
	return this;
}

void CExperimentalServer::Disconnect(IInterface * p)
{
}

CInterObject * CExperimentalServer::CreateObject(CString const & name)
{
	EstablishConnections(true, false);

	CPersistentObject * o = null;

	auto type = CUol::GetObjectClass(name);

	if(type == CCommander	::GetClassName())	o = new CCommander(this, name); else 
	if(type == CBrowser		::GetClassName())	o = new CBrowser(this, name); else
	if(type == CTradeHistory::GetClassName())	o = new CTradeHistory(this, name); else
	if(type == CTradingview	::GetClassName())	o = new CTradingview(this, name); else
	if(type == CEarth		::GetClassName())	o = new CEarth(this, name); else
	if(type == CEmailAccount::GetClassName())	o = new CEmailAccount(this, name);  else
	if(type == CEmail		::GetClassName())	o = new CEmail(this, name);  

	return o;
}

CInterObject * CExperimentalServer::GetEntity(CUol & e)
{
	EstablishConnections(true, false);

	return Server->FindObject(e);
}

CList<CUol> CExperimentalServer::GenerateSupportedAvatars(CUol & e, CString const & type)
{
	CList<CUol> l;

	auto p = CMap<CString, CString>{{L"entity", e.ToString()}, {L"type", type}};

	if(e.GetObjectClass() == CCommander::GetClassName())
	{
		if(type == AVATAR_ICON2D)		l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CCommanderIcon::GetClassName()), p)); else
		if(type == AVATAR_WIDGET)		l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CCommanderWidget::GetClassName()), p)); else
		if(type == AVATAR_ENVIRONMENT)	l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CCommanderEnvironment::GetClassName()), p));
	}

	if(e.GetObjectClass() == CBrowser::GetClassName())
	{
		if(type == AVATAR_ICON2D)		l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CBrowserIcon::GetClassName()), p)); else 
		if(type == AVATAR_WIDGET)		l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CBrowserWidget::GetClassName()), p)); else
		if(type == AVATAR_ENVIRONMENT)	l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CBrowserEnvironment::GetClassName()), p));
	}

	if(e.GetObjectClass() == CTradeHistory::GetClassName())
	{
		if(type == AVATAR_ICON2D)		l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CChartIcon::GetClassName()), p)); else
		if(type == AVATAR_WIDGET)		l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CChartWidget::GetClassName()), p)); 
		//if(type == AVATAR_ENVIRONMENT)	l.push_back(CUol(Url, CGuid::Generate(CChartEnvironment::GetClassName())));
	}

	if(e.GetObjectClass() == CTradingview::GetClassName())
	{
		if(type == AVATAR_ICON2D)		l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CChartIcon::GetClassName()), p)); else
		if(type == AVATAR_WIDGET)		l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CTradingviewWidget::GetClassName()), p));
		//if(type == AVATAR_ENVIRONMENT)	l.push_back(CUol(Url, CGuid::Generate(CTradingviewEnvironment::GetClassName())));
	}

	if(e.GetObjectClass() == CEarth::GetClassName())
	{
		if(type == AVATAR_ICON2D)		l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CEarthIcon::GetClassName()), p));
		if(type == AVATAR_WIDGET)		l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CEarthWidget::GetClassName()), p));
		if(type == AVATAR_ENVIRONMENT)	l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CEarthEnvironment::GetClassName()), p));
	}

	if(e.GetObjectClass() == CEmailAccount::GetClassName())
	{
		if(type == AVATAR_ENVIRONMENT)	l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CEmailAccountEnvironment::GetClassName()), p));
	}
	
	if(e.GetObjectClass() == CEmail::GetClassName())
	{
		if(type == AVATAR_WIDGET)	l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CEmailWidget::GetClassName()), p));
	}

	return l;
}

CAvatar * CExperimentalServer::CreateAvatar(CUol & avatar)
{
	EstablishConnections(true, true);

	CAvatar * a = null;
	
	if(avatar.Scheme == CAvatar::Scheme && avatar.Server == Instance->Name)
	{
		if(avatar.GetObjectClass() == CCommanderIcon::GetClassName())			a = new CCommanderIcon(this, avatar.Object); else
		if(avatar.GetObjectClass() == CCommanderEnvironment::GetClassName())	a = new CCommanderEnvironment(this, avatar.Object); else 
		if(avatar.GetObjectClass() == CCommanderWidget::GetClassName())			a = new CCommanderWidget(this, avatar.Object); else 

		if(avatar.GetObjectClass() == CBrowserIcon::GetClassName())				a = new CBrowserIcon(this, avatar.Object); else 
		if(avatar.GetObjectClass() == CBrowserWidget::GetClassName())			a = new CBrowserWidget(this, avatar.Object); else 
		if(avatar.GetObjectClass() == CBrowserEnvironment::GetClassName())		a = new CBrowserEnvironment(this, avatar.Object); else 

		if(avatar.GetObjectClass() == CChartIcon::GetClassName())				a = new CChartIcon(this, avatar.Object); else 
		if(avatar.GetObjectClass() == CChartWidget::GetClassName())				a = new CChartWidget(this, avatar.Object); else 
		if(avatar.GetObjectClass() == CTradingviewWidget::GetClassName())		a = new CTradingviewWidget(this, avatar.Object); else 

		if(avatar.GetObjectClass() == CEarthIcon::GetClassName())				a = new CEarthIcon(this, avatar.Object); else
		if(avatar.GetObjectClass() == CEarthWidget::GetClassName())				a = new CEarthWidget(this, GeoStore, avatar.Object); else
		if(avatar.GetObjectClass() == CEarthEnvironment::GetClassName())		a = new CEarthEnvironment(this, GeoStore, avatar.Object); 

		if(avatar.GetObjectClass() == CEmailAccountEnvironment::GetClassName())	a = new CEmailAccountEnvironment(this, avatar.Object); else
		if(avatar.GetObjectClass() == CEmailWidget::GetClassName())				a = new CEmailWidget(this, avatar.Object); 
	}
	
	if(a)
	{
		a->Url = avatar;
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
													c->SetRoot(Storage->NativeToUniversal(list.front()));
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

	auto root = new CMenuItem(GetTitle());
	
	auto a = Instance->Name + L"{" + IExecutor::CreateDirective + L" ";

	root->Items.AddNew(new CMenuItem(L"Commander",	[this, a](auto args)
													{
														Core->Execute(a + L"class=" + CCommander::GetClassName() + L"}", sh_new<CShowParameters>(args, Style));
													}));

	root->Items.AddNew(new CMenuItem(L"Browser",	[this, a](auto args)
													{
														Core->Execute(a + L"class=" + CBrowser::GetClassName() + L"}", sh_new<CShowParameters>(args, Style)); 
													}));

	root->Items.AddNew(new CMenuItem(L"Earth",		[this, a](auto args)
													{
														Core->Execute(a + L"class=" + CEarth::GetClassName() + L"}", sh_new<CShowParameters>(args, Style)); 
													}));

	actions.AddNew(root);

	return actions;
}

void CExperimentalServer::Execute(CXon * command, CExecutionParameters * ep)
{
	EstablishConnections(true, true);

	auto f = command->Nodes.First();

	if(f->Name == CCore::OpenDirective && command->Any(CCore::UrlArgument))
	{
		CUrl u(command->Get<CString>(CCore::UrlArgument));

		if(u.Scheme == CWorldEntity::Scheme)
		{
			World->OpenEntity(CUol(u), CArea::LastInteractive, dynamic_cast<CShowParameters *>(ep));
		}
	}
	else if(f->Name == IExecutor::CreateDirective)
	{
		if(command->Get<CString>(L"class") == CTradeHistory::GetClassName())
		{
			auto c = new CTradeHistory(this);
			Server->RegisterObject(c, true);
			c->Free();
				
			World->OpenEntity(c->Url, CArea::Main, dynamic_cast<CShowParameters *>(ep));
		}

		if(command->Get<CString>(L"class") == CCommander::GetClassName())
		{
			auto c = new CCommander(this);
			c->SetRoot(L"/");
			Server->RegisterObject(c, true);
			c->Free();

			World->OpenEntity(c->Url, CArea::Main, dynamic_cast<CShowParameters *>(ep));
		}

		if(command->Get<CString>(L"class") == CBrowser::GetClassName())
		{
			auto c = new CBrowser(this);
			c->SetAddress(CUrl(UO_WEB_HOME));
			Server->RegisterObject(c, true);
			c->Free();

			World->OpenEntity(c->Url, CArea::Main, dynamic_cast<CShowParameters *>(ep));
		}

		if(command->Get<CString>(L"class") == CEarth::GetClassName())
		{
			auto d = new CEarth(this);
			Server->RegisterObject(d, true);
			d->Free();

			World->OpenEntity(d->Url, CArea::Main, dynamic_cast<CShowParameters *>(ep));
		}
	}
	else
		Log->ReportError(this, L"Wrong command");
}

CElement * CExperimentalServer::CreateElement(CString const & name, CString const & type)
{
	if(type == CBrowser::GetClassName())		return new CCefElement(this, Style, name); else

	return null;
}
