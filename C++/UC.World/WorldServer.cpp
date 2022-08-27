#include "stdafx.h"
#include "WorldServer.h"
#include "IMenu.h"
#include "MobileSkinModel.h"
#include "EnvironmentWindow.h"
#include "TextEdit.h"
#include "Button.h"
#include "Stack.h"
#include "Table.h"
#include "DefaultIcon.h"
#include "Logo.h"
#include "VrWorld.h"
#include "DesktopWorld.h"
#include "MobileWorld.h"
#include "GroupIcon.h"
#include "Solo.h"
#include "HighspaceGroupUnit.h"
#include "LowspaceGroupUnit.h"

using namespace uc;

static CWorldServer * This = null;
	
CServer * StartUosServer(CNexus * l, CServerRelease * info, CXon * command)
{
	if(command)
	{
		if(auto n = command->One(L"Name"))
		{
			if(n->Get<CString>() == WORLD_VR_EMULATION)
			{
				This = new CVrWorld(l, info);
			}
			else if(n->Get<CString>() == WORLD_MOBILE_EMULATION)
			{
				This = new CMobileWorld(l, info);
			}
		}
	}

	if(!This)
	{
		This = new CDesktopWorld(l, info);
	}

	return This;
}

void StopUosServer(CServer *)
{
	delete This;
}

CWorldServer::CWorldServer(CNexus * l, CServerRelease * si) : CStorableServer(l, si), CWorld(l)//, InteractiveMover(l), BackgroundMover(l)
{
	Server = this;
	Nexus = Server->Nexus;
	Core = Nexus->Core;
	Log  = Core->Supervisor->CreateLog(CWorldCapabilities::Name);
	Core->ExitRequested	+= ThisHandler(OnExitRequested);
	Nexus->Stopping += ThisHandler(OnNexusStopping);

	PfcUpdate	= new CPerformanceCounter(CWorldCapabilities::Name + L" update");
	Diagnostic	= Core->Supervisor->CreateDiagnostics(CWorldCapabilities::Name);
	Diagnostic->Updating += ThisHandler(OnDiagnosticsUpdating);

	DiagGrid.AddColumn(L"Name");
	DiagGrid.AddColumn(L"Class");
	DiagGrid.AddColumn(L"Position");
	DiagGrid.AddColumn(L"Rotation");
	DiagGrid.AddColumn(L"Scale");
	DiagGrid.AddColumn(L"Size");
	DiagGrid.AddColumn(L"Smax");
	DiagGrid.AddColumn(L"Pmax");
	DiagGrid.AddColumn(L"Expressions");
}

CWorldServer::~CWorldServer()
{
	while(auto i = Objects.Find([](auto j){ return j->Shared;  }))
	{
		DestroyObject(i, true);
	}

	if(Sphere)
	{
		Sphere->Free();
	}

	Area->Free();

	for(auto & i : RenderLayers)
		Engine->Renderer->RemoveLayer(i.second);

	for(auto & i : ActiveLayers)
		Engine->Interactor->RemoveLayer(i.second);
	
	delete HudView;
	delete NearView;
	delete MainView;
	delete ThemeView;

	for(auto i : Viewports)
	{
		delete i;
	}
	
	delete VisualGraph;
	delete ActiveGraph;

	delete Materials;
	delete Style;
	delete Engine;
	
	delete EngineConfig;
	delete WorldConfig;
	delete AreasConfig;

	delete PfcUpdate;

	for(auto & i : Modes)
	{
		delete i.WorldConfig;
		delete i.EngineConfig;
	}

	Core->ExitRequested	-= ThisHandler(OnExitRequested);
	Nexus->Stopping	-= ThisHandler(OnNexusStopping);
	Diagnostic->Updating -= ThisHandler(OnDiagnosticsUpdating);

	if(Storage)
	{
		Nexus->Disconnect(Storage);
	}
}

void CWorldServer::EstablishConnections()
{
	if(!Storage)
	{
		Storage = CStorableServer::Storage = Nexus->Connect(this, IFileSystem::InterfaceName, [&]{ Nexus->StopServer(this); });
	}
}

IInterface * CWorldServer::Connect(CString const & pr)
{
	return this;
}

void CWorldServer::Disconnect(IInterface * o)
{
}

void CWorldServer::Start()
{
	EstablishConnections();

	auto r = MapReleasePath(L"");

	for(auto & i : Storage->Enumerate(r, L".*"))
	{
		if(i.Type == CFileSystemEntry::Directory)
		{
			auto ws = Storage->ReadFile(CPath::Join(r, i.Name, L"World.xon"));
			auto es = Storage->ReadFile(CPath::Join(r, i.Name, L"Engine.xon"));
			Modes.push_back({
								i.Name,
								new CTonDocument(CXonTextReader(ws)), 
								new CTonDocument(CXonTextReader(es)), 
							});
			Storage->Close(ws);
			Storage->Close(es);
		}
	}

	ScreenshotId = Core->RegisterGlobalHotKey(MOD_ALT|MOD_CONTROL, VK_SNAPSHOT, [this](auto){ Engine->ScreenEngine->TakeScreenshot(Name); });
	
	auto sconfig = LoadReleaseDocument(Name + L"/World.xon"); 
	auto gconfig = LoadGlobalDocument(Name + L"/World.xon");
		
	if(gconfig)
	{
		WorldConfig = gconfig;
		delete sconfig;
	}
	else
		WorldConfig = sconfig;

	Layout	= WorldConfig->Get<CString>(L"Layout");
	Fov		= WorldConfig->Get<CFloat>(L"Fov");

	for(auto i : Command->Nodes)
	{
		if(i->Name == L"Layout") Layout = i->Get<CString>(); 
	}

	if(!(AreasConfig = LoadGlobalDocument(Name + L"/" + Layout + L".layout/Areas.xon")))
	{
		AreasConfig = LoadReleaseDocument(Name + L"/Areas.xon");
		Initializing = true;
	}
	
	auto df = MapReleasePath(L"Engine.xon");
	auto cf = MapReleasePath(Name + L"/Engine.xon");
	EngineConfig = new CConfig(Storage, df, cf);
		
	Engine = new CEngine(this, EngineConfig);
	Engine->ScreenEngine->SetLayout(Layout);

	Materials = new CMaterialPool(Engine);

	auto sf = Storage->ReadFile(MapReleasePath(Name + L"/Default.style"));
	Style = new CStyle(Engine, Materials, CXonTextReader(sf));
	Storage->Close(sf);

	auto arrow = CRectangleSectionMenuItem::CreateArrowMesh(this);
	Style->DefineMesh(L"RectangleSectionMenuItem/Arrow", arrow);
	arrow->Free();

	for(auto i : Engine->ScreenEngine->Screens)
	{
		 Targets.push_back(Engine->DisplaySystem->GetAppropriateDevice(i->As<CWindowScreen>())->AddTarget(i->As<CWindowScreen>()));
	}
				
	InitializeViewports();

	VisualGraph		= Engine->CreateVisualGraph();
	ActiveGraph		= Engine->Interactor->CreateActiveGraph(L"Main");

	InitializeGraphs();
	
	InitializeView();
	
	Area = new CArea(this);
	
	for(auto i : Viewports)
	{	
		auto & vs = Area->AllocateVisualSpace(i);
		VisualGraph->AddBack(vs.Space);

		auto & as = Area->AllocateActiveSpace(i);
		ActiveGraph->AddBack(as.Space);
	}

	Area->Load(AreasConfig->One(L"Area"));
	
	ServiceBackArea	= Area->Match(CArea::ServiceBack);
	ThemeArea		= Area->Match(CArea::Theme);
	FieldArea		= Area->Match(CArea::Fields)->As<CPositioningArea>();
	MainArea		= Area->Match(CArea::Main)->As<CPositioningArea>();
	HudArea			= Area->Match(CArea::Hud)->As<CPositioningArea>();
	TopArea			= Area->Match(CArea::Top)->As<CPositioningArea>();
	ServiceFrontArea= Area->Match(CArea::ServiceFront);

	ServiceBackArea	->SetView(MainView); 
	ThemeArea		->SetView(ThemeView); 
	FieldArea		->SetView(MainView); 
	MainArea		->SetView(MainView); 
	HudArea			->SetView(HudView); 
	TopArea			->SetView(HudView); 
	ServiceFrontArea->SetView(MainView); 
	
	if(Area->Match(CArea::Background))
	{
		BackArea = Area->Match(CArea::Background)->As<CPositioningArea>();
		BackArea->SetView(MainView); 
	}

	InitializeAreas();
	
	for(auto vp : Viewports)
	{
		RenderLayers[vp]	= Engine->Renderer->AddLayer(vp, VisualGraph, Area->VisualSpaces.Match(vp).Space);
		ActiveLayers[vp]	= Engine->Interactor->AddLayer(vp, Area->ActiveSpaces.Match(vp).Space);
	}

	auto logo = new CLogo(this);
	logo->SetText(WorldConfig->Get<CString>(L"Title") + L" Mode");
	auto a = AllocateUnit(logo);
	Show(a, CArea::ServiceFront, null);
	logo->Free();
	
	Sphere = new CSphere(this);
	auto sphere = AllocateUnit(Sphere);
	Show(sphere, CArea::ServiceBack, null);

	InitializeModels();

	Engine->Renderer->Update();

	for(auto i : AreasConfig->Many(L"Open"))
	{
		auto u = FindUnit(i->Get<CUol>());

		auto sf = new CShowParameters();
		sf->Activate = false;

		OpenUnit(u, CArea::Last, sf);

		sf->Free();

		///Engine->Update();
	}
	
	Engine->Start();

	Hide(a, null);

	Starting = false;
}

void CWorldServer::OnExitRequested()
{
	auto logo = new CLogo(this);
	logo->SetText(Core->RestartCommand.empty() ?  L"Exiting..." : L"Restarting...");
	auto a = AllocateUnit(logo);
	Show(a, CArea::ServiceFront, null);
	logo->Free();

	for(auto i : Area->Areas)
	{
		if(CUol::GetObjectId(i->Area->Name) != CArea::ServiceFront && CUol::GetObjectId(i->Area->Name) != CArea::Skin)
		{
			i->Area->DetachSpaces();
		}
	}

	Engine->Renderer->Update();
}

void CWorldServer::OnNexusStopping()
{
	Engine->Stop();

	//Storage->CreateGlobalDirectory(this);
	//Storage->CreateLocalDirectory(this);

	for(auto i : AreasConfig->Many(L"Open"))
		AreasConfig->Remove(i);

	AreasConfig->Remove(AreasConfig->One(L"Area"));

	while(auto u = Units.First())
	{
		if(u->Parent && u->Parent->Parent == Area && u->Lifespan == ELifespan::Permanent)
		{
			u->Parent->Remember(u);
			AreasConfig->Add(L"Open")->Set(u->Entity.Url);
		}

		Dealloc(u);
	}

	Area->Save(AreasConfig->Add(L"Area"));

	auto f = Storage->WriteFile(MapUserGlobalPath(Name + L"/" + Layout + L".layout/Areas.xon"));
	AreasConfig->Save(&CXonTextWriter(f, true));
	Storage->Close(f);

	f = Storage->WriteFile(MapUserGlobalPath(Name + L"/World.xon"));
	WorldConfig->Save(&CXonTextWriter(f));
	Storage->Close(f);

	Core->UnregisterGlobalHotKey(ScreenshotId);
}

CGroup * CWorldServer::CreateGroup(CString const & name)
{
	auto  o = new CGroup(this, name);
	RegisterObject(o, true);
	return o;
}

CStorableObject * CWorldServer::CreateObject(CString const & name)
{	
	CStorableObject * o = null;

	auto type = CUol::GetObjectClass(name);

	if(type == CGroup::GetClassName())	o = new CGroup(this, name);

	return o;
}

CUol CWorldServer::GenerateAvatar(CUol & entity, CString const & type)
{
	CList<CUol> avs;

	auto protocol = Nexus->Connect<IAvatarProtocol>(this, entity.Server);

	CUol avatar;
	CAvatar * a = null;

	if(protocol)
	{
		avs = protocol->GenerateSupportedAvatars(entity, type);
		if(!avs.empty())
		{
			avatar = avs.front();
		}
	}
	else
	{
		for(auto & i : Nexus->ConnectMany<IAvatarProtocol>(this))
		{
			avs = i->GenerateSupportedAvatars(entity, type);
			if(!avs.empty())
			{
				protocol = i;
				avatar = avs.front();
				break;
			}
		}
	}

	if(avatar.IsEmpty())
	{
		auto p = CMap<CString, CString>{{L"entity", entity.ToString()}, {L"type", type}};
		
		if(type == AVATAR_ICON2D)	avatar = CUol(CAvatar::Scheme, Instance, CGuid::Generate64(CDefaultIcon::GetClassName()), p);
	}

	
	return avatar;
}

CAvatar * CWorldServer::CreateAvatar(CUol & avatar, CString const & dir)
{
	CAvatar * a = null;

	auto protocol = Nexus->Connect<IAvatarProtocol>(this, avatar.Server);
		
	if(protocol)
	{
		a = protocol->CreateAvatar(avatar);
	
		if(a)
		{
			a->Protocol = protocol;
			///a->Destroying += ThisHandler(OnDependencyDestroying);

			a->SetDirectories(dir);
			a->SetEntity(CUol(avatar.Parameters(L"entity")));

			if(!dir.empty() && a->IsSaved())
			{
				a->Load();
			}
		}
	}
	else
	{	
		a = new CDefaultIcon(this);
		a->Protocol = CProtocolConnection<IAvatarProtocol>(CConnection(this, this, IAvatarProtocol::InterfaceName));
		RegisterObject(a, false);
		a->Free();
	}

	return a;
}

void CWorldServer::DestroyAvatar(CAvatar * a)
{
	if(a->Protocol == this)
		Server->DestroyObject(a, true);
	else
		a->Protocol->DestroyAvatar(a);
}

CUnit * CWorldServer::AllocateUnit(CModel * m)
{
	auto u = new CSolo(this, m, MainView);
	Units.push_back(u);
	return u;
}

CUnit * CWorldServer::AllocateUnit(CUol & entity, CString const & type)
{
	auto dir = CPath::Join(Instance, Name, Layout + L".layout/" + entity.Object);
	
	auto u = (CUnit *)null;

	if(entity.GetObjectClass() == CGroup::GetClassName())
	{	
		if(Tight)
			u = new CLowspaceGroupUnit(this, dir, entity, type, MainView);
		else
			u = new CHighspaceGroupUnit(this, dir, entity, type, MainView); 
	}
	else
	{
		if(!GenerateAvatar(entity, type).IsEmpty())
		{
			u = new CSolo(this, dir, entity, type, MainView);
		} 
		else
		{
			Log->ReportError(this, L"Can't open %s %s", type, entity.Object);
			return null;
		}
	}
	
	Units.push_back(u);
	return u;
}

void CWorldServer::Dealloc(CUnit * u)
{
	if(u->Lifespan == ELifespan::Permanent)
	{
		u->Save();
	}

	if(u->Parent)
	{
		u->Parent->Close(u);
	}

	Units.Remove(u);
	u->Free();
}

CUnit * CWorldServer::OpenEntity(CUol & entity, CString const & area, CShowParameters * f)
{
	auto u = FindUnit(entity);

	if(!u)
	{
		u = AllocateUnit(entity, Complexity);
	}

	if(u)
	{
		OpenUnit(u, area, f);
	}
	
	return u;
}

void CWorldServer::OpenUnit(CUnit * u, CString const & area, CShowParameters * f)
{
	///if(u->Avatarization->Model)
	///{
		Show(u, area, f);
	///}
	///else
	///{
	///	Dealloc(u);
	///	DeleteObject(u->Avatarization);
	///}
}

CUnit * CWorldServer::FindUnit(CUol & entity)
{
	auto load = [this](CUol & entity) -> CUnit *
				{
					CString e;
					auto l = MapUserGlobalPath(Name + L"/" + Layout + L".layout/" + entity.Object);
					
					if(Storage->Exists(l))
					{
						e = CPath::GetName(entity.Object);
					}

					if(e.empty())
					{
						auto ldir = MapUserGlobalPath(Name + L"/" + Layout + L".layout");
						
						for(auto & i : Storage->Enumerate(l, L"Group-.*").Where([](auto & i){ return i.Type == CFileSystemEntry::Directory; }))
						{
							auto l = CPath::Join(ldir, i.Name, CPath::GetName(entity.Object));

							if(Storage->Exists(l))
							{
								e = i.Name;
								break;
							}
						}
					}

					if(!e.empty())
					{
						CUnit * u = null;

						if(e.StartsWith(CGroup::GetClassName() + L"-"))
						{
							if(Tight)
								u = new CLowspaceGroupUnit(	this, 
															CPath::Join(Instance, Name, Layout + L".layout/" + e), 
															e,
															MainView);
							else
								u = new CHighspaceGroupUnit(this, 
															CPath::Join(Instance, Name, Layout + L".layout/" + e), 
															e,
															MainView);
						}
						else
						{
							u = new CSolo(	this, 
											CPath::Join(Instance, Name, Layout + L".layout/" + e), 
											MainView,
											e);
						}
												
						//u->Load(&CTonDocument(CXonTextReader(s)));
						Units.push_back(u);
	
					//	Storage->Close(s);
						return u;
					}

					return null;
				};

	auto u = Units.Find([&entity](CUnit * i){ return i->ContainsEntity(entity); });
	
	if(!u)
	{
		u = load(entity);
	}

	return u;
}

CUnit * CWorldServer::FindGroup(CArea * a)
{
	return null;
}

void CWorldServer::Show(CUnit * u, CString const & area, CShowParameters * f)
{
	auto prevMaster	= u->Parent;
	auto master		= (CArea *)null;
	auto origin		= CTransformation::Nan;

	if(area == CArea::LastInteractive)
	{
		master = Area->Find(u->LastInteractiveMaster);

		if(!master)
			master = Area->Match(u->GetDefaultInteractiveMasterTag());
	}
	else if(area == CArea::Last)
	{
		master = Area->Find(u->LastMaster);

		if(!master)
			master = Area->Match(u->GetDefaultInteractiveMasterTag());
	}
	else
	{
		master = Area->Find(area); // master area by name

		if(!master)
			master = Area->Match(area); // master area by tag
	}

	auto hidings = CollectHidings(u, master);

	if(f && f->Pick.Active)
	{
		origin = f->Pick.GetWorldPosition();
		origin.Scale = {0.1f, 0.1f, 0.1f};
	}

	if(prevMaster != master)
	{
		if(prevMaster)
		{
			origin = u->Transformation;
			prevMaster->Remember(u);
			prevMaster->Close(u);
		}

		master->Open(u, EAddLocation::Known, MainViewport, f ? f->Pick : CPick(), origin);

		u->LastMaster = master->Name;

		if(master->Interactive)
			u->LastInteractiveMaster = master->Name;
		else
			u->LastNoninteractiveMaster = master->Name;
	}

	if(f)
	{
		if(f->Activate || prevMaster == master)
			master->Activate(u, null, f->Pick, origin);
	
		if(origin.IsReal() && f->Animation.IsReal())
		{
			auto t = u->Transformation;
			StartShowAnimation(u, f, origin, t);
		}
	}

	if((!prevMaster || !prevMaster->Interactive) && master->Interactive)
	{
		UnitOpened(u, u->Transformation, f);
	}

	Showings.push_back(u);

	for(auto i : hidings)
	{
		if(!Showings.Contains(i))
		{
			if(i->LastNoninteractiveMaster.empty())
			{
				Hide(i, null);
			}
			else
			{
				Show(i, i->LastNoninteractiveMaster, null);
			}
		}
	}
	
	Showings.Remove(u); // do after hiding!
	///Log->ReportMessage(this, L"Shown: %s  ->  %s  -> %s", u->Avatarization->Avatar->Url.Object, u->Name, master->Name);
}

void CWorldServer::StartShowAnimation(CArea * a, CShowParameters * f, CTransformation & from, CTransformation & to)
{
	a->Transform(from);
	RunAnimation(a, CAnimated<CTransformation>(from, to, f->Animation));
}

void CWorldServer::StartHideAnimation(CArea * u, CHideParameters * f, CTransformation & from, CTransformation & to, std::function<void()> hide)
{
	if(!f || !f->End.IsReal() || !Engine->IsRunning())
	{
		hide();
	} 
	else
	{
		auto ani = CAnimated<CTransformation>(from, f->End, Style->GetAnimation(L"Animation"));

		Core->DoUrgent(	this,
						L"Hide animation",
						[this, u, ani, hide]() mutable
						{	
							if(ani.Animation.Running)
							{
								u->Transform(ani.GetNext());
								Engine->Update();
								return false;
							} 
							else
							{
								hide();
								return true;
							}
						});
	}
}

void CWorldServer::Hide(CUnit * u, CHideParameters * p)
{
	if(!u->IsUnder(Area))
		return;

	if(Showings.Contains(u))
		return;

	if(Hidings.Contains(u))
		return;

	Hidings.push_back(u);

	u->Parent->Remember(u);

	auto hide = [this, u]
				{
					UnitClosed(u);

					auto master = u->Parent;

					master->Close(u);
					u->LastNoninteractiveMaster.clear();

					if(u->Lifespan == ELifespan::Visibility)
					{
						auto name = u->Name;
						Dealloc(u);
						master->Forget(name);
					}

					Hidings.Remove(u);
				};

	StartHideAnimation(u, p, u->Transformation, p->End, hide);
}

void CWorldServer::Attach(CElement * m, CUol & alloc)
{
	///auto a = Allocations.Find([m](auto i){ return i->Model == m; });
	///auto b = FindAllocation(alloc, CUol(), CUol());
	///
	///if(a && a->IsOpen())
	///{	
	///	a->Attached.push_back(alloc);
	///	b->Parents.push_back(a);
	///	auto b = OpenAllocation(alloc, CArea::LastInteractive, CShowFeatures());
	///	
	///	if(a->Avatar)
	///	{
	///		a->Avatar->GetInfo(Url)->Add(L"Attached")->Set(alloc);
	///	}
	///}
	throw CException(HERE, L"Not implemented");
}

void CWorldServer::Detach(CElement * m, CUol & l)
{
	///auto a = Allocations.Find([&](auto i){ return i->Model == m; });
	///auto b = FindAllocation(l, CUol(), CUol());
	///
	///if(a && a->IsOpen())
	///{	
	///	a->Attached.Remove(l);
	///	b->Parents.Remove(a);
	///	Hide(b, CHideFeatures());
	///
	///	if(a->Avatar)
	///	{
	///		auto p = a->Avatar->GetInfo(Url)->Find(L"Attached", l);
	///		a->Avatar->GetInfo(Url)->Remove(p);
	///	}
	///}
	throw CException(HERE, L"Not implemented");
}

bool CWorldServer::IsAttachedTo(CUol & l, CElement * to)
{
	///auto b = Allocations.Find([to](auto i){ return i->Avatar && i->Model == to; });
	///return b->Attached.Contains(l);
	throw CException(HERE, L"Not implemented");
}

bool CWorldServer::IsAttachedTo(CUol & l, CUol & to)
{
	///auto b = FindAllocation(to, CUol(), CUol());
	///return b->Attached.Contains(l);
	throw CException(HERE, L"Not implemented");
}
	
void CWorldServer::Drag(CArray<CDragItem> & d)
{
	for(auto i : d)
	{
		Drags.push_back(i);
	}
		
	DragAllocation = CreateAvatar(GenerateAvatar(CUol(d.front().Object), AVATAR_ICON2D), L"");
	///DragAllocation->Area->SetView(MainView);

	auto n = DragAllocation;
	n->Visual->Clipping = EClipping::No;
	n->Active->Clipping = EClipping::No;
	
	CCardMetrics m;
	m.FaceSize = CSize(48, 48, 48);
	m.FaceMargin = CFloat6(0);
	///n->As<CCard>()->SetAppearance(ECardTitleMode::No, m);
	n->UpdateLayout(CLimits(m.FaceSize, m.FaceSize), true);
	

	throw CException(HERE, L"Need update");
	///MainVisualGraph->GetSpace(TopArea->Space)->AddNode(n->Visual);

	DragDefaultAvatar = DragAllocation;
	DragCurrentAvatar = DragAllocation;

	ActiveGraph->Root->IsPropagator = false;
}

void CWorldServer::CancelDragDrop()
{
	if(DropTarget)
	{
		DropTarget->As<IDropTarget>()->Leave(Drags, DragCurrentAvatar);
		DropTarget->Free();
		DropTarget = null;
	}

	auto n = DragAllocation;
	
	throw CException(HERE, L"Need update");
	///MainVisualGraph->GetSpace(TopArea->Space)->RemoveNode(n->Visual);
	
	///Dealloc(DragAllocation);
	//DragAllocation->Free();
	
	Drags.clear();
	Engine->InputSystem->First<CMouse>()->SetImage(null);
	ActiveGraph->Root->IsPropagator = true;
}

CUnit * CWorldServer::GetUnit(CActive * a)
{
	auto m = a->AncestorOwnerOf<CModel>();

	while(m && m->Parent && m->Parent->Active->AncestorOwnerOf<CModel>())
	{
		m = m->Parent->Active->AncestorOwnerOf<CModel>();
	}
	
	if(m)
	{
		return m->Unit;
	}
	else
	{
		return null;
	}
}

void CWorldServer::RunAnimation(CElement * n, CAnimated<CTransformation> a)
{
	n->Take();
	Core->DoUrgent(	this,
					L"Element animation",
					[this, n, a]() mutable 
					{	
						if(a.Animation.Running)
						{
							n->Transform(a.GetNext());
							Engine->Update();
							return false;
						} 
						else
						{
							n->Free();
							return true;
						}
					});
}

void CWorldServer::RunAnimation(CArea * n, CAnimated<CTransformation> a)
{
	n->Take();
	Core->DoUrgent(	this,
					L"Area animation",
					[this, n, a]() mutable 
					{	
						if(a.Animation.Running)
						{
							n->Transform(a.GetNext());
							Engine->Update();
							return false;
						} 
						else
						{
							n->Free();
							return true;
						}
					});
}

void CWorldServer::Execute(CXon * command, CExecutionParameters * parameters)
{
	auto f = command->Nodes.First();

	if(f->Name == L"Start")
	{
		Start();
	}

	if(f->Name == L"Open" && f->Any(L"Url"))
	{
		CUrl o(f->Get<CString>(L"Url"));

		if(o.Scheme == CWorldEntity::Scheme)
		{
			OpenEntity(CUol(f->Get<CString>(L"Url")), CArea::LastInteractive, dynamic_cast<CShowParameters *>(parameters));
		}
	}
}

CView * CWorldServer::Get(const CString & name)
{
	if(name == ThemeView->Name)
	{
		return ThemeView;
	}

	if(name == MainView->Name)
	{
		return MainView;
	}
	return null;
}

void CWorldServer::OnDiagnosticsUpdating(CDiagnosticUpdate & u)
{
	DiagGrid.Clear();

	std::function<void(CElement *, const CString &)> dumpn =	[this, &dumpn, &u](auto n, auto & s)
																{
																	if(Diagnostic->ShouldProceed(u, DiagGrid.GetSize()))
																	{
																		auto & r = DiagGrid.AddRow();
	
																		if(Diagnostic->ShouldFill(u, DiagGrid.GetSize()))
																		{
																			r.SetNext(s + n->Name);
																			r.SetNext(n->GetInstanceName());
																			r.SetNext(n->Transformation.Position.ToNiceString());
																			r.SetNext(n->Transformation.Rotation.ToNiceString());
																			r.SetNext(n->Transformation.Scale.ToNiceString());
																			r.SetNext(L"%7.0f %7.0f", n->Size.W, n->Size.H);
																			r.SetNext(L"%7.0f %7.0f", n->Slimits.Smax.W, n->Slimits.Smax.H);
																			r.SetNext(L"%7.0f %7.0f", n->Slimits.Pmax.W, n->Slimits.Pmax.H);
																			r.SetNext(n->GetStatus());
																		}
																			
																		for(auto i : n->Nodes)
																		{
																			dumpn(i, s + L"  ");
																		}
																	}
																};

	std::function<void(CPlacement *, const CString &)> dumpa =	[this, &dumpa, &dumpn, &u](auto a, auto & tab)
																{
																	if(!a->Area)
																	{
																		return;
																	}

																	if(Diagnostic->ShouldProceed(u, DiagGrid.GetSize()))
																	{
																		auto & r = DiagGrid.AddRow();
	
																		if(Diagnostic->ShouldFill(u, DiagGrid.GetSize()))
																		{
																			auto c = RGB(0, 204, 204);
																			r.SetNext(c, tab + a->Name);
																			r.SetNext(c, a->Area->GetInstanceName());
																			r.SetNext(c, a->Area->Transformation.Position.ToNiceString());
																			r.SetNext(c, a->Area->Transformation.Rotation.ToNiceString());
																			r.SetNext(c, a->Area->Transformation.Scale.ToNiceString());
																			r.SetNext(L"");
																			r.SetNext(L"");
																			r.SetNext(L"");
																			r.SetNext(c, CString::Join(a->Area->Tags, L" "));
																		}

																		if(auto g = a->Area->As<CGroupUnit>())
																		{
																			for(auto i : g->Header->Tabs)
																			{
																				if(i->Model)
																					dumpn(i->Model, tab + L"  ");
																			}
																		}
																		else if(auto u = a->Area->As<CSolo>())
																		{
																			dumpn(u->Model, tab + L"  ");
																		}

																		for(auto i : a->Area->Areas)
																		{
																			dumpa(i, tab + L"  ");
																		}
																	}
																};
	for(auto i : Area->Areas)
	{
		dumpa(i, L"");
	}

	Diagnostic->Add(u, DiagGrid);
}

CProtocolConnection<IAvatarProtocol> CWorldServer::FindAvatarSystem(CUol & e, CString const & type)
{
	CList<CUol> avs;

	auto p = Nexus->Connect<IAvatarProtocol>(this, e.Server);

	if(p)
	{
		avs = p->GenerateSupportedAvatars(e, type);
		if(!avs.empty())
		{
			return p;
		}
	}
	else
	{
		for(auto & i : Nexus->ConnectMany<IAvatarProtocol>(this))
		{
			avs = i->GenerateSupportedAvatars(e, type);
			if(!avs.empty())
			{
				return i;
			}
		}
	}

	return CProtocolConnection<IAvatarProtocol>();
}

CElement * CWorldServer::CreateElement(CString const & name, CString const & type)
{
	//if(type == CWindow::GetClassName())		return new CWindow(this, Hub, Style, name); else
	if(type == CText::GetClassName())		return new CText(this, Style, name); else
	if(type == CTextEdit::GetClassName())	return new CTextEdit(this, Style, name); else
	if(type == CButton::GetClassName())		return new CButton(this, Style, name); else
	if(type == CStack::GetClassName())		return new CStack(this, Style, name); else
	if(type == CTable::GetClassName())		return new CTable(this, Style, name); else

	return null;
}


CInterObject * CWorldServer::GetEntity(CUol & e)
{
	return Server->FindObject(e);
}

CList<CUol> CWorldServer::GenerateSupportedAvatars(CUol & e, CString const & type)
{
	CList<CUol> l;

	auto p = CMap<CString, CString>{{L"entity", e.ToString()}, {L"type", type}};

	if(e.GetObjectClass() == CGroup::GetClassName())
	{
		if(type == AVATAR_ICON2D)	l.push_back(CUol(CAvatar::Scheme, Instance, CGuid::Generate64(CGroupIcon::GetClassName()), p));
	}

	return l;

}

CAvatar * CWorldServer::CreateAvatar(CUol & u)
{
	CAvatar * a = null;
	
	if(u.Server == Instance)
	{
		if(u.GetObjectClass() == CGroupIcon::GetClassName())	a = new CGroupIcon(this, u.Object); else
		if(u.GetObjectClass() == CDefaultIcon::GetClassName())	a = new CDefaultIcon(this, u.Object); else

		return null;
	}

	a->Url = u;
	RegisterObject(a, false);
	a->Free();
	
	return a;
}
