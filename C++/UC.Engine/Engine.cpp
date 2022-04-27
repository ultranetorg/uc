#include "StdAfx.h"
#include "Engine.h"
#include "DirectVisualGraph.h"

using namespace uc;

CEngine::CEngine(CLevel2 * l, CServer * s, CConfig * c) : Level(l), EngineLevel(*l)
{
	ilInit();
	ilEnable(IL_ORIGIN_SET);

	EngineLevel.Server	= s;
	EngineLevel.Config	= c;
	EngineLevel.Log		= Level->Core->Supervisor->CreateLog(L"Engine");
	
	Diagnostic		= Level->Core->Supervisor->CreateDiagnostics(L"Engine");
	PcUpdate		= new CPerformanceCounter(L"Engine update");
	Level->Core->AddPerformanceCounter(PcUpdate);

/*
	#ifdef D3D_DEBUG_INFO
		EngineLevel.Log->ReportWarning(this, L"D3D_DEBUG_INFO enabled");
	#endif*/


	#ifdef UOS_ENGINE_DIRECT
		DisplaySystem	= new CDirectSystem(&EngineLevel);
	#elif defined UOS_ENGINE_VULKAN
		DisplaySystem	= new CVulkanEngine(&EngineLevel);
	#endif

	ScreenEngine		= new CScreenEngine(&EngineLevel, DisplaySystem);

	InputSystem			= new CInputSystem(&EngineLevel, ScreenEngine);

	PipelineFactory		= new CDirectPipelineFactory(&EngineLevel, DisplaySystem);
	MaterialFactory		= new CMaterialFactory(&EngineLevel, DisplaySystem, PipelineFactory);
	TextureFactory		= new CTextureFactory(&EngineLevel, DisplaySystem);
	FontFactory			= new CFontFactory(&EngineLevel, DisplaySystem, ScreenEngine);

	Renderer			= new CRenderer(&EngineLevel, DisplaySystem, PipelineFactory, MaterialFactory);
	Interactor			= new CInteractor(&EngineLevel, ScreenEngine, InputSystem);
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
	return new CMesh(&EngineLevel);
}

CVisual * CEngine::CreateVisual(const CString & name, CMesh * mesh, CMaterial * mtl, CMatrix const & m)
{
	return new CVisual(&EngineLevel, name, mesh, mtl, m);
}

CActive * CEngine::CreateActive(const CString & name, CMesh * mesh, CMatrix const & m)
{
	auto a = new CActive(&EngineLevel, name);
	a->SetMesh(mesh);
	a->SetMatrix(m);
	return a;
}

CVisualGraph * CEngine::CreateVisualGraph()
{
	return new CDirectVisualGraph(&EngineLevel, PipelineFactory);
}

CVisualSpace * CEngine::CreateVisualSpace(const CString & n)
{
	return new CVisualSpace(&EngineLevel, n);
}

CActiveSpace * CEngine::CreateActiveSpace(const CString & n)
{
	return new CActiveSpace(n);

}
