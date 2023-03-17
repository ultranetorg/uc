#include "StdAfx.h"
#include "ActiveGraph.h"
#include "ScreenViewport.h"

using namespace uc;

CActiveGraph::CActiveGraph(CEngineLevel * l, const CString & name) : CActiveSpace(L"Root")
{
	Level	= l;
	Name	= name;

	SetGraph(this);

	Root = new CActive(l, L"Root");
	Root->Graph = this;
	Root->FinalMatrix = CMatrix::Identity;

	Diagnostic = Level->Core->Supervisor->CreateDiagnostics(Name + L" - ActiveGraph");
	Diagnostic->Updating += ThisHandler(OnDiagnosticsUpdate);

	DiagGrid.AddColumn(L"Name");
	DiagGrid.AddColumn(L"Refs");
	DiagGrid.AddColumn(L"Status");
	DiagGrid.AddColumn(L"S/F/P/L");
#ifdef _DEBUG
	DiagGrid.AddColumn(L"Position");
	DiagGrid.AddColumn(L"Rotation");
	DiagGrid.AddColumn(L"Scale");
#endif
	DiagGrid.AddColumn(L"Mesh BB");
}

CActiveGraph::~CActiveGraph()
{
	if(HoverFocus)
		HoverFocus->Free();

	if(Focus)
		Focus->Free();

	Root->Free();

	Diagnostic->Updating -= ThisHandler(OnDiagnosticsUpdate);
}
	
void CActiveGraph::OnDiagnosticsUpdate(CDiagnosticUpdate & u)
{
	DiagGrid.Clear();

#ifdef _DEBUG
	for(auto & s : _Intersections)
	{
		for(auto i : s.second)
		{
			if(i.Active)
				Diagnostic->Add(L"Viewport: %p   Space: %10s   D: %15f   Z: %.15f   P: [%s]   %-100s", i.Camera->Viewport, s.first->Name, i.Distance, i.Z, i.Point.ToNiceString(), i.Active->FullName);
			else
				Diagnostic->Add(L"Viewport: %p   Space: %10s", i.Camera->Viewport, s.first->Name);
		}
	}
	Diagnostic->Add(L"");
#endif	

	std::function<void(CActiveSpace *, CActive *, const CString &)> 
	
	adump =	[this, &adump, &u](auto s, auto n, auto & tab)
			{
				if(Diagnostic->ShouldProceed(u, DiagGrid.GetSize()))
				{
					auto & r = DiagGrid.AddRow();

					if(Diagnostic->ShouldFill(u, DiagGrid.GetSize()))
					{
						r.SetNext(L"%p %s%s", s, tab, n->Name);
						r.SetNext(CInt32::ToString(n->GetRefs()));
						r.SetNext(n->GetStatus());
						r.SetNext(	CString(1, ToString(n->State)[0]) + 
									(Focus && n == Focus ? L" AF" : L"   ") + 
									(HoverFocus && n == HoverFocus ? L" HF" : L"   ") + 
									(n->IsPropagator ? L" P" : L"  ")/* + 
									(n->IsListener ? L" L" : L"  ")*/);
						#ifdef _DEBUG
						r.SetNext(n->_Decomposed.Position.ToNiceString());
						r.SetNext(n->_Decomposed.Rotation.ToNiceString());
						r.SetNext(n->_Decomposed.Scale.ToNiceString());
						#endif																				
						r.SetNext(n->Mesh ? CFloat::NiceFormat(n->Mesh->BBox.GetWidth()) + L" " + CFloat::NiceFormat(n->Mesh->BBox.GetHeight()) + L" " + CFloat::NiceFormat(n->Mesh->BBox.GetDepth()) : L"");
					}

					for(auto i : n->Nodes)
					{
						adump(s, i, tab + L"  ");
					}
				}
			};

	std::function<void(CActiveSpace *, const CString &)> sdump =	[this, &sdump, &adump, &u](auto s, auto & tab) 
																	{
																		if(Diagnostic->ShouldFill(u, DiagGrid.GetSize()))
																		{
																			auto & r = DiagGrid.AddRow();
																			r.SetNext(RGB(0, 204, 204), L"%p %s%s", s, tab, s->Name);
																		}

																		for(auto v : s->Actives)
																			//if(!v->Parent)
																				adump(s, v, tab + L"  "); 
		
																		for(auto i : s->Spaces)
																			sdump(i, tab + L"  ");
																	};

	
	sdump(this, L"");

	Diagnostic->Add(u, DiagGrid);
	Diagnostic->Add(L"");
}

void CActiveGraph::OnNodeDetach(CActive * n)
{
	if(Focus && Focus->HasAncestor(n))
	{
		Activate(n->Parent);
	}
}

CActive * CActiveGraph::Activate(CActive * f)
{
	auto args = new CManualArgs();
	auto root = new CEventNode(f, args);

	auto n = Activate(root, f, null);

	CallEvent(root);

	delete root;
	args->Free();

	return n;
}

CActive * CActiveGraph::Activate(CEventNode * pevent, CActive * f, CInputMessage * im)
{
	if(Focus != f)
	{
		if(f)
		{
			f = f->GetElderEnabled();
		}

		while(f && f->Enabled && f->ActivatePassOn && !f->Nodes.Empty()) // find descedant with index=0
		{
			f = f->Nodes.Min([](auto a, auto b){ return a->Index < b->Index; });
		}
					
		if(Focus)
		{
			auto ca = f->FindCommonAncestor(Focus);

			auto a  = new CActiveStateArgs();
			a->Old	= EActiveState::Active;
			a->New	= EActiveState::Normal;

			auto n = Focus;
				
			while(n != ca)
			{
				n->State = EActiveState::Normal;
				Propagate(pevent->AddRelated(n, a), n, EGraphEvent::State);
				n = n->Parent;
			}

			a->Free();
		}
		
		if(f)
		{
			auto ca = f->FindCommonAncestor(Focus);

			auto a  = new CActiveStateArgs();
			a->Old	= EActiveState::Normal;
			a->New	= EActiveState::Active;

			auto n = ca ? ca->FindChildContaining(f) : Root;
			
			while(n && n->Parent != f)
			{
				n->State = EActiveState::Active;
				Propagate(pevent->AddRelated(n, a), n, EGraphEvent::State);
				n = n->FindChildContaining(f);
			}
			
			Focus = sh_assign(Focus, f);

			a->Free();
		}
		else
			sh_free(Focus);
	}
	return Focus;
}

CEventNode * CActiveGraph::Fire(CPick & pk, CInputMessage * m, EGraphEvent type, CActive * n)
{
	CInputArgs * args = null;

	if(m->Class == EInputClass::Mouse)
	{
		auto a = new CMouseArgs(m);
		a->Pick		= pk;
		a->Event	= type;
	
		if(Capture)
		{
			a->Capture = Capture;
			a->Capture->Take();
		}

		args = a;
	}
	if(m->Class == EInputClass::TouchScreen)
	{
		auto a = new CTouchArgs(m);
		a->Picks	= Picks;
		a->Event	= type;
	
		if(Capture)
		{
			a->Capture = Capture;
			a->Capture->Take();
		}

		args = a;

		#ifdef _DEBUG
		//Level->Log->ReportDebug(this, L"%-10s %-10s %10g %10g %s", ToString(a->Input->Action), ToString(type), a->Input->Touch.Position.x, a->Input->Touch.Position.y, n->FullName);
		#endif
	}
	if(m->Class == EInputClass::Keyboard)
	{
		args = new CKeyboardArgs(m);
	}

	auto e = new CEventNode(n, args);
	Propagate(e, n, type);
	args->Free();

	return e;
}

void CActiveGraph::FireRelated(CEventNode * pevent, CPick & pk, CInputMessage * m, EGraphEvent type, CActive * n)
{
	CInputArgs * args = null;
	
	if(m->Class == EInputClass::Mouse)
	{
		auto a = new CMouseArgs(m);
		a->Pick		= pk;
		a->Event	= type;
	
		if(Capture)
		{
			a->Capture = Capture;
			a->Capture->Take();
		}

		args = a;
	}
	if(m->Class == EInputClass::TouchScreen)
	{
		auto a = new CTouchArgs(m);
		a->Picks	= Picks;
		a->Event	= type;
	
		if(Capture)
		{
			a->Capture = Capture;
			a->Capture->Take();
		}

		args = a;

		#ifdef _DEBUG
		//Level->Log->ReportDebug(this, L"%-10s %-10s %10g %10g %s", ToString(a->Input->Action), ToString(type), a->Input->Touch.Position.x, a->Input->Touch.Position.y, n->FullName);
		#endif

	}
	if(m->Class == EInputClass::Keyboard)
	{
		args = new CKeyboardArgs(m);
	}

	Propagate(pevent->AddRelated(n, args), n, type);
	args->Free();
}
		
void CActiveGraph::MovePrimary(CEventNode * pevent, CPick & pk, CInputMessage * m)
{
	if(pk == LastPick)
		return;

	CInputArgs * args = null;

	if(HoverFocus && HoverFocus != pk.Active) // выхожим из одного, входим в другой
	{
		auto ca = HoverFocus->FindCommonAncestor(pk.Active);
		auto n = HoverFocus;
	
		while(n && n != ca)
		{
			FireRelated(pevent, pk, m, EGraphEvent::Leave, n);
			n = n->Parent;
		}
	
		HoverFocus = sh_assign(HoverFocus, ca);
		//HoverFocus = ca;
	}

	if(pk.Active && HoverFocus != pk.Active)
	{
		auto ca = HoverFocus ? HoverFocus->FindCommonAncestor(pk.Active) : null;
		auto n = ca ? ca->FindChildContaining(pk.Active) : Root;
	
		while(n)
		{
			//Level->Log->ReportMessage(this, L"CursorMoved (Enter) : %s", n->FullName.c_str());
			FireRelated(pevent, pk, m, EGraphEvent::Enter, n);
	
			n = n->FindChildContaining(pk.Active);
		}
		
		HoverFocus = sh_assign(HoverFocus, pk.Active);
	}
			
	if(HoverFocus)
	{
		//Level->Log->ReportMessage(this, L"CursorMoved (Hover) : %s", HoverFocus->FullName.c_str());
		auto t = EGraphEvent::Hover;
	
		if(Capture && Capture->Status == 2)
		{
			if(pk.Active != Capture->Pick.Active)
			{
				t = EGraphEvent::GuestHover;
			}
		}

		FireRelated(pevent, pk, m, t, HoverFocus);
	}
		
	if(Focus && Capture)
	{
		auto d = (Capture->Pick.Vpp - pk.Vpp).GetLength();

		if(Capture->Status == 1 && (m->Class == EInputClass::Mouse && d > 3 || 
									m->Class == EInputClass::TouchScreen && d > 20))
		{
			//Level->Log->ReportMessage(this, L"CursorMoved (Captured) : %s", Focus->FullName.c_str());
			Capture->Status = 2;
			FireRelated(pevent, pk, m, EGraphEvent::Captured, Focus);
		}

		if(HoverFocus != Focus)
		{
			//Level->Log->ReportMessage(this, L"CursorMoved (Outside) : %s", HoverFocus ?  HoverFocus->FullName.c_str() : L"");
			FireRelated(pevent, pk, m, EGraphEvent::Roaming, Focus);
		}
	}

	LastPick = pk;
}

void CActiveGraph::ProcessMouse(CMouseInput * m, CPick & pk)
{
	auto root = Fire(pk, m, EGraphEvent::Input, pk.Active);

	auto d = m->Device;

	if(m->Action == EMouseAction::On)
	{
		On(root, pk, m);
	}

	if(m->Action == EMouseAction::Move)
	{
		MovePrimary(root, pk, m);
	}

	if(m->Action == EMouseAction::Rotation)
	{
	}

	if(m->Action == EMouseAction::Off)
	{
		Off(root, pk, m);
	}

	auto cf = Capture ? Focus : HoverFocus;
	
	if(cf)
	{
		d->ResetImage();
		FireRelated(root, pk, m, EGraphEvent::Feedback, cf);
	}

	CallEvent(root);

	d->ApplyCursor();
	
	delete root;
}

void CActiveGraph::ProcessTouch(CTouchInput * m, CMap<int, CPick> & pks)
{
	Picks = pks;

	auto & pk = pks(m->Touch.Id);

	auto root = Fire(pk, m, EGraphEvent::Input, pk.Active);;

	if(m->Touch.Primary)
	{
		if(m->Action == ETouchAction::Added)
		{
			On(root, pk, m);
		}

		if(m->Action == ETouchAction::Movement)
		{
			MovePrimary(root, pk, m);
		}

		if(m->Action == ETouchAction::Removed)
		{
			Off(root, pk, m);
		}
	}

	CallEvent(root);
	
	delete root;
}

void CActiveGraph::ProcessKeyboard(CKeyboardInput * m)
{
	if(Focus)
	{
		auto root = Fire(CPick(), m, EGraphEvent::Input, Focus);
		CallEvent(root);
		delete root;
	}
}

void CActiveGraph::On(CEventNode * pevent, CPick & pk, CInputMessage * m)
{
	if(Activate(pevent, pk.Active, m) != null)
	{
		Capture = new CNodeCapture(m);
		Capture->Status		= 1;
		Capture->Pick		= pk;
	}
}

void CActiveGraph::Off(CEventNode * pevent, CPick & pk, CInputMessage * m)
{
	if(pk.Active && Focus)
	{
		auto ca = Focus->FindCommonAncestor(pk.Active);

		if(ca)
		{
			FireRelated(pevent, pk, m, EGraphEvent::Click, ca);
		}
	}

	if(Capture)
	{
		if(Capture->Status == 2)
		{
			FireRelated(pevent, pk, m, EGraphEvent::Released, Focus);
		}

		//Capture.Pick.Active->Free();
		//Capture.Pick.Active = null;
		//Capture.Status = 0;
		sh_free(Capture);
	}
}

void CActiveGraph::CallEvent(CEventNode * e)
{
	//#ifdef _DEBUG
	//	auto mia = 	dynamic_cast<CMouseArgs *>(e->Args);
	//		if(e->Type == EActiveEvent::Move && mia && mia->Action == EInputAction::Rotation)
	//		{
	//			Level->Log->ReportDebug(this, L"%s   %4d     %-50s     %s", filtering ? L"F" : L" ", dynamic_cast<CMouseArgs *>(e->Args)->Action,  e->Receiver->FullName,  e->Source->FullName);
	//		}
	//#endif

	#ifdef _DEBUG
	CString t;
	if(e->Args->As<CMouseArgs>())	t = L"Mouse"; else
	if(e->Args->As<CTouchArgs>())	t = L"Touch";

	Level->Log->ReportDebug(this, L"%-10s   %-15s   %s", t, e->Args->ToString(), e->Source->FullName);
	#endif



	for(auto r : e->Receivers)
	{
		if(!r->HasAncestor(Root))
			continue;
	
		auto l = (r == e->Source ? EListen::Primary : EListen::PrimaryRecursive);

		if(auto arg = e->Args->As<CMouseArgs>())	r->MouseEvent	(l, r, e->Source, arg); else
		if(auto arg = e->Args->As<CKeyboardArgs>()) r->KeyboardEvent(l, r, e->Source, arg); else
		if(auto arg = e->Args->As<CTouchArgs>())	r->TouchEvent	(l, r, e->Source, arg);
	
		if(e->Args->StopPropagation)
			break;
	}
	
	if(!e->Args->StopPropagation)
	{
		for(auto i = e->Receivers.rbegin(); i != e->Receivers.rend(); i++)
		{
			auto r = *i;
	
			if(!r->HasAncestor(Root))
				continue;
	
			auto l = (r == e->Source ? EListen::Normal : EListen::NormalRecursive);

			if(auto arg = e->Args->As<CActiveStateArgs>())	r->StateChanged	(r, e->Source, arg);	else
			if(auto arg = e->Args->As<CMouseArgs>())		r->MouseEvent	(l, r, e->Source, arg); else
			if(auto arg = e->Args->As<CKeyboardArgs>())		r->KeyboardEvent(l, r, e->Source, arg); else
			if(auto arg = e->Args->As<CTouchArgs>())		r->TouchEvent	(l, r, e->Source, arg);
	
			if(e->Args->StopPropagation)
				break;
		}
	}

	if(!e->Args->StopRelatedPropagation)
	{
		for(auto i : e->Related)
		{
			CallEvent(i);
		}
	}
}

void CActiveGraph::Propagate(CEventNode * e, CActive * s, EGraphEvent type) // propagate down
{
	auto c = Root;

	while(c)
	{
		if((e->Args->As<CMouseArgs>()	&& c->MouseEvent.	Contains(EListen::NormalRecursive|EListen::PrimaryRecursive))  ||
		   (e->Args->As<CTouchArgs>()	&& c->TouchEvent.	Contains(EListen::NormalRecursive|EListen::PrimaryRecursive))  ||
		   (e->Args->As<CKeyboardArgs>()&& c->KeyboardEvent.Contains(EListen::NormalRecursive|EListen::PrimaryRecursive))  ||
		   c == s)
		{
			e->Receivers.AddBack(c);
		}

		if(type == EGraphEvent::Enter)
		{
			if(c->Parent)
			{
				c->Parent->HoverChild = c;
			}
		}
		if(type == EGraphEvent::Leave)
		{
			if(c->Parent)
			{
				c->Parent->HoverChild = null;
			}
		}

		if(c->IsPropagator)
			c = c->FindChildContaining(s);
		else
			break;
	}
}

CPick CActiveGraph::ReversePick(CCamera * c, CActiveSpace * s, CActive * a, CFloat3 & p)
{
	CPick pk;

	pk.Vpp = c->ProjectVertexXY(p.VertexTransform(a->FinalMatrix * s->Matrix));
	pk.Active = a; a->Take();
	pk.Space = s; s->Take();
	pk.Camera = c;
	pk.Ray = c->Raycast(pk.Vpp);
	pk.Point = p;

	return pk;
}
	
void CActiveGraph::Pick(CScreen * sc, CFloat2 & sp, CActiveSpace * s, CPick * cpk, CPick * npk)
{
	for(auto i : s->Actives)
	{
		Pick(sc, sp, s, i, i, null, CFloat2(NAN, NAN), CRay(), cpk, npk);

		if(npk->Active)
			return;
	}
}

void CActiveGraph::Pick(CScreen * sc, CFloat2 & sp, CActiveSpace * s, CActive * root, CActive * a, CCamera * cam, CFloat2 & vpp, CRay & ray, CPick * cis, CPick * nis)
{
	if(!a->Spaces.empty() && !a->Spaces.Contains(s))
	{
		return;
	}

	if(a->Enabled)
	{
		if(s->View)
		{
			for(auto c : s->View->Cameras)
			{
				if(auto svp = c->Viewport->As<CScreenViewport>())
				{
					if(svp->Target->Screen != sc)
					{
						continue;
					}

					if(svp->Contains(sp))
					{
						auto p = svp->ScreenToViewport(sp); // separate var important! do not use cis members
						auto r = c->Raycast(p); // separate var important! do not use cis members

						//cis->Space	= s; s->Take();
						cis->ScreenPoint	= sp;
						cis->Camera			= c;
						cis->Vpp			= p;
						cis->Ray			= r;
										
						Pick(s, a, sp, c, p, r, nis);
					
						for(auto i : a->Nodes)
						{
							Pick(sc, sp, s, root, i, c, p, r, cis, nis);
						}

						if(nis->Active)
							return;

					}
				}
			}
		} 
		else
		{
			Pick(s, a, sp, cam, vpp, ray, nis);

			for(auto i : a->Nodes)
			{
				Pick(sc, sp, s, root, i, cam, vpp, ray, cis, nis);
			}

			if(nis->Active)
				return;
		}
	}
}

void CActiveGraph::Pick(CActiveSpace * s, CActive * n, CFloat2 & sp, CCamera * c,  CFloat2 & vpp, CRay & r, CPick * nis)
{
	if(n->Enabled && n->IsReady())
	{
		auto is = n->Mesh->Intersect(r.Transform(!(n->FinalMatrix * s->Matrix)));
				
		if(is.StartIndex != -1)
		{
			auto cl = n->GetActualClipper();

			while(cl)
			{
				if(cl->ClippingMesh->Intersect(r.Transform(!(cl->FinalMatrix * s->Matrix))).StartIndex == -1)
				{
					is.StartIndex = -1;
					break;
				}
				cl = cl->GetActualClipper();
			}
		}
		
		if(is.StartIndex != -1)
		{
			auto z = c->ProjectVertex((n->FinalMatrix * s->Matrix).TransformCoord(is.Point /*CPick::GetPosition(n, is)*/)).z;

			//if(z < nis->Z)
			if(is.Distance < nis->Distance)
			{
				nis->Camera			= c;
				nis->Vpp			= vpp;
				nis->Ray			= r;
				nis->ScreenPoint	= sp;
	
				nis->Space		= sh_assign(nis->Space, s);
				nis->Active		= sh_assign(nis->Active, n);
				nis->Mesh		= sh_assign(nis->Mesh, n->Mesh);
				nis->StartIndex	= is.StartIndex;
				nis->Point		= is.Point;
				nis->Distance	= is.Distance;
				nis->Z			= z;
			}

			#ifdef _DEBUG
				CPick _is;
				_is.Camera		= c;
				_is.Vpp			= vpp;
				_is.Ray			= r;

				_is.Space		= sh_assign(_is.Space, s);
				_is.Active		= sh_assign(_is.Active, n);
				_is.Mesh		= sh_assign(_is.Mesh, n->Mesh);
				_is.StartIndex	= is.StartIndex;
				_is.Point		= is.Point;
				_is.Distance	= is.Distance;
				_is.Z			= z;

				_Intersections[s].push_back(_is);
			#endif		
		}
	}
}

