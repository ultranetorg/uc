#include "stdafx.h"
#include "FieldElement.h"
#include "IShellFriend.h"

using namespace uc;

CFieldElement::CFieldElement(CShellLevel * l, const CString & name) : CRectangle(l->World, name), Positioning(true)
{
	Level = l;
	EnableActiveBody = false;

	Complexity	= L"";
	Free3D		= false;
	FullScreen	= false;
	Tight		= true;
		
	Active->MouseEvent[EListen::PrimaryAll]	+= ThisHandler(OnMouseFilter);
	Active->MouseEvent[EListen::NormalAll]	+= ThisHandler(OnMouse);
	Active->TouchEvent[EListen::NormalAll]	+= ThisHandler(OnTouch);
	
	SelectionMesh = new CFrameMesh(Level->World->Engine->Level);
	Selection = new CVisual(Level->World->Engine->Level, L"selection", SelectionMesh, Level->World->Materials->GetMaterial(Level->Style->Get<CFloat4>(L"Selection/Border/Material")), CMatrix::Identity);

	Positioning.GetView					= [this]		 { return GetUnit()->GetActualView(); };
	Positioning.Bounds[null]			= [this]		 { return Surface->Polygon; };
	Positioning.Transformation[null]	= [this](auto vp){ return (Surface->Active->FinalMatrix * GetUnit()->ActiveSpaces.Match(vp).Space->Matrix).Decompose(); };

	Express(L"W", [this]{ return Slimits.Smax.W; });
	Express(L"H", [this]{ return Slimits.Smax.H; });
}
	
CFieldElement::~CFieldElement()
{	
	if(Menu)
	{
		Menu->Close();
		Menu->Free();
	}

	if(Entity)
		Entity->Added -= ThisHandler(OnItemAdded);

	while(auto i = Items.First())
	{
		RemoveItem(i);
	}

	if(Surface)
	{
		RemoveNode(Surface);
	}

	SelectionMesh->Free();
	Selection->Free();
}

void CFieldElement::Load()
{
	for(auto i : Items)
	{
		RemoveNode(i);
	}
	Items.Clear();

	auto url = Level->Nexus->MapPath(UOS_MOUNT_USER_GLOBAL, CPath::Join(Directory, L"Store.xon"));

	if(Level->Storage->Exists(url))
	{
		auto store = Level->Storage->OpenReadStream(url);

		auto & doc = CTonDocument(CXonTextReader(store));
		CMeshStore		mhs(Level->World);
		CMaterialStore	mts(Level->World);

		mhs.Load(&doc);
		mts.Load(&doc);

		auto g = Level->Storage->OpenDirectory(Level->Nexus->MapPath(UOS_MOUNT_USER_GLOBAL, Directory));

		for(auto & f : g->Enumerate(L"FieldItemElement-*.xon"))
		{
			auto fie = new CFieldItemElement(Level, this, CPath::Join(Directory, CPath::GetName(f.Path)));
			fie->Load(&mhs, &mts, Entity);

			if(fie->Entity)
			{
				fie->Express(L"P", [this]{ return CFloat6(ItemPadding); });

				fie->Active->StateChanged += ThisHandler(OnItemStateModified);
				AddNode(fie);
				Items.Add(fie);

				Added(fie);

				fie->Free();
			}
			else
			{
				fie->Free();
			}

		}

		Level->Storage->Close(g);
		Level->Storage->Close(store);
	}
}

void CFieldElement::Save()
{
	CMeshStore		mhs(Level->World);
	CMaterialStore	mts(Level->World);

	for(auto i : Items)
	{
		i->Save(&mhs, &mts);
	}

	CTonDocument d;
	mhs.Save(&d);
	mts.Save(&d);

	auto f = Level->Storage->OpenWriteStream(Level->Nexus->MapPath(UOS_MOUNT_USER_GLOBAL, CPath::Join(Directory, L"Store.xon")));
	d.Save(&CXonTextWriter(f, true));
	Level->Storage->Close(f);
}

void CFieldElement::SetDirectories(CString const & dir)
{
	Directory = dir;
}

void CFieldElement::SetField(CFieldServer * o)
{
	if(Entity)
		Entity->Added -= ThisHandler(OnItemAdded);

	Entity = o;

	if(Entity)
		Entity->Added += ThisHandler(OnItemAdded);
}

void CFieldElement::SetSurface(CFieldSurface * s)
{
	if(Surface)
	{
		RemoveNode(Surface);
		Surface = null;
	}

	if(s)
	{
		AddNode(s);
	}
	
	Surface = s;
}

void CFieldElement::UpdateLayout(CLimits const & l, bool apply)
{
	__super::UpdateLayout(l, apply);

	if(apply && IW > 0 && IH > 0)
	{
		if(Entity)
		{
			for(auto i : Entity->Items)
			{
				if(Find(i->Url) == null)
				{
					if(auto fie = AddItem(i))
					{
						fie->Transformation.Position = CFloat3::Nan;
					}
				}
			}
		}

		for(auto i : Items)
		{
			i->UpdateLayout(l, apply);

			if(i->Size.IsReal())
			{
				if(!i->Transformation.IsReal())
				{
					Placing(i);

					if(!i->Transformation.IsReal())
					{
						i->Transform(GetRandomFreePosition(i));
					}
				}
			}
		}
	}
}

void CFieldElement::MoveAvatar(CAvatar * a, CTransformation & p)
{
	auto fi = Items.Find([a](auto i){ return i->Avatar == a; });
	fi->Transform(p);
}
	
void CFieldElement::DeleteAvatar(CAvatar * n)
{
	auto fi = Items.Find([n](auto i){ return i->Avatar == n; });
	DeleteItem(fi);
}

CPositioning * CFieldElement::GetPositioning()
{
	return &Positioning;
}

CFieldItemElement * CFieldElement::AddItem(CFieldItem * fi)
{	
	auto avtype = Level->World->Complexity == AVATAR_ENVIRONMENT ? fi->Complexity : AVATAR_ICON2D;

	auto id = CGuid::Generate64(CFieldItemElement::GetClassName());
	auto fie = new CFieldItemElement(Level, this, CPath::Join(Directory, id + L".xon"));
	
	fie->Express(L"P", [this]{ return ItemPadding; });
	fie->Express(L"M", [this]{ return ItemMargin; });
	fie->SetItem(avtype, fi);

	if(avtype == AVATAR_ICON2D)
	{
		fie->SetMetrics(GetMetrics(avtype, IconTitleMode, CSize::Nan));
		fie->SetTitleMode(IconTitleMode);
	}
	else if(avtype == AVATAR_WIDGET)
	{
		fie->SetMetrics(GetMetrics(avtype, WidgetTitleMode, CSize::Nan));
		fie->SetTitleMode(WidgetTitleMode);
	}

	fie->Active->StateChanged += ThisHandler(OnItemStateModified);

	AddNode(fie);
	Items.Add(fie);

	Added(fie);

	fie->Free();
	return fie;
}

void CFieldElement::OnItemAdded(CFieldItem * fi)
{
	//auto fie = AddItem(fi);
	//fie->Transformation.Position = CFloat3(NAN, NAN, NAN);

	UpdateLayout();
}

void CFieldElement::RemoveItem(CFieldItemElement * fie)
{
	Removed(fie);

	fie->Active->StateChanged -= ThisHandler(OnItemStateModified);

	RemoveNode(fie);

	Items.Remove(fie);
}
	
void CFieldElement::DeleteItem(CFieldItemElement * fie)
{
	auto e = fie->Entity;

	fie->Delete();
	RemoveItem(fie);

	Entity->Remove(e);
}


void CFieldElement::Clear()
{
	while(auto i = Items.First())
	{
		DeleteItem(i);
	}
}

void CFieldElement::OnMouseFilter(CActive * r, CActive * s, CMouseArgs * arg)
{
	if(arg->Control == EMouseControl::RightButton)
	{
		if(!Selectings.Empty())
		{
			arg->StopPropagation = true;

			if(arg->Event == EGraphEvent::Click)
			{
				if(!Menu)
				{
					Menu = new CRectangleMenu(Level->World, Level->Style);
				}
				
				Menu->Section->Clear();

				auto arr =	[this](auto tm)
							{	
								for(auto i : Selectings)
								{
									i->SetMetrics(GetMetrics(i->Type, tm, i->Avatar->Size));
									i->SetTitleMode(tm); 
									i->UpdateLayout();
								}

								auto b = FindBounds(Selectings);
																
								switch(tm)
								{
									case ECardTitleMode::Bottom:
									case ECardTitleMode::Top:		Arrange(Selectings, EXAlign::Center);	break;
									case ECardTitleMode::Left:		Arrange(Selectings, EXAlign::Right);	break;
									case ECardTitleMode::Right:		Arrange(Selectings, EXAlign::Left);		break;
								}

								TransformItems(Selectings, CTransformation(b.X, b.Y, 0));
							};


				auto arrange = Menu->Section->AddSectionItem(L"Arrange");
				auto grid = arrange->GetSection()->AddSectionItem(L"Grid");
				grid->GetSection()->AddItem(L"Title Left")->Clicked		= [arr](auto, auto){ arr(ECardTitleMode::Left);		};
				grid->GetSection()->AddItem(L"Title Right")->Clicked	= [arr](auto, auto){ arr(ECardTitleMode::Right);	};
				grid->GetSection()->AddItem(L"Title Top")->Clicked		= [arr](auto, auto){ arr(ECardTitleMode::Top);		};
				grid->GetSection()->AddItem(L"Title Bottom")->Clicked	= [arr](auto, auto){ arr(ECardTitleMode::Bottom);	};

				auto settitle =	[this](auto tm)
								{	
									for(auto i : Selectings)
									{
										i->SetMetrics(GetMetrics(i->Type, tm, i->Avatar->Size));
										i->SetTitleMode(tm); 
										i->UpdateLayout();
									}
								};

				auto title = Menu->Section->AddSectionItem(L"Title");
				title->GetSection()->AddItem(L"No")->Clicked		= [settitle](auto, auto){ settitle(ECardTitleMode::No);		};
				title->GetSection()->AddItem(L"Left")->Clicked		= [settitle](auto, auto){ settitle(ECardTitleMode::Left);	};
				title->GetSection()->AddItem(L"Right")->Clicked		= [settitle](auto, auto){ settitle(ECardTitleMode::Right);	};
				title->GetSection()->AddItem(L"Top")->Clicked		= [settitle](auto, auto){ settitle(ECardTitleMode::Top);	};
				title->GetSection()->AddItem(L"Bottom")->Clicked	= [settitle](auto, auto){ settitle(ECardTitleMode::Bottom);	};

				if(Selectings.Count([](auto i){ return i->Type == AVATAR_ICON2D; }) == Selectings.Size()) // for icons only
				{
					auto setsize =	[this](auto s)
									{	
										for(auto i : Selectings)
										{
											i->SetMetrics(GetMetrics(i->Type, i->TitleMode, s));
											i->UpdateLayout();
										}
									};

					auto icon = Menu->Section->AddSectionItem(L"Icon");
					icon->GetSection()->AddItem(L"Default")->Clicked	= [setsize](auto, auto){ setsize(CSize::Nan);		};
					icon->GetSection()->AddItem(L"16x16")->Clicked		= [setsize](auto, auto){ setsize(CSize(16, 16, 0));	};
					icon->GetSection()->AddItem(L"24x24")->Clicked		= [setsize](auto, auto){ setsize(CSize(24, 24, 0));	};
					icon->GetSection()->AddItem(L"32x32")->Clicked		= [setsize](auto, auto){ setsize(CSize(32, 32, 0));	};
					icon->GetSection()->AddItem(L"48x48")->Clicked		= [setsize](auto, auto){ setsize(CSize(48, 48, 0));	};
					icon->GetSection()->AddItem(L"64x64")->Clicked		= [setsize](auto, auto){ setsize(CSize(64, 64, 0));	};
				}

				Menu->Section->AddSeparator();

				Menu->Section->AddItem(L"Delete")->Clicked =	[this](auto, auto)
																{
																	for(auto i : Selectings)
																	{
																		DeleteItem(i);
																	}
																};
		
				Menu->Open(arg->Pick);
			}
		}
	}
}

void CFieldElement::OnMouse(CActive * r, CActive * s, CMouseArgs * arg)
{
	if(arg->Control == EMouseControl::LeftButton)
	{
		if(arg->Action == EMouseAction::On)
		{
			if(s == Surface->Active)
			{
				Active->IsPropagator = false;

				for(auto fi : Selectings)
				{
					fi->Select(false);
				}
				Selectings.Clear();
			}
		}

		if(arg->Action == EMouseAction::Off)
		{
			FinishSelecting();
		}
		
		arg->StopPropagation = true;
	}

	if(arg->Control == EMouseControl::MiddleButton)
	{
		auto fi = FindItem(s);

		if(fi || InMoving)
		{
			arg->StopPropagation = true;
		}
	
		if(arg->Action == EMouseAction::Off) // see copy-paste below
		{
			if(InMoving)
			{
				FinishMovement(arg);
			}
		}
	}

	if(arg->Action == EMouseAction::Move)
	{
		auto fi = FindItem(s);

		if(arg->Capture && arg->Capture->InputAs<CMouseInput>())
		{
			auto ci = arg->Capture->InputAs<CMouseInput>();

			if(ci->Control == EMouseControl::LeftButton)
			{
				if(arg->Event == EGraphEvent::Captured)
				{
					if(s == Surface->Active)
					{
						StartSelecting(arg);
					}
				}
		
				if(InSelecting)
				{
					ProcessSelecting(arg->Capture, arg->Pick);
				}
			}
			
			if(ci->Control == EMouseControl::MiddleButton)
			{
				if(arg->Event == EGraphEvent::Captured)
				{
					if(fi)
					{
						StartMovement(s, arg, arg->Capture);
					}
				}
		
				if(!Selectings.Empty())
				{
					if(InMoving)
					{
						if(arg->Event == EGraphEvent::Hover || arg->Event == EGraphEvent::Roaming)
						{
							ProcessMovement(arg, arg->Pick);
						}
				
						if(arg->Event == EGraphEvent::Released)
						{
							FinishMovement(arg);
						}
					}
		
					arg->StopPropagation = true;
				}
			}
	
			if(ci->Control == EMouseControl::RightButton)
			{
				//if(a.Type == EMouseEventType::Captured && fi && fi->Avatar->AllowFieldOperation(s, a))
				//{
				//	//InDragging = true;
				//	//Grid->Active->IsPropagator = false;
				//}
					
				if(arg->Event == EGraphEvent::Leave && InMoving)
				{
					//CArray<CDragDropItem *> l;
					//	
					//for(auto i : SelectedItems)
					//{
					//	auto odd = new CObjectDragDrop();
					//	//odd->Object = i;
					//	odd->Url	= i->Avatar->ObjectUrl;
					//	odd->Owner	= this;
					//	i->Avatar->SaveInstance(&odd->Data);	
					//	odd->Data.Reset();
					//	l.push_back(odd);
					//}
					//					
					//auto fi = SelectedItems[0];
					//
					//auto c = Level->Nexus->Connect(this, fi->Avatar->ObjectUrl, UOS_PROTOCOL_WORLD_ICON);
					//auto dn = dynamic_cast<CIcon *>(c);
					//
					////auto dn = hs->CreateIcon(fi->Avatar->ObjectUrl);
					//SelectedItems[0]->Avatar->Copy(dn);
					//
					//Level->World->DoDragAndDrop(l, dn->Get<IDragNode>(), this);
					//
					//dn->Free();
					//
					//for(auto i : l)
					//{
					//	i->Free();
					//}
					//
					//InDragging = false;
					//
					//for(auto fi : SelectedItems)
					//{
					//	fi->Avatar->InMoving = false;
					//}
					//SelectedItems.clear();
					//
					//Level->Nexus->Disconnect(this, c, UOS_PROTOCOL_WORLD_ICON);
				}
			}
		}
	}	
}

void CFieldElement::OnTouch(CActive * r, CActive * s, CTouchArgs * arg)
{
	auto selected = Items.Where([](auto i){ return i->IsSelected(); });

	auto fi = FindItem(s);
	
	if(arg->Event == EGraphEvent::Captured)
	{
		if(arg->Picks.size() == 1)
		{
			if(fi)
			{
				StartMovement(s, arg, arg->Capture);
			} 
			else if(s == Surface->Active)
			{	
				StartSelecting(arg);
			}
		}
	}

	auto & pk = arg->Picks(arg->Input->Touch.Id);

	if(arg->Event == EGraphEvent::Hover || arg->Event == EGraphEvent::Roaming)
	{
		if(InSelecting)
			ProcessSelecting(arg->Capture, pk);

		if(InMoving)
			ProcessMovement(arg, pk);

		arg->StopPropagation = true;
	}
		
	if(arg->Event == EGraphEvent::Released)
	{
		if(InSelecting)
			FinishSelecting();

		if(InMoving)
			FinishMovement(arg);

		arg->StopPropagation = true;
	}

	if(arg->Input->Action == ETouchAction::Removed)
	{
		if(InMoving || InSelecting)
		{
			arg->StopPropagation = true;
		}
	}
}

void CFieldElement::StartMovement(CActive * s, CInputArgs * arg, CNodeCapture * c)
{
	auto fi = FindItem(s);

	if(fi)
	{
		if(!Selectings.Contains(fi))
		{
			fi->Select(true);
			Selectings.Add(fi);
		}
			
		for(auto i : Selectings)
		{
			auto s = i->Size;
		
			if(PositioningType == EFieldPositioningType::Stand)
				s.H = 0;
		
			i->Capture	= Positioning.Capture(c->Pick, s, i->Active->Transit(c->Pick.Active, c->Pick.Point));
		}
						
		Active->IsPropagator = false;
		InMoving = true;
		arg->StopPropagation = true;
	}
}

void CFieldElement::ProcessMovement(CInputArgs * arg, CPick & pk)
{
	if(InMoving)
	{
		if(GetUnit()->ActiveSpaces.Match(pk.Camera->Viewport).Space)
		{
			for(auto i : Selectings)
			{
				auto p = Positioning.Move(i->Capture, pk).Position;
								
				i->Transform(p.x, p.y);
			}
		}
	}
}

void CFieldElement::FinishMovement(CInputArgs * arg)
{
	if(InMoving)
	{
		for(auto fi : Selectings)
		{
			fi->TransformZ(ItemZ);
		}

		arg->StopPropagation = true;
		arg->StopRelatedPropagation = true;
	
		Active->IsPropagator = true;
		InMoving = false;
	}
	
	for(auto fi : Selectings)
	{
		fi->Select(false);
	}
	Selectings.Clear();
}

void CFieldElement::OnItemStateModified(CActive * s, CActive *, CActiveStateArgs * a)
{
}

CCardMetrics CFieldElement::GetMetrics(CString const & am, ECardTitleMode tm, const CSize & s)
{
	CCardMetrics m;

	if(am == AVATAR_ICON2D)
	{
		auto size = s ? s : IconSize;

		m.FaceSize		= size;
		m.FaceMargin	= {0};

		if(tm == ECardTitleMode::Left || tm == ECardTitleMode::Right)	m.TextSize	= CSize(size.W * 4, size.H, 0);
		if(tm == ECardTitleMode::Bottom || tm == ECardTitleMode::Top)	m.TextSize	= CSize(size.W * (Level->World->Tight ? 1 : 2), Level->Style->GetFont(L"Text/Font")->Height * 4, 0);
	}
	if(am == AVATAR_WIDGET)
	{
		m.FaceSize		= s ? s : CSize(IH/3, IH/3 * 0.75f, 0);
		m.FaceMargin	= {0};

		if(tm == ECardTitleMode::Left || tm == ECardTitleMode::Right)	m.TextSize	= CSize(IconSize.W * 4, NAN, 0);
		if(tm == ECardTitleMode::Bottom || tm == ECardTitleMode::Top)	m.TextSize	= CSize(NAN, IconSize.H * 2, 0);
	}

	return m;
}

CRect CFieldElement::FindBounds(CRefList<CFieldItemElement *> & items)
{
	CRect r = {FLT_MAX, FLT_MAX, 0, 0};

	float top = 0; 
	float right = 0;

	for(auto i : items)
	{
		r.X = min(r.X, i->Transformation.Position.x);
		r.Y = min(r.Y, i->Transformation.Position.y);

		right	= max(right, i->Transformation.Position.x + i->W);
		top		= max(top, i->Transformation.Position.y + i->H);
	}

	r.W = right - r.X;
	r.H = top - r.Y;

	return r;
}

CSize CFieldElement::Arrange(CRefList<CFieldItemElement *> & list, EXAlign xa)
{
	CSize o(0,0,0);
	CArray<CFieldItemElement *> items(list.begin(), list.end());

	auto n = uint(ceil(sqrt(items.size())));

	auto w = CArray<float>(n, 0.f);
	auto h = CArray<float>(n, 0.f);

	for(auto i = 0u; i < items.size(); i++)
	{
		auto r = i/n;
		auto c = i%n;

		w[c] = max(w[c], items[i]->W);
		h[r] = max(h[r], items[i]->H);
	}

	o.W = w.Sum();
	o.H = h.Sum();

	CFloat3 p = {0, o.H, 0};

	for(auto r = 0u; r < n; r++)
	{
		for(auto c = 0u; c < n; c++)
		{
			auto i = r * n + c;

			if(i < items.size())
			{
				items[i]->TransformY(p.y - items[i]->H);

				switch(xa)
				{
					case EXAlign::Left:		items[i]->TransformX(p.x); break;
					case EXAlign::Center:	items[i]->TransformX(p.x + (w[c] - items[i]->W)/2); break;
					case EXAlign::Right:	items[i]->TransformX(p.x + w[c] - items[i]->W); break;
				}
			}

			p.x += w[c];
		}
		p.x = 0;
		p.y -= h[r];
	}

	return o;
}

void CFieldElement::TransformItems(CRefList<CFieldItemElement *> & items, CTransformation & t)
{
	for(auto i : items)
	{
		auto tt = i->Transformation * t;
		tt.Position.z = ItemZ;
		i->Transform(tt);
	}

//	PropagateAreaChanges(this);
}

CTransformation CFieldElement::GetRandomPosition(CFieldItemElement * fie)
{
	CFloat3 np((Surface->W - fie->W) * float(rand())/RAND_MAX, (Surface->H  - fie->H) * float(rand())/RAND_MAX, ItemZ);
	
	auto t = ItemTransformation;

	t.Position = np;

	return t;
}

CTransformation CFieldElement::GetRandomFreePosition(CFieldItemElement * fie)
{
	int		tried = 0;
	bool	failed = true;
	CTransformation t;

	do
	{
		t = GetRandomPosition(fie);

		CRect r(t.Position.x, t.Position.y, fie->W, fie->H);

		failed = false;

		for(auto i : Items)
		{
			if(i != fie)
			{
				if(failed = CRect(i->Transformation.Position.x, i->Transformation.Position.y, i->W, i->H).Intersects(r))
				{
					tried++;
					break;
				}
			}
		}
	} while(tried < 100 && failed);

	return t;
}


CRefList<CFieldItemElement *> CFieldElement::Find(CList<CUol> & items)
{
	CRefList<CFieldItemElement *> a;

	for(auto i : items)
	{
		auto fie = Items.Find([i](auto j){ return j->Entity->Url == i; });

		if(fie)
		{
			a.Add(fie);
		}
	}

	return a;
}

CFieldItemElement * CFieldElement::Find(CUol & o)
{
	return Items.Find([o](auto i){ return i->Entity->Url == o; });
}

bool CFieldElement::Test(CArray<CDragItem> & d, CAvatar * n)
{
	return d.Has(	[this](auto & i)
					{
						if(CUol::IsValid(i.Object) && i.Owner != Entity->Url)
						{
							auto p = Level->World->FindAvatarSystem(CUol(i.Object), DefaultAvatarType);

							if(p)
							{
								Level->Nexus->Disconnect(p);
								return true;
							}
						}

						return false; 
					});
}

CModel * CFieldElement::Enter(CArray<CDragItem> & d, CAvatar * a)
{
	///a->As<IFaceTitleAvatar>()->SetAppearance(IconTitleMode, GetMetrics(AVATAR_ICON2D, IconTitleMode, CSize::Nan));
	a->UpdateLayout(CLimits(Size, Size), true);
	return null;
}

void CFieldElement::Leave(CArray<CDragItem> & d, CAvatar *)
{
}

void CFieldElement::Drop(CArray<CDragItem> & d, CPick & p)
{
	for(auto i : d)
	{
		//auto dd = dynamic_cast<CObjectDragDrop *>(i);
		//
		//auto c = Level->Nexus->Connect(this, dd->Url, UOS_PROTOCOL_WORLD_ICON);
		//auto av = dynamic_cast<CIcon *>(c);
		//
		////auto av = ws->CreateIcon(dd->Url);
		//
		//if(dd->Data.GetSize() > 0)
		//{
		//	av->LoadInstance(&dd->Data);
		//}
		//
		////av->SetName(CGuid::Generate(av->GetInstanceName()));
		//av->ObjectUrl = dd->Url;
		//av->SetAppearance(DefaultTitleMode, GetMetrics(UOS_AVATAR_ICON2D, DefaultTitleMode));
		//av->Get<CWorldNode>()->UpdateArea(Area, true);
		//							
		//auto p = Active->GetXyPlaneIntersectionPoint(a.Movement.Viewport, a.Movement.Position);
		//
		//auto fi = AddItem(av, CFloat3(p.x - av->Get<CWorldNode>()->Area.W, p.y, 0));
		//				
		//av->Free();
		//Level->Nexus->Disconnect(this, c);

		Entity->Added -= ThisHandler(OnItemAdded);

		for(auto i : d)
		{
			auto fi = Entity->Add(CUol(i.Object), AVATAR_ICON2D);

		 	auto fie = AddItem(fi);

			fie->UpdateLayout(CLimits(CSize(IW, IH, 0), CSize(IW, IH, 0)), true);

			CTransformation t = ItemTransformation;

			auto r = p.Camera->Raycast(p.Vpp); //scan ray of new pos
			t.Position = CPlane(0, 0, -1).Intersect(r.Transform(Active->FinalMatrix.GetInversed()));// new pos in Field
			t.Position.z = ItemZ;

			t.Position.x -= fie->W;

			fie->Transform(t);

		}

		Entity->Added += ThisHandler(OnItemAdded);

		//UpdateContentLayout(CSize(IW, IH, 0), true);
	}
	//Grid->Active->IsPropagator = true;
}

CFieldItemElement * CFieldElement::FindItem(CActive * a)
{
	auto fi = a->AncestorOwnerOf<CFieldItemElement>();
	return fi && fi->Parent == this ? fi : null;
}

void CFieldElement::AddTitleMenu(IMenuSection * ms, CAvatar * a)
{
	auto fi = FindItem(a->Active);

	auto title = ms->AddSectionItem(L"Title");
	title->GetSection()->AddItem(L"No")->Clicked		= [this, a, fi](auto, auto){ fi->SetMetrics(GetMetrics(fi->Type, ECardTitleMode::No,	a->Size));	fi->SetTitleMode(ECardTitleMode::No);		fi->UpdateLayout();	};
	title->GetSection()->AddItem(L"Left")->Clicked		= [this, a, fi](auto, auto){ fi->SetMetrics(GetMetrics(fi->Type, ECardTitleMode::Left,	a->Size));	fi->SetTitleMode(ECardTitleMode::Left);		fi->UpdateLayout();	};
	title->GetSection()->AddItem(L"Right")->Clicked		= [this, a, fi](auto, auto){ fi->SetMetrics(GetMetrics(fi->Type, ECardTitleMode::Right,	a->Size));	fi->SetTitleMode(ECardTitleMode::Right);	fi->UpdateLayout();	};
	title->GetSection()->AddItem(L"Top")->Clicked		= [this, a, fi](auto, auto){ fi->SetMetrics(GetMetrics(fi->Type, ECardTitleMode::Top,	a->Size));	fi->SetTitleMode(ECardTitleMode::Top);		fi->UpdateLayout();	};
	title->GetSection()->AddItem(L"Bottom")->Clicked	= [this, a, fi](auto, auto){ fi->SetMetrics(GetMetrics(fi->Type, ECardTitleMode::Bottom,a->Size));	fi->SetTitleMode(ECardTitleMode::Bottom);	fi->UpdateLayout();	};
}

void CFieldElement::AddIconMenu(IMenuSection * ms, CAvatar * a)
{
	auto fi = FindItem(a->Active);

	auto icon = ms->AddSectionItem(L"Icon");
	icon->GetSection()->AddItem(L"Default")->Clicked	= [this, a, fi](auto, auto){ fi->SetMetrics(GetMetrics(fi->Type, fi->TitleMode, CSize::Nan));	fi->UpdateLayout();	};
	icon->GetSection()->AddItem(L"16x16")->Clicked		= [this, a, fi](auto, auto){ fi->SetMetrics(GetMetrics(fi->Type, fi->TitleMode, {16, 16, 0}));	fi->UpdateLayout();	};
	icon->GetSection()->AddItem(L"24x24")->Clicked		= [this, a, fi](auto, auto){ fi->SetMetrics(GetMetrics(fi->Type, fi->TitleMode, {24, 24, 0}));	fi->UpdateLayout();	};
	icon->GetSection()->AddItem(L"32x32")->Clicked		= [this, a, fi](auto, auto){ fi->SetMetrics(GetMetrics(fi->Type, fi->TitleMode, {32, 32, 0}));	fi->UpdateLayout();	};
	icon->GetSection()->AddItem(L"48x48")->Clicked		= [this, a, fi](auto, auto){ fi->SetMetrics(GetMetrics(fi->Type, fi->TitleMode, {48, 48, 0}));	fi->UpdateLayout();	};
	icon->GetSection()->AddItem(L"64x64")->Clicked		= [this, a, fi](auto, auto){ fi->SetMetrics(GetMetrics(fi->Type, fi->TitleMode, {64, 64, 0}));	fi->UpdateLayout();	};
}

void CFieldElement::AddNewMenu(CRectangleMenu * menu, CFloat3 & p)
{
	auto nmi = new CRectangleSectionMenuItem(Level->World, Level->Style, L"New");
	menu->Section->AddItem(nmi);
			
	auto cc = Level->Nexus->ConnectMany<IShellFriend>(this, SHELL_FRIEND_PROTOCOL);
	for(auto f : cc)
	{
		auto smi = new CRectangleSectionMenuItem(Level->World, Level->Style, f->GetTitle());
		auto sms = f->CreateNewMenu(this, p, menu);

		smi->SetSection(sms);
		nmi->Section->AddItem(smi);

		sms->Free();
		smi->Free();
	}
				
	Level->Nexus->Disconnect(cc);
	nmi->Free();

}

void CFieldElement::StartSelecting(CInputArgs * arg)
{
	InSelecting = true;
	arg->StopPropagation = true;
}

void CFieldElement::ProcessSelecting(CNodeCapture * c, CPick & pk)
{
	if(!Selection->Parent)
	{
		Visual->AddNode(Selection);
	}

	auto p = Surface->Active->Transit(c->Pick.Active, c->Pick.Point);
	auto q = Surface->Active->Transit(pk.Active, pk.Point);

	auto x = min(p.x, q.x);
	auto y = min(p.y, q.y);
	auto w = fabs(p.x - q.x);
	auto h = fabs(p.y - q.y);

	SelectionMesh->Generate(x, y, 0, w-2, h-2, 1, 1, 1, 1);

	for(auto i : Items)
	{
		auto bb = i->Visual->GetAABB();

		auto min = Surface->Active->Transit(i->Active, bb.Min);
		auto max = Surface->Active->Transit(i->Active, bb.Max);

		if(CRect(min.x, min.y, max.x - min.x, max.y - min.y).Intersects(CRect(x, y, w, h)))
		{
			if(!Selectings.Contains(i))
			{
				i->Select(true);
				Selectings.Add(i);
			}
		}
		else
		{
			if(Selectings.Contains(i))
			{
				i->Select(false);
				Selectings.Remove(i);
			}
		}
	}
}

void CFieldElement::FinishSelecting()
{
	if(InSelecting)
	{
		InSelecting = false;

		if(Selection->Parent)
			Visual->RemoveNode(Selection);
	}
	
	Active->IsPropagator = true;
}