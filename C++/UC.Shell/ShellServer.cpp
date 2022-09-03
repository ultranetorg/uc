#include "StdAfx.h"
#include "ShellServer.h"

using namespace uc;

static CShellServer * This = null;

CServer * CreateUosServer(CNexus * l, CServerInstance * info)
{
	This = new CShellServer(l, info);
	return This;
}

void DestroyUosServer(CServer *)
{
	delete This;
}

CShellServer::CShellServer(CNexus * l, CServerInstance * si) : CStorableServer(l, si)
{
	Server	= this;
	Nexus	= Server->Nexus;
	Core	= Nexus->Core;
	Log		= l->Core->Supervisor->CreateLog(Instance->Name);
}

CShellServer::~CShellServer()
{
	while(auto i = Objects.Find([](auto j){ return j->Shared;  }))
	{
		DestroyObject(i, true);
	}

	if(Menu)
	{
		Menu->Close();
		Menu->Free();
	}

	delete ImageExtractor;
	delete Style;

	if(World)
	{
		if(World->Sphere)
		{
			World->Sphere->Active->MouseEvent -= ThisHandler(OnWorldSphereMouse);
		}

		Nexus->Disconnect(World);
	}

	if(Storage)
	{
		Nexus->Disconnect(Storage);
	}
}

void CShellServer::EstablishConnections()
{
	if(!Storage)
	{
		Storage = CStorableServer::Storage = Nexus->Connect(this, IFileSystem::InterfaceName, [&]{ Nexus->StopServer(Instance); });
	}

	if(!World)
	{
		World = Nexus->Connect(this, WORLD_PROTOCOL, [&]{ Nexus->StopServer(Instance); });

		Engine			= World->Engine;
		Style			= World->Style->Clone();
		ImageExtractor	= new CImageExtractor(World, Server);
	}
}

IInterface * CShellServer::Connect(CString const & p)
{
	if(p == IImageExtractor::InterfaceName)	return ImageExtractor; else
	if(p == ITray::InterfaceName)			return FindObject<IInterface>(SHELL_TRAY_1); else
		return this;
}

void CShellServer::Disconnect(IInterface * o)
{
}

CStorableObject * CShellServer::CreateObject(CString const & name)
{
	EstablishConnections();

	CStorableObject * o = null;

	auto type = CUol::GetObjectClass(name);

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

CInterObject * CShellServer::GetEntity(CUol & e)
{
	return Server->FindObject(e);
}

CList<CUol> CShellServer::GenerateSupportedAvatars(CUol & e, CString const & type)
{
	CList<CUol> l;

	auto p = CMap<CString, CString>{{L"entity", e.ToString()}, {L"type", type}};

	if(e.GetObjectClass() == SHELL_HUD)
	{
		if(type == AVATAR_ENVIRONMENT)	l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CHudEnvironment::GetClassName()), p));
	}

	if(e.GetObjectClass() == CHistory::GetClassName())
	{
		if(type == AVATAR_WIDGET)		l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CHistoryWidget::GetClassName()), p));
	}

	if(e.GetObjectClass() == CBoard::GetClassName())
	{
		if(type == AVATAR_WIDGET)		l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CBoardWidget::GetClassName()), p));
		if(type == AVATAR_ENVIRONMENT)	l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CBoardEnvironment::GetClassName()), p));
	}

	if(e.GetObjectClass() == CField::GetClassName())
	{
		if(type == AVATAR_ENVIRONMENT)	l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CFieldEnvironment::GetClassName()), p)); else
		if(type == AVATAR_WIDGET)		l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CFieldWidget::GetClassName()), p)); else
		if(type == AVATAR_ICON2D)		l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CFieldIcon::GetClassName()), p));
	}

	if(e.GetObjectClass() == CDirectoryMenu::GetClassName())
	{
		if(type == AVATAR_WIDGET)		l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CMenuWidget::GetClassName()), p));
	}

	if(e.GetObjectClass() == CApplicationsMenu::GetClassName())
	{
		if(type == AVATAR_WIDGET)		l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CSystemMenuWidget::GetClassName()), p));
	}

	if(e.GetObjectClass() == CTheme::GetClassName())
	{
		if(type == AVATAR_WIDGET)		l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CThemeWidget::GetClassName()), p)); else
		if(type == AVATAR_ENVIRONMENT)	l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CThemeEnvironment::GetClassName()), p));;
	}

	if(e.GetObjectClass() == CLink::GetClassName())
	{
		if(type == AVATAR_ICON2D)		l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CLinkIcon::GetClassName()), p));;
		if(type == AVATAR_ENVIRONMENT)	l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CLinkProperties::GetClassName()), p));;
	}

	if(e.GetObjectClass() == CNotepad::GetClassName())
	{
		if(type == AVATAR_ICON2D)		l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CNotepadIcon::GetClassName()), p)); else
		if(type == AVATAR_WIDGET)		l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CNotepadWidget::GetClassName()), p));
	}

	if(e.GetObjectClass() == CPicture::GetClassName())
	{
		if(type == AVATAR_ICON2D)		l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CPictureIcon::GetClassName()), p)); else
		if(type == AVATAR_WIDGET)		l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CPictureWidget::GetClassName()), p));
	}

	if(e.GetObjectClass() == CTray::GetClassName())
	{
		if(type == AVATAR_WIDGET)		l.push_back(CUol(CAvatar::Scheme, Instance->Name, CGuid::Generate64(CTrayWidget::GetClassName()), p));
	}

	return l;

}

CAvatar * CShellServer::CreateAvatar(CUol & entity)
{
	EstablishConnections();

	CAvatar * a = null;
	
	if(entity.Scheme == CAvatar::Scheme && entity.Server == Instance->Name)
	{
		if(entity.GetObjectClass() == CHudEnvironment::GetClassName())		a = new CHudEnvironment(this, entity.Object); else 

		if(entity.GetObjectClass() == CHistoryWidget::GetClassName())		a = new CHistoryWidget(this, entity.Object); else 

		if(entity.GetObjectClass() == CBoardWidget::GetClassName())			a = new CBoardWidget(this, entity.Object); else 
		if(entity.GetObjectClass() == CBoardEnvironment::GetClassName())	a = new CBoardEnvironment(this, entity.Object); else 

		if(entity.GetObjectClass() == CFieldEnvironment::GetClassName())	a = new CFieldEnvironmentServer(this, entity.Object); else 
		if(entity.GetObjectClass() == CFieldWidget::GetClassName())			a = new CFieldWidget(this, entity.Object); else 
		if(entity.GetObjectClass() == CFieldIcon::GetClassName())			a = new CFieldIcon(this, entity.Object); else 

		if(entity.GetObjectClass() == CMenuWidget::GetClassName())			a = new CMenuWidget(this, entity.Object); else 
		if(entity.GetObjectClass() == CSystemMenuWidget::GetClassName())	a = new CSystemMenuWidget(this, entity.Object); else 

		if(entity.GetObjectClass() == CThemeWidget::GetClassName())			a = new CThemeWidget(this, entity.Object); else
		if(entity.GetObjectClass() == CThemeEnvironment::GetClassName())	a = new CThemeEnvironment(this, entity.Object); else 

		if(entity.GetObjectClass() == CLinkIcon::GetClassName())			a = new CLinkIcon(this, entity.Object); else 
		if(entity.GetObjectClass() == CLinkProperties::GetClassName())		a = new CLinkProperties(this, entity.Object); else 
		
		if(entity.GetObjectClass() == CNotepadIcon::GetClassName())			a = new CNotepadIcon(this, entity.Object); else 
		if(entity.GetObjectClass() == CNotepadWidget::GetClassName())		a = new CNotepadWidget(this, entity.Object); else

		if(entity.GetObjectClass() == CPictureIcon::GetClassName())			a = new CPictureIcon(this, entity.Object); else 
		if(entity.GetObjectClass() == CPictureWidget::GetClassName())		a = new CPictureWidget(this, entity.Object); else

		if(entity.GetObjectClass() == CTrayWidget::GetClassName())			a = new CTrayWidget(this, entity.Object); 
		
		else 
			return null;
	}
	else
		return null;

	a->Url = entity;
	
	Server->RegisterObject(a, false);
	a->Free();
	
	return a;
}

void CShellServer::DestroyAvatar(CAvatar * a)
{
	Server->DestroyObject(a, true);
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

													for(auto & i : paths)
													{
														auto link = new CLink(this);
														link->SetTarget((CUrl)Storage->ToUol(Storage->NativeToUniversal(i)));
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
													v->SetFile(Storage->NativeToUniversal(paths.front()));
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
													v->SetFile(Storage->NativeToUniversal(paths.front()));
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
												o->AddPath(Storage->NativeToUniversal(list.front()));
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

	auto shell = new CMenuItem(GetTitle());
	
	//auto c = Instance + L"{" + IExecutor::CreateDirective + L" ";

	CTonDocument c;
	c.Add(Instance->Name)->Add(IExecutor::CreateDirective);
		
	shell->Items.AddNew(new CMenuItem(L"Field", [=](auto args)
												{
													auto cc = c;
													cc.One(Instance->Name)->Add(L"class")->Set(CField::GetClassName());
													Core->Execute(cc.One(Instance->Name), sh_new<CShowParameters>(args, Style)); 
												}));

	shell->Items.AddNew(new CMenuItem(L"Theme", [=](auto args)
												{
													auto cc = c;
													cc.One(Instance->Name)->Add(L"class")->Set(CTheme::GetClassName());
													Core->Execute(cc.One(Instance->Name), sh_new<CShowParameters>(args, Style)); 
												}));


	auto win = new CMenuItem(L"Windows");

	TCHAR			szPath[MAX_PATH];
	CList<CString>	sources;

	if(SUCCEEDED(SHGetFolderPath(NULL, CSIDL_COMMON_PROGRAMS, NULL, 0, szPath)))
	{
		sources.push_back(CPath::Join(IFileSystem::This, CPath::Universalize(szPath)));
	}

	if(SUCCEEDED(SHGetFolderPath(NULL, CSIDL_PROGRAMS, NULL, 0, szPath)))
	{
		sources.push_back(CPath::Join(IFileSystem::This, CPath::Universalize(szPath)));
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

void CShellServer::Execute(CXon * command, CExecutionParameters * parameters)
{
	EstablishConnections();

	auto f = command->Nodes.First();

	if(f->Name == L"CreateDefaultObjects")
	{
		TCHAR szPath[MAX_PATH];

		// hud
		auto hud = new CFieldServer(this, SHELL_HUD_1);
		hud->SetDefaultInteractiveMaster(CArea::Hud);
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
		CList<CString> items;
	
		if(SUCCEEDED(SHGetFolderPath(NULL, CSIDL_DESKTOP, NULL, 0, szPath))) 
		{
			auto  dir = CPath::Join(IFileSystem::This, CPath::Universalize(szPath));
			
			for(auto & i : Storage->Enumerate(dir, L".*"))
			{
				items.push_back(CPath::Join(dir, i.Name));
			}
		}
	
		if(SUCCEEDED(SHGetFolderPath(NULL, CSIDL_COMMON_DESKTOPDIRECTORY, NULL, 0, szPath)))
		{
			auto  dir = CPath::Join(IFileSystem::This, CPath::Universalize(szPath));

			for(auto & i : Storage->Enumerate(dir, L".*"))
			{
				items.push_back(CPath::Join(dir, i.Name));
			}
		}
			
		items.RemoveIf([](auto & i){ return i == L"desktop.ini"; });

		CList<CUol> links;
		
		for(auto & i : items)
		{
			auto link = new CLink(this);
			link->SetTarget((CUrl)Storage->ToUol(i));
			Server->RegisterObject(link, true);
			links.push_back(link->Url);
			link->Free();
		}

		home->Add(links, AVATAR_ICON2D);

		if(SUCCEEDED(SHGetFolderPath(NULL, CSIDL_MYPICTURES, NULL, 0, szPath)))
		{
			int n = 0;

			auto dir = CPath::Join(IFileSystem::This, CPath::Universalize(szPath));

			for(auto & i : Storage->Enumerate(dir, L".*"))
			{
				auto e = CPath::GetExtension(i.Name);
				
				if(e == L"jpg" || e == L"png")
				{
					auto p = new CPicture(this);
					p->SetFile(CPath::Join(dir, i.Name));
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
		}

		// Usage.txt notepad
		auto v = new CNotepad(this);
		v->SetFile(CPath::Join(IFileSystem::Software, Core->CurrentReleaseSubPath, L"About.txt"));
		Server->RegisterObject(v, true);
		home->Add(v->Url, AVATAR_WIDGET);
		v->Free();

		// Mouse.png picture	
		auto p = new CPicture(this);
		p->SetFile(MapReleasePath(L"Mouse.png"));
		Server->RegisterObject(p, true);
		home->Add(p->Url, AVATAR_WIDGET);
		p->Free();

		// theme
		auto t = new CTheme(this, SHELL_THEME_1);
		t->SetSource(MapReleasePath(L"Spaceland.vwm"));
		Server->RegisterObject(t, true);
		t->Free();

		auto d = new CFieldServer(this, SHELL_FIELD_HOME);
		d->SetTitle(L"Home");
		Server->RegisterObject(d, true);
		d->Free();

		d->Add(CUol(CWorldEntity::Scheme, Instance->Name, SHELL_FIELD_MAIN), AVATAR_ICON2D);
		d->Add(CUol(CWorldEntity::Scheme, Instance->Name, SHELL_FIELD_PICTURES), AVATAR_ICON2D);
		d->Add(CUol(CWorldEntity::Scheme, Instance->Name, SHELL_FIELD_WORK), AVATAR_ICON2D);
	}
	else if(f->Name == L"Start")
	{
		Server->FindObject(SHELL_HISTORY_1); // force to revive
	
		if(World->Initializing)
		{
			if(World->Complexity == AVATAR_ENVIRONMENT)
			{
				World->OpenEntity(CUol(CWorldEntity::Scheme, Instance->Name, SHELL_HUD_1), CArea::Hud, null);
	
				if(World->Free3D)
				{
					World->OpenEntity(CUol(CWorldEntity::Scheme, Instance->Name, SHELL_BOARD_1), CArea::Near, null);
				}
			}
	
			auto f = new CShowParameters(); 
			f->PlaceOnBoard = true;
			
			auto u = World->OpenEntity(CUol(CWorldEntity::Scheme, Instance->Name, SHELL_FIELD_WORK), CArea::Fields, f);
			
			if(World->BackArea)
				World->Show(u, CArea::Background, null);
	
			u = World->OpenEntity(CUol(CWorldEntity::Scheme, Instance->Name, SHELL_FIELD_PICTURES), CArea::Fields, f);
			
			if(World->BackArea)
				World->Show(u, CArea::Background, null);
			
			u = World->OpenEntity(CUol(CWorldEntity::Scheme, Instance->Name, SHELL_FIELD_MAIN), CArea::Fields, f);
			
			if(World->BackArea)
				World->Show(u, CArea::Background, null);
	
			World->OpenEntity(CUol(CWorldEntity::Scheme, Instance->Name, SHELL_THEME_1), CArea::Theme, null);
			
			if(World->FullScreen)
			{
				World->OpenEntity(CUol(CWorldEntity::Scheme, Instance->Name, SHELL_FIELD_HOME), CArea::Fields, f);
			}
	
			f->Free();
		}
	
		if(World->FullScreen)
		{
			World->Components[WORLD_HISTORY] = CUol(CWorldEntity::Scheme, Instance->Name, SHELL_HISTORY_1);
			World->Components[WORLD_BOARD]	 = CUol(CWorldEntity::Scheme, Instance->Name, SHELL_BOARD_1);
			World->Components[WORLD_TRAY]	 = CUol(CWorldEntity::Scheme, Instance->Name, SHELL_TRAY_1);
	
			World->GlobalHotKeys[EKeyboardControl::GlobalHome] =[this](auto k)
																{ 
																	World->OpenEntity(CUol(CWorldEntity::Scheme, Instance->Name, SHELL_FIELD_HOME), CArea::LastInteractive, null);
																};
		}
	
		if(World->Sphere)
		{
			World->Sphere->Active->MouseEvent[EListen::Normal] += ThisHandler(OnWorldSphereMouse);
		}
	}
	else if(f->Name == CCore::OpenDirective && command->Any(CCore::UrlArgument))
	{
		CUrl u(command->Get<CString>(CCore::UrlArgument));

		if(u.Scheme == CWorldEntity::Scheme)
		{
			CUol o(u);

			if(o.GetObjectClass() == CLink::GetClassName())
			{
				CObject<CLink> l = FindObject(o);
	
				Core->Open(l->Target, parameters);
			} 
			else if(o.GetObjectClass() == CField::GetClassName() || 
					o.GetObjectClass() == CTheme::GetClassName() ||
					o.GetObjectClass() == CPicture::GetClassName() ||
					o.GetObjectClass() == CNotepad::GetClassName())
			{
				auto e = FindObject(o);
	
				World->OpenEntity(e->Url, CArea::LastInteractive, dynamic_cast<CShowParameters *>(parameters));
			}
			else
			{
				Log->ReportWarning(this, L"Object Class not supported: %s", o.Object);
			}
		}
	}
	else if(f->Name == IExecutor::CreateDirective)
	{
		if(command->Get<CString>(L"class") == CField::GetClassName()) 
		{
			auto d = new CFieldServer(this);
			d->SetTitle(L"Field (New)");
			Server->RegisterObject(d, true);
			d->Free();
				
			World->OpenEntity(d->Url, CArea::Fields, dynamic_cast<CShowParameters *>(parameters));
		}
		if(command->Get<CString>(L"class") == CTheme::GetClassName())
		{
			CArray<std::pair<CString, CString>> types;
			types.push_back({L"All supported formats", L"*.vwm; "});

			for(auto & i : Engine->TextureFactory->Formats)
			{
				types.back().second += CString::Join(i.second.Split(L" "), [](auto t){ return L"*." + t; }, L"; ") + L"; ";
			}

			auto paths = Core->Os->OpenFileDialog(0, types);

			if(!paths.empty())
			{
				auto d = new CTheme(this);
				Server->RegisterObject(d, true);
				d->Free();

				d->SetSource(Storage->NativeToUniversal(paths.front()));

				World->OpenEntity(d->Url, CArea::Theme, dynamic_cast<CShowParameters *>(parameters));
			}
		}
	}
	else
		throw CException(HERE, L"Wrong command");

	//else
	//{
	//	auto sf = dynamic_cast<CShowFeatures *>(ep);
	//	World->OpenEntity(o, UOS_SPACE_MAIN, *sf, true);
	//}
}
