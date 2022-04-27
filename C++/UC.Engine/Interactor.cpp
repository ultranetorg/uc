#include "StdAfx.h"
#include "Interactor.h"

using namespace uc;

CInteractor::CInteractor(CEngineLevel * l, CScreenEngine * ve, CInputSystem * ie) : CEngineEntity(l), Pc(L"Interactor")
{
	Level = new CEngineLevel(*l);
	Level->Log	= Level->Core->Supervisor->CreateLog(L"Interactor");

	ViewEngine	= ve;
	InputEngine	= ie;

	InputEngine->Recieved += ThisHandler(Process);
		
	Diagnostics = Level->Core->Supervisor->CreateDiagnostics(L"Interactor");
	Diagnostics->Updating	+= ThisHandler(OnDiagnosticsUpdate);

	Level->Core->AddPerformanceCounter(&Pc);
}
	
CInteractor::~CInteractor()
{
	InputEngine->Recieved -= ThisHandler(Process);

	Level->Core->RemovePerformanceCounter(&Pc);
	delete Level;

	Diagnostics->Updating -= ThisHandler(OnDiagnosticsUpdate);
}

void CInteractor::OnDiagnosticsUpdate(CDiagnosticUpdate & a)
{
	Diagnostics->Add(L"Update: %f", Pc.GetTime());
}

CActiveGraph * CInteractor::CreateActiveGraph(const CString & name)
{
	return new CActiveGraph(Level, name);
}

CActiveLayer * CInteractor::AddLayer(CScreenViewport * screen, CActiveSpace * space)
{
	auto rt = new CActiveLayer(Level);
	rt->Viewport = screen;
	rt->Space = space;
	Layers.push_back(rt);
	return rt;
}

void CInteractor::RemoveLayer(CActiveLayer * t)
{
	delete t;
	Layers.Remove(t);
}

void CInteractor::Pick(CScreen * sc, CFloat2 & sp, CActiveSpace * s, CPick * cis, CPick * nis)
{
	CPick c, n;
	
	for(auto i : s->Spaces)
	{
		Pick(sc, sp, i, cis, nis);

		if(nis->Active)
		{
			return;
		}
	}

	s->Graph->Pick(sc, sp, s, &c, &n);
	
	if(!cis->Camera && c.Camera)
	{
		*cis = c;
	}
	if(n.Active)
	{
		*nis = n; 
		return;
	}
}

void CInteractor::Pick(CScreen * s, CFloat2 & p, CPick * pick)
{
	CPick cpick, npic;

	#ifdef _DEBUG
	for(auto i : Layers)
	{
		i->Space->Graph->_Intersections.clear();
	}
	#endif
		
	for(auto i = Layers.rbegin(); i != Layers.rend(); i++)
	{
		if((*i)->Viewport->Target->Screen == s)
		{
			Pick(s, p, (*i)->Space, &cpick, &npic);
	
			*pick = npic.Active ? npic : cpick;
							
			if(pick->Active)
			{
				break;;
			}
		}
	}

}

void CInteractor::Process(CInputMessage * m)
{
	CPick pick;

	Pc.BeginMeasure();

	if(m->Class == EInputClass::Mouse && !BlockMouse)
	{
		auto mi = m->As<CMouseInput>();

		Pick(mi->Screen, mi->Position, &pick);
	
		if(pick.Active)
		{
			auto g = pick.Active->Graph;
			
			g->ProcessMouse(mi, pick);

			if(g->Focus)
			{
				FocusGraph = g;
			}
		}
	}

	if(m->Class == EInputClass::TouchScreen)
	{
		auto ti = m->As<CTouchInput>();

		if(ti->Touch.Primary) // hask to prevent mouse messages during touching
		{
			if(ti->Action == ETouchAction::Added)
				BlockMouse = true;

			if(ti->Action == ETouchAction::Removed)
				BlockMouse = false;
		}

		CMap<int, CPick> pks;

		for(auto & i : ti->Touches)
		{
			Pick(ti->Screen, i.Position, &pks[i.Id]);
		}

		if(pks(ti->Touch.Id).Active)
		{
			auto g = pks(ti->Touch.Id).Active->Graph;
				
			g->ProcessTouch(ti, pks);
	
			//#ifdef _DEBUG
			//Level->Log->ReportDebug(this, L"%s", pks(v->Touch).Space->Name);
			//#endif 
	
			if(g->Focus)
			{
				FocusGraph = g;
			}
		}
	}

	if(m->Class == EInputClass::Keyboard)
	{
		auto ki = m->As<CKeyboardInput>();

		if(FocusGraph)
		{
			FocusGraph->ProcessKeyboard(ki);
		}
	}

	MessageProcessed(m);

	Pc.EndMeasure();
}
