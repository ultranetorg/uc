#include "StdAfx.h"
#include "ShellServer.h"

using namespace uc;

static CShellServer * This = null;

CServer * StartUosServer(CLevel2 * l, CServerInfo * info)
{
	This = new CShellServer(l, info);
	return This;
}

void StopUosServer()
{
	delete This;
}

CShellServer::CShellServer(CLevel2 * l, CServerInfo * si) : CServer(l, si), CShellLevel(l)
{
	Server		= this;
	Log			= l->Core->Supervisor->CreateLog(SHELL_OBJECT);
	Storage		= Nexus->Storage;
}

CShellServer::~CShellServer()
{
	while(auto i = Objects.Find([](auto j){ return j->Shared;  }))
	{
		DestroyObject(i);
	}

	if(Menu)
	{
		Menu->Close();
		Menu->Free();
	}

	delete ImageExtractor;
	delete Style;
	delete WebInformer;

	if(World)
	{
		if(World->Sphere)
		{
			World->Sphere->Active->MouseEvent -= ThisHandler(OnWorldSphereMouse);
		}

		World.Server->Disconnecting -= ThisHandler(OnWorldDisconnecting);
		Nexus->Disconnect(World);
	}
}

void CShellServer::OnWorldDisconnecting(CServer * s, IProtocol * p, CString & pn)
{
	if(p == World && pn == WORLD_PROTOCOL)
	{
		Nexus->StopServer(this); // THE END
	}
}

void CShellServer::EstablishConnections()
{
	if(!World)
	{
		World = Nexus->Connect(this, WORLD_PROTOCOL);
		World.Server->Disconnecting += ThisHandler(OnWorldDisconnecting);
		
		Engine			= World->Engine;
		Style			= World->Style->Clone();
		ImageExtractor	= new CImageExtractor(World, Server);
		WebInformer		= new CWebInformer(Level, this, MapUserGlobalPath(L"")); /// finish FS changes
	}
}

IProtocol * CShellServer::Connect(CString const & p)
{
	if(p == IMAGE_EXTRACTOR_PROTOCOL)	return ImageExtractor; else
	if(p == TRAY_PROTOCOL)				return FindObject<IProtocol>(SHELL_TRAY_1); else
		return this;
}

void CShellServer::Disconnect(IProtocol * o)
{
}

CNexusObject * CShellServer::CreateObject(CString const & name)
{
	EstablishConnections();

	CNexusObject * o = null;

	auto type = CUol::GetObjectType(name);

	if(type == SHELL_HUD)							o = new CFieldServer(this, name); else 
	if(type == CField::GetClassName())				o = new CFieldServer(this, name); else
	if(type == CHistory::GetClassName())			o = new CHistory(this, name); else
	if(type == CBoard::GetClassName())				o = new CBoard(this, name); else
	if(type == CDirectoryMenu::GetClassName())		o = new CDirectoryMenu(this, name); else
	if(type == CApplicationsMenu::GetClassName())	o = new CApplicationsMenu(this, name); else
	if(type == CLink::GetClassName())				o = new CLink(this, name); else
	if(type == CNotepad::GetClassName())			o = new CNotepad(this, name); else
	if(type == CPicture::GetClassName())			o = new CPicture(this, name); else
	if(type == CTheme::GetClassName())				o = new CTheme(this, name); 
	if(type == CTray::GetClassName())				o = new CTray(this, name); 

	return o;
}

void CShellServer::Start(EStartMode sm)
{
	EstablishConnections();

	if(sm == EStartMode::Installing)
	{
		TCHAR szPath[MAX_PATH];

		Storage->CreateGlobalDirectory(this);

		// hud
		auto hud = new CFieldServer(this, SHELL_HUD_1);
		hud->SetDefaultInteractiveMaster(AREA_HUD);
		Server->RegisterObject(hud, true);
		hud->Free();

		// history 
		auto his = new CHistory(this, SHELL_HISTORY_1);
		Server->RegisterObject(his, true);
		his->Free();
		hud->Add(his->Url, AVATAR_WIDGET);

		// board
		auto board = new CBoard(this, SHELL_BOARD_1);
		Server->RegisterObject(board, true);
		board->Free();
		hud->Add(board->Url, AVATAR_WIDGET);

		// apps menu
		auto menu = new CApplicationsMenu(this, SHELL_APPLICATIONSMENU_1);
		Server->RegisterObject(menu, true);
		menu->Free();
		board->Add(menu->Url, AVATAR_WIDGET);


		// tray
		auto tr = new CTray(this, SHELL_TRAY_1);
		Server->RegisterObject(tr, true);
		tr->Free();
		hud->Add(tr->Url, AVATAR_WIDGET);
								
		//desktops
		auto create =	[this](const CString & name, const CString & title)
						{
							auto d = new CFieldServer(this, name);
							d->SetTitle(title);
							Server->RegisterObject(d, true);
							d->Free();
							
							return d;
						};
		
		auto home = create(SHELL_FIELD_MAIN,		L"Main");
		auto pics = create(SHELL_FIELD_PICTURES,	L"Pictures");
		auto work = create(SHELL_FIELD_WORK,		L"Work");

		// desktop icons
		CList<CStorageEntry> items;
	
		if(SUCCEEDED(SHGetFolderPath(NULL, CSIDL_DESKTOP, NULL, 0, szPath))) 
		{
			auto d = Storage->OpenDirectory(Storage->MapPath(UOS_MOUNT_LOCAL, CPath::Universalize(szPath)));
			for(auto i : d->Enumerate(L"*.*"))
			{
				items.push_back(i);
			}
			Storage->Close(d);
		}
	
		if(SUCCEEDED(SHGetFolderPath(NULL, CSIDL_COMMON_DESKTOPDIRECTORY, NULL, 0, szPath)))
		{
			auto d = Storage->OpenDirectory(Storage->MapPath(UOS_MOUNT_LOCAL, CPath::Universalize(szPath)));
			for(auto i : d->Enumerate(L"*.*"))
			{
				items.push_back(i);
			}
			Storage->Close(d);
		}
			
		items.RemoveIf([](auto & i){ return i.Path.EndsWith(L"/desktop.ini"); });

		CList<CUol> links;
		
		for(auto i : items)
		{
			auto link = new CLink(this);
			link->SetTarget((CUrl)Storage->ToUol(i.Type, i.Path));
			Server->RegisterObject(link, true);
			links.push_back(link->Url);
			link->Free();
		}

		home->Add(links, AVATAR_ICON2D);

		if(SUCCEEDED(SHGetFolderPath(NULL, CSIDL_MYPICTURES, NULL, 0, szPath)))
		{
			auto d = Storage->OpenDirectory(Storage->MapPath(UOS_MOUNT_LOCAL, CPath::Universalize(szPath)));
			
			int n = 0;
			for(auto i : d->Enumerate(L"*.*"))
			{
				auto e = CPath::GetExtension(i.Path);
				
				if(e == L"jpg" || e == L"png")
				{
					auto p = new CPicture(this);
					p->SetFile(i.Path);
					Server->RegisterObject(p, true);
					pics->Add(p->Url, AVATAR_WIDGET);
					p->Free();
				
					n++;

					if(n > 8)
					{
						break;
					}
				}
			}
			Storage->Close(d);
		}


		// Usage.txt notepad
		auto v = new CNotepad(this);
		v->SetFile(Storage->MapPath(UOS_MOUNT_CORE, L"About.txt"));
		Server->RegisterObject(v, true);
		home->Add(v->Url, AVATAR_WIDGET);
		v->Free();

		// Mouse.png picture	
		auto p = new CPicture(this);
		p->SetFile(MapPath(L"Mouse.png"));
		Server->RegisterObject(p, true);
		home->Add(p->Url, AVATAR_WIDGET);
		p->Free();

		// theme
		auto t = new CTheme(this, SHELL_THEME_1);
		t->SetSource(MapPath(L"Spaceland.vwm"));
		Server->RegisterObject(t, true);
		t->Free();



		auto d = new CFieldServer(this, SHELL_FIELD_HOME);
		d->SetTitle(L"Home");
		Server->RegisterObject(d, true);
		d->Free();

		d->Add(CUol(Url, SHELL_FIELD_MAIN), AVATAR_ICON2D);
		d->Add(CUol(Url, SHELL_FIELD_PICTURES), AVATAR_ICON2D);
		d->Add(CUol(Url, SHELL_FIELD_WORK), AVATAR_ICON2D);
	}

	Server->FindObject(SHELL_HISTORY_1); // force to revive

	if(World->Initializing)
	{
		if(World->Complexity == AVATAR_ENVIRONMENT)
		{
			World->OpenEntity(CUol(Url, SHELL_HUD_1), AREA_HUD, null);

			if(World->Free3D)
			{
				World->OpenEntity(CUol(Url, SHELL_BOARD_1), AREA_NEAR, null);
			}
		}

		auto f = new CShowParameters(); 
		f->PlaceOnBoard = true;
		
		auto u = World->OpenEntity(CUol(Url, SHELL_FIELD_WORK), AREA_FIELDS, f);
		
		if(World->BackArea)
			World->Show(u, AREA_BACKGROUND, null);

		u = World->OpenEntity(CUol(Url, SHELL_FIELD_PICTURES), AREA_FIELDS, f);
		
		if(World->BackArea)
			World->Show(u, AREA_BACKGROUND, null);
		
		u = World->OpenEntity(CUol(Url, SHELL_FIELD_MAIN), AREA_FIELDS, f);
		
		if(World->BackArea)
			World->Show(u, AREA_BACKGROUND, null);

		World->OpenEntity(CUol(Url, SHELL_THEME_1), AREA_THEME, null);
		
		if(World->FullScreen)
		{
			World->OpenEntity(CUol(Url, SHELL_FIELD_HOME), AREA_FIELDS, f);
		}

		f->Free();
	}

	if(World->FullScreen)
	{
		World->Components[WORLD_HISTORY]	= CUol(Url, SHELL_HISTORY_1);
		World->Components[WORLD_BOARD]		= CUol(Url, SHELL_BOARD_1);
		World->Components[WORLD_TRAY]		= CUol(Url, SHELL_TRAY_1);

		World->GlobalHotKeys[EKeyboardControl::GlobalHome] =[this](auto k)
															{ 
																World->OpenEntity(CUol(Url, SHELL_FIELD_HOME), AREA_LAST_INTERACTIVE, null);
															};
	}

	if(World->Sphere)
	{
		World->Sphere->Active->MouseEvent[EListen::Normal] += ThisHandler(OnWorldSphereMouse);
	}
}

void CShellServer::OnWorldSphereMouse(CActive *, CActive *, CMouseArgs * arg)
{
	if(arg->Control == EMouseControl::RightButton && arg->Event == EGraphEvent::Click)
	{
		if(!Menu)
		{
			Menu = new CRectangleMenu(World, Style, L"ShellGlobalMenu");
		}

		Menu->Section->Clear();

		AddSystemMenuItems(Menu->Section);

		Menu->Open(arg->Pick);

		arg->StopPropagation = true;
	}
}

CNexusObject * CShellServer::GetEntity(CUol & e)
{
	return Server->FindObject(e);
}

CList<CUol> CShellServer::GenerateSupportedAvatars(CUol & e, CString const & type)
{
	CList<CUol> l;

	auto p = CMap<CString, CString>{{L"entity", e.ToString()}, {L"type", type}};

	if(e.GetType() == SHELL_HUD)
	{
		if(type == AVATAR_ENVIRONMENT)	l.push_back(CUol(Url, CGuid::Generate64(CHudEnvironment::GetClassName()), p));
	}

	if(e.GetType() == CHistory::GetClassName())
	{
		if(type == AVATAR_WIDGET)		l.push_back(CUol(Url, CGuid::Generate64(CHistoryWidget::GetClassName()), p));
	}

	if(e.GetType() == CBoard::GetClassName())
	{
		if(type == AVATAR_WIDGET)		l.push_back(CUol(Url, CGuid::Generate64(CBoardWidget::GetClassName()), p));
		if(type == AVATAR_ENVIRONMENT)	l.push_back(CUol(Url, CGuid::Generate64(CBoardEnvironment::GetClassName()), p));
	}

	if(e.GetType() == CField::GetClassName())
	{
		if(type == AVATAR_ENVIRONMENT)	l.push_back(CUol(Url, CGuid::Generate64(CFieldEnvironment::GetClassName()), p)); else
		if(type == AVATAR_WIDGET)		l.push_back(CUol(Url, CGuid::Generate64(CFieldWidget::GetClassName()), p)); else
		if(type == AVATAR_ICON2D)		l.push_back(CUol(Url, CGuid::Generate64(CFieldIcon::GetClassName()), p));
	}

	if(e.GetType() == CDirectoryMenu::GetClassName())
	{
		if(type == AVATAR_WIDGET)		l.push_back(CUol(Url, CGuid::Generate64(CMenuWidget::GetClassName()), p));
	}

	if(e.GetType() == CApplicationsMenu::GetClassName())
	{
		if(type == AVATAR_WIDGET)		l.push_back(CUol(Url, CGuid::Generate64(CSystemMenuWidget::GetClassName()), p));
	}

	if(e.GetType() == CTheme::GetClassName())
	{
		if(type == AVATAR_WIDGET)		l.push_back(CUol(Url, CGuid::Generate64(CThemeWidget::GetClassName()), p)); else
		if(type == AVATAR_ENVIRONMENT)	l.push_back(CUol(Url, CGuid::Generate64(CThemeEnvironment::GetClassName()), p));;
	}

	if(e.GetType() == CLink::GetClassName())
	{
		if(type == AVATAR_ICON2D)		l.push_back(CUol(Url, CGuid::Generate64(CLinkIcon::GetClassName()), p));;
		if(type == AVATAR_ENVIRONMENT)	l.push_back(CUol(Url, CGuid::Generate64(CLinkProperties::GetClassName()), p));;
	}

	if(e.GetType() == CNotepad::GetClassName())
	{
		if(type == AVATAR_ICON2D)		l.push_back(CUol(Url, CGuid::Generate64(CNotepadIcon::GetClassName()), p)); else
		if(type == AVATAR_WIDGET)		l.push_back(CUol(Url, CGuid::Generate64(CNotepadWidget::GetClassName()), p));
	}

	if(e.GetType() == CPicture::GetClassName())
	{
		if(type == AVATAR_ICON2D)		l.push_back(CUol(Url, CGuid::Generate64(CPictureIcon::GetClassName()), p)); else
		if(type == AVATAR_WIDGET)		l.push_back(CUol(Url, CGuid::Generate64(CPictureWidget::GetClassName()), p));
	}

	if(e.GetType() == CTray::GetClassName())
	{
		if(type == AVATAR_WIDGET)		l.push_back(CUol(Url, CGuid::Generate64(CTrayWidget::GetClassName()), p));
	}

	return l;

}

CAvatar * CShellServer::CreateAvatar(CUol & u)
{
	EstablishConnections();

	CAvatar * a = null;
	
	if(u.Server == Url.Server)
	{
		if(u.GetType() == CHudEnvironment::GetClassName())			a = new CHudEnvironment(this, u.Object); else 

		if(u.GetType() == CHistoryWidget::GetClassName())			a = new CHistoryWidget(this, u.Object); else 

		if(u.GetType() == CBoardWidget::GetClassName())				a = new CBoardWidget(this, u.Object); else 
		if(u.GetType() == CBoardEnvironment::GetClassName())		a = new CBoardEnvironment(this, u.Object); else 

		if(u.GetType() == CFieldEnvironment::GetClassName())		a = new CFieldEnvironmentServer(this, u.Object); else 
		if(u.GetType() == CFieldWidget::GetClassName())				a = new CFieldWidget(this, u.Object); else 
		if(u.GetType() == CFieldIcon::GetClassName())				a = new CFieldIcon(this, u.Object); else 

		if(u.GetType() == CMenuWidget::GetClassName())				a = new CMenuWidget(this, u.Object); else 
		if(u.GetType() == CSystemMenuWidget::GetClassName())	a = new CSystemMenuWidget(this, u.Object); else 

		if(u.GetType() == CThemeWidget::GetClassName())				a = new CThemeWidget(this, u.Object); else
		if(u.GetType() == CThemeEnvironment::GetClassName())		a = new CThemeEnvironment(this, u.Object); else 

		if(u.GetType() == CLinkIcon::GetClassName())				a = new CLinkIcon(this, u.Object); else 
		if(u.GetType() == CLinkProperties::GetClassName())			a = new CLinkProperties(this, u.Object); else 
		
		if(u.GetType() == CNotepadIcon::GetClassName())				a = new CNotepadIcon(this, u.Object); else 
		if(u.GetType() == CNotepadWidget::GetClassName())			a = new CNotepadWidget(this, u.Object); else

		if(u.GetType() == CPictureIcon::GetClassName())				a = new CPictureIcon(this, u.Object); else 
		if(u.GetType() == CPictureWidget::GetClassName())			a = new CPictureWidget(this, u.Object); else

		if(u.GetType() == CTrayWidget::GetClassName())				a = new CTrayWidget(this, u.Object); else

		return null;
	}

	a->Url = u;
	
	Server->RegisterObject(a, false);
	a->Free();
	
	return a;
}

void CShellServer::DestroyAvatar(CAvatar * a)
{
	Server->DestroyObject(a);
}

IMenuSection * CShellServer::CreateNewMenu(CFieldElement * fe, CFloat3 & p, IMenu * m)
{
	auto types = m->CreateSection(L"ShellActions:MenuSection");

	types->AddItem(L"Link(s)")->Clicked =	[this, fe, p](auto, auto mi)
											{
												auto paths = Core->Os->OpenFileDialog(FOS_ALLOWMULTISELECT | FOS_NODEREFERENCELINKS);
												
												if(!paths.empty())
												{
													CList<CUol> links;

													for(auto i : paths)
													{
														auto link = new CLink(this);
														link->SetTarget((CUrl)Storage->ToUol(Storage->GetType(Storage->NativeToUniversal(i)), Storage->NativeToUniversal(i)));
														Server->RegisterObject(link, true);
														link->Free();
														links.push_back(link->Url);
													}

													auto fis = fe->Entity->Add(links, AVATAR_ICON2D);
	
													auto fies = fe->Find(fis.Select<CUol>([](auto i){ return i->Url; }));
					
													fe->Arrange(fies, EXAlign::Center);
													fe->TransformItems(fies, CTransformation(p/* - CFloat3(-a.W/2, -a.H/2, 0)*/));
												}
											};
	types->AddItem(L"Notepad")->Clicked =	[this, fe, p](auto, auto mi)
											{
												auto paths = Core->Os->OpenFileDialog(FOS_NODEREFERENCELINKS);
												
												if(!paths.empty())
												{
													auto v = new CNotepad(this);
													v->SetFile(Nexus->Storage->NativeToUniversal(paths.front()));
													Server->RegisterObject(v, true);
													v->Free();

													auto fi = fe->Entity->Add(v->Url, AVATAR_WIDGET);
													auto fie = fe->Find(fi->Url);
																																							
													fie->Transform(0, 0, 0);
													fe->TransformItems(CRefList<CFieldItemElement *>{fie}, CTransformation(p));
												}
											};

	types->AddItem(L"Picture")->Clicked =	[this, fe, p](auto, auto mi)
											{
												auto paths = Core->Os->OpenFileDialog(FOS_NODEREFERENCELINKS);
												
												if(!paths.empty())
												{
													auto v = new CPicture(this);
													v->SetFile(Nexus->Storage->NativeToUniversal(paths.front()));
													Server->RegisterObject(v, true);
													v->Free();

													auto fi = fe->Entity->Add(v->Url, AVATAR_WIDGET);
													auto fie = fe->Find(fi->Url);
																																							
													fie->Transform(0, 0, 0);
													fe->TransformItems(CRefList<CFieldItemElement *>{fie}, CTransformation(p));
												}
											};

	types->AddItem(L"Menu")->Clicked =	[this, fe, p](auto, auto mi)
										{
											auto list = Core->Os->OpenFileDialog(FOS_PICKFOLDERS);

											if(!list.empty())
											{
												auto o = new CDirectoryMenu(this);
												o->AddPath(Nexus->Storage->NativeToUniversal(list.front()));
												Server->RegisterObject(o, true);
												o->Free();
												
												//auto o = Nexus->Storage->LocalPathToObject(paths.front());
												auto fi = fe->Entity->Add(o->Url, AVATAR_WIDGET);
												auto fie = fe->Find(fi->Url);

												fie->Transform(0, 0, 0);
												fe->TransformItems(CRefList<CFieldItemElement *>{fie}, CTransformation(p));
											}
										};

	types->AddItem(L"System Menu")->Clicked =	[this, fe, p](auto, auto mi)
												{
													auto o = new CApplicationsMenu(this);
													Server->RegisterObject(o, true);
													o->Free();

												
													auto fi = fe->Entity->Add(o->Url, AVATAR_WIDGET);
													auto fie = fe->Find(fi->Url);

													fie->Transform(0, 0, 0);
													fe->TransformItems(CRefList<CFieldItemElement *>{fie}, CTransformation(p));
												};

	return types;
}

CRefList<CMenuItem *> CShellServer::CreateActions()
{
	CRefList<CMenuItem *> actions;

	auto shell = new CMenuItem(L"Shell");
	
	CUrl a = Server->Url;
	a[L"Action"] = L"Create";

	a[L"Class"] = CField::GetClassName();
	shell->Items.AddNew(new CMenuItem(L"Field", [this, a](auto args)
												{
													Nexus->Execute(a, sh_new<CShowParameters>(args, Style)); 
												}));

	a[L"Class"] = CTheme::GetClassName();
	shell->Items.AddNew(new CMenuItem(L"Theme", [this, a](auto args)
												{
													Nexus->Execute(a, sh_new<CShowParameters>(args, Style)); 
												}));


	auto win = new CMenuItem(L"Windows");

	TCHAR			szPath[MAX_PATH];
	CList<CString>	sources;

	if(SUCCEEDED(SHGetFolderPath(NULL, CSIDL_COMMON_PROGRAMS, NULL, 0, szPath)))
	{
		sources.push_back(Nexus->Storage->MapPath(UOS_MOUNT_LOCAL, CPath::Universalize(szPath)));
	}

	if(SUCCEEDED(SHGetFolderPath(NULL, CSIDL_PROGRAMS, NULL, 0, szPath)))
	{
		sources.push_back(Nexus->Storage->MapPath(UOS_MOUNT_LOCAL, CPath::Universalize(szPath)));
	}

	win->Opening =	[this, win, sources]() mutable
					{
						win->Items = LoadLinks(sources); 
					};
	
	actions.AddNew(shell);
	actions.AddNew(win);

	return actions;
}

CObject<CField> CShellServer::FindField(CUol & o)
{
	return Server->FindObject(o);
}

CObject<CFieldEnvironment> CShellServer::FindFieldEnvironmentByEntity(CUol & o)
{
	for(auto i : Server->Objects)
	{
		if(i->Url.Object.StartsWith(CFieldEnvironment::GetClassName()+L"-"))
		{
			CObject<CFieldEnvironment> de = Server->FindObject(i->Url);
			
			if(de->As<CFieldEnvironmentServer>()->Entity->Url == o)
			{
				return de;
			}
		}
	}

	return CObject<CFieldEnvironment>();
}

CObject<CFieldServer> CShellServer::GetField(CUol & o)
{
	return Server->FindObject(o);
}

bool CShellServer::CanExecute(const CUrq & u)
{
	if(u.Protocol == CUol::Protocol)
	{
		return CUsl(u) == Server->Url;
	}
	return false;
}

void CShellServer::Execute(const CUrq & u, CExecutionParameters * ep)
{
	EstablishConnections();

	if(CUol::IsValid(u))
	{
		CUol o(u);

		if(o.GetType() == CLink::GetClassName())
		{
			CObject<CLink> l = Server->FindObject(o);

			Nexus->Execute(l->Target, ep);
		} 
		else if(o.GetType() == CField::GetClassName() || 
				o.GetType() == CTheme::GetClassName() ||
				o.GetType() == CPicture::GetClassName() ||
				o.GetType() == CNotepad::GetClassName())
		{
			auto e = Server->FindObject(o);

			World->OpenEntity(e->Url, AREA_LAST_INTERACTIVE, dynamic_cast<CShowParameters *>(ep));
		}
		else
		{
			Log->ReportWarning(this, L"Object Class not supported: %s", o.Object);
		}
	}
	else if(CUsl::IsUsl(u))
	{
		if(u.Query.Contains(L"Action", L"Create"))
		{
			if(u.Query.Contains(L"Class", CField::GetClassName())) 
			{
				auto d = new CFieldServer(this);
				d->SetTitle(L"Field (New)");
				Server->RegisterObject(d, true);
				d->Free();
				
				World->OpenEntity(d->Url, AREA_FIELDS, dynamic_cast<CShowParameters *>(ep));
			}
			if(u.Query.Contains(L"Class", CTheme::GetClassName()))
			{
				CArray<std::pair<CString, CString>> types;
				types.push_back({L"All supported formats", L"*.vwm; "});

				for(auto i : Engine->TextureFactory->Formats)
				{
					types.back().second += CString::Join(i.second.Split(L" "), [](auto t){ return L"*." + t; }, L"; ") + L"; ";
				}

				auto paths = Core->Os->OpenFileDialog(0, types);

				if(!paths.empty())
				{
					auto d = new CTheme(this);
					Server->RegisterObject(d, true);
					d->Free();

					d->SetSource(Nexus->Storage->NativeToUniversal(paths.front()));

					World->OpenEntity(d->Url, AREA_THEME, dynamic_cast<CShowParameters *>(ep));
				}
			}
		}
	}
	else
		throw CException(HERE, L"Wrong Urq");

	//else
	//{
	//	auto sf = dynamic_cast<CShowFeatures *>(ep);
	//	World->OpenEntity(o, UOS_SPACE_MAIN, *sf, true);
	//}
}
