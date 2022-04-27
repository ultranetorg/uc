#include "StdAfx.h"
#include "VisualGraph.h"

using namespace uc;

CVisualGraph::CVisualGraph(CEngineLevel * l, CDirectPipelineFactory * pf) : CVisualSpace(l, L"Root")
{
	PipelineFactory = pf;

	Diagnostic = Level->Core->Supervisor->CreateDiagnostics(GetClassName());
	Diagnostic->Updating += ThisHandler(OnDiagnosticsUpdate);

	DiagGrid.AddColumn(L"Name");
	DiagGrid.AddColumn(L"Refs");
	//DiagGrid.AddColumn(L"Space");
	DiagGrid.AddColumn(L"Material");
	DiagGrid.AddColumn(L"Status");
	///DiagGrid.AddColumn(L"View");
	DiagGrid.AddColumn(L"Clipping");
	#ifdef _DEBUG
	DiagGrid.AddColumn(L"Position");
	DiagGrid.AddColumn(L"Rotation");
	DiagGrid.AddColumn(L"Scale");
	DiagGrid.AddColumn(L"SS Pos");
	#endif
	DiagGrid.AddColumn(L"Visual BB (in the node CS)");
}
	
CVisualGraph::~CVisualGraph()
{
	Diagnostic->Updating -= ThisHandler(OnDiagnosticsUpdate);
}

void CVisualGraph::OnDiagnosticsUpdate(CDiagnosticUpdate & u)
{
	DiagGrid.Clear();

	std::function<void(CVisualSpace *, CVisual *, const CString &)> dumpNode
	= [this, &dumpNode, &u](auto s, auto n, auto & tab)
	{
		if(Diagnostic->ShouldProceed(u, DiagGrid.GetSize()))
		{
			auto & r = DiagGrid.AddRow();

			if(Diagnostic->ShouldFill(u, DiagGrid.GetSize()))
			{
				r.SetNext(L"%p %s%s", n, tab, n->GetName());
				r.SetNext(L"%d", n->GetRefs());
				r.SetNext(n->Material ? n->Material->Name : L"");
				r.SetNext(n->GetStatus());
				r.SetNext(n->GetClippingStatus());
				#ifdef _DEBUG
				r.SetNext(n->_Decomposed.Position.ToNiceString());
				r.SetNext(n->_Decomposed.Rotation.ToNiceString());
				r.SetNext(n->_Decomposed.Scale.ToNiceString());
				r.SetNext(n->_SSPosition.ToNiceString());
				#endif
				r.SetNext(n->GetAABB().ToNiceString());
			}

			for(auto i : n->Nodes)
			{
				dumpNode(s, i, tab + L"  ");
			}
		}
	};

	std::function<void(CVisualSpace *, const CString &)> dumpSpace =[this, &dumpSpace, &dumpNode, &u](auto s, auto & tab) 
																	{
																		if(Diagnostic->ShouldFill(u, DiagGrid.GetSize()))
																		{
																			auto & r = DiagGrid.AddRow();
																			r.SetNext(RGB(0, 204, 204), L"%p %s%s", s, tab, s->Name);
																		}

																		for(auto v : s->Nodes)
																			if(!v->Parent)
																				dumpNode(s, v, tab + L"  "); 
		
																		for(auto i : s->Spaces)
																			dumpSpace(i, tab + L"  ");
																	};

	dumpSpace(this, L"");

	Diagnostic->Add(u, DiagGrid);
}

void CVisualGraph::AssignPipeline(CVisual * v, CMap<CDirectPipeline *, int> & pipelines)
{
	if(v->Pipeline)
	{
		v->Pipeline->Visuals.Remove(v);
	}

	auto p = PipelineFactory->GetPipeline(v->Material->Shader);
	p->Visuals.push_back(v);

	if(!pipelines.Contains(p))
	{
		pipelines[p]++;
	}

	if(v->Pipeline && v->Pipeline != p)
	{
		v->Pipeline->Free();
	}
	if(p && v->Pipeline == p)
	{
		p->Free();
	}
		
	v->Pipeline = p;

	v->ShaderChanged = false;
		
	///auto n = v;
	///while(n)
	///{
	///	if(n->InheritableMaterial)
	///	{
	///		n->InheritableMaterial->BuildShaderCode(&c, v->Mesh);
	///	}
	///	n = n->Parent;
	///}

}
