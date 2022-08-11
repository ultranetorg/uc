#include "StdAfx.h"
#include "Engine.h"
#include "DirectVisualGraph.h"

using namespace uc;

CEngine::CEngine(CStorableServer * s, CConfig * c)
{
	ilInit();
	ilEnable(IL_ORIGIN_SET);

	Level = new CEngineLevel();
	Level->Server	= s;
	Level->Core		= s->Nexus->Core;
	Level->Config	= c;
	Level->Log		= Level->Core->Supervisor->CreateLog(L"Engine");
	
	Diagnostic		= Level->Core->Supervisor->CreateDiagnostics(L"Engine");
	PcUpdate		= new CPerformanceCounter(L"Engine update");
	Level->Core->AddPerformanceCounter(PcUpdate);

/*
	#ifdef D3D_DEBUG_INFO
		EngineLevel.Log->ReportWarning(this, L"D3D_DEBUG_INFO enabled");
	#endif*/

	#ifdef UOS_ENGINE_DIRECT
		DisplaySystem	= new CDirectSystem(Level);
	#elif defined UOS_ENGINE_VULKAN
		DisplaySystem	= new CVulkanEngine(Level);
	#endif

	ScreenEngine		= new CScreenEngine(Level, DisplaySystem);

	InputSystem			= new CInputSystem(Level, ScreenEngine);

	PipelineFactory		= new CDirectPipelineFactory(Level, DisplaySystem);
	MaterialFactory		= new CMaterialFactory(Level, DisplaySystem, PipelineFactory);
	TextureFactory		= new CTextureFactory(Level, DisplaySystem);
	FontFactory			= new CFontFactory(Level, DisplaySystem, ScreenEngine);

	Renderer			= new CRenderer(Level, DisplaySystem, PipelineFactory, MaterialFactory);
	Interactor			= new CInteractor(Level, ScreenEngine, InputSystem);
}
	
CEngine::~CEngine()
{
	delete Interactor;
	delete Renderer;

	delete PipelineFactory;
	delete TextureFactory;
	delete MaterialFactory;
	delete FontFactory;

	delete ScreenEngine;
	delete DisplaySystem;
	delete InputSystem;

	delete PcUpdate;

	delete Level;
}

void CEngine::Start()
{
	Level->Core->AddWorker(this);
	Level->Core->Processed += ThisHandler(Update);

	//Interactor->MessageProcessed	+= ThisHandler(OnInteractorMessageProcessed);
	Diagnostic->Updating			+= ThisHandler(OnDiagnosticsUpdate);
}

void CEngine::Stop()
{
	Level->Core->Processed -= ThisHandler(Update);
	Level->Core->RemoveWorker(this);
		
	//Interactor->MessageProcessed	-= ThisHandler(OnInteractorMessageProcessed);
	Diagnostic->Updating			-= ThisHandler(OnDiagnosticsUpdate);
}

bool CEngine::IsRunning()
{
	return Level->Core->IdleWorkers.Contains(this);
}

void CEngine::Update()
{
	PcUpdate->BeginMeasure();
	{
		Lock.lock();

		Renderer->Update();
		ScreenEngine->Update();

		Lock.unlock();
	}
	PcUpdate->EndMeasure();
}

//void CEngine::OnInteractorMessageProcessed(CInputMessage * m)
//{
//	Update();
//}

void CEngine::DoIdle()
{
	Update();
}

void CEngine::OnDiagnosticsUpdate(CDiagnosticUpdate & u)
{
	for(auto i : DisplaySystem->Devices)
	{
		Diagnostic->Add(L"VMFree: %d", i->GetVideoMemoryFreeAmount());
	}
}

CMesh * CEngine::CreateMesh()
{
	return new CMesh(Level);
}

CVisual * CEngine::CreateVisual(const CString & name, CMesh * mesh, CMaterial * mtl, CMatrix const & m)
{
	return new CVisual(Level, name, mesh, mtl, m);
}

CActive * CEngine::CreateActive(const CString & name, CMesh * mesh, CMatrix const & m)
{
	auto a = new CActive(Level, name);
	a->SetMesh(mesh);
	a->SetMatrix(m);
	return a;
}

CVisualGraph * CEngine::CreateVisualGraph()
{
	return new CDirectVisualGraph(Level, PipelineFactory);
}

CVisualSpace * CEngine::CreateVisualSpace(const CString & n)
{
	return new CVisualSpace(Level, n);
}

CActiveSpace * CEngine::CreateActiveSpace(const CString & n)
{
	return new CActiveSpace(n);

}
