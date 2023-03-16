#include "stdafx.h"
#include "VwmExporter.h"

namespace uos
{
	CVwmExporter::CVwmExporter( CSource * s)
	{
		Source	= s;
		Zip		= null;
	}

	CVwmExporter::~CVwmExporter()
	{
	}

	void CVwmExporter::Clear()
	{	
		if(WritePool != null)
		{
			delete WritePool;
			WritePool = null;
		}
		if(Zip != null)
		{
			delete Zip;
			Zip = null;
		}
		for(auto i : Roots)
		{
			delete i;
		}

		Roots.clear();
		
		delete MaterialFactory;
		delete MeshFactory;
		
		UI.SetProgressOverall(100);
		UI.SetProgressCurrent(100);
	}

	void CVwmExporter::RecordAsset(const MaxSDK::AssetManagement::AssetUser& asset)
	{
		WritePool->RelocateSource(asset.GetFullFilePath().data());
	}

	void CVwmExporter::Start()
	{
		CMainDlg mainDlg(Source);

		if(mainDlg.Show() == IDOK)
		{
			UI.Show(Source, this);
		}
	}
	
	void CVwmExporter::OnProcessInit()
	{
		UI.ReportMessage(L"Started...\r\n");
		
		try
		{
			Zip	= new CZipDirectory(Source->DestFilePath, EFileMode::New);

		}
		catch(CAttentionException & e)
		{
			UI.ReportError(e.Message.c_str());
			return;
		}

		WritePool		= new CWritePool(Zip);
		MeshFactory		= new CMeshFactory(Source, &UI, WritePool);
		MaterialFactory	= new CMaterialFactory(Source, &UI, WritePool);
		
		CString pv = CPath::ReplaceFileName(Source->MaxInterface->GetCurFilePath().data(), L"Preview.jpg");
		if(CPath::IsFile(pv))
		{
			WritePool->Add(L"Preview.jpg", pv, &UI);
		}
				
		Time = Source->MaxInterface->GetTime();

		Enumerate();
		Export();

		Clear();

		UI.ReportMessage(L"Done\r\n");
	}
/*
	int CVwmExporter::callback(INode * node)
	{
		INode * p = node->GetParentNode();
		if(p->IsRootNode())
		{
			try
			{
				CVwmNode * vwmNode = new CVwmNode(node, MaterialFactory, &ProcessDlg, Source);
				Nodes.insert(vwmNode);
			}
			catch(CException &)
			{
			}
		}
		
		return TREE_CONTINUE;
	}
*/
	void CVwmExporter::Enumerate()
	{
		Source->MaxInterface->EnumAuxFiles(*this, FILE_ENUM_ALL);
		UI.SetProgressOverall(10);
	
		Enumerate(null, Source->MaxInterface->GetRootNode());
		UI.SetProgressOverall(25);
	}

	void CVwmExporter::Enumerate(CVwmNode * parent, INode * node)
	{
		CVwmNode * vwmNode = null;

		auto name = node->GetName();
		auto cid = node->ClassID();

		if(node == Source->MaxInterface->GetRootNode() || node->IsGroupHead())
		{
			vwmNode = new CVwmNode(parent, node, &UI, Source);
		}
		else if(MeshFactory->AskIsValidTriObj(node))
		{	
			vwmNode = new CVwmNode(parent, node, &UI, Source);
			vwmNode->MakeVisual(MeshFactory, MaterialFactory);
		}
		else if(CVwmCamera::AskIsCamera(node, Source))
		{
			vwmNode = new CVwmNode(parent, node, &UI, Source);
			vwmNode->MakeCamera();
		}
		else if(CVwmLight::AskIsLight(node, Source))
		{
			vwmNode = new CVwmNode(parent, node, &UI, Source);
			vwmNode->MakeLight();
		}

		if(vwmNode != null)
		{
			if(parent != null)
			{
				parent->AddChild(vwmNode);
			}
			else
			{
				Roots.insert(vwmNode);
			}
		}

		int n = node->NumberOfChildren();
		for(int i=0; i<n; i++)
		{
			Enumerate(vwmNode, node->GetChildNode(i));
		}
	}

	void CVwmExporter::Export()
	{
		MeshFactory->Export(Zip, 25, 25);
		MaterialFactory->Export(Zip, 50, 25);

		int c = 0;
		CBonDocument doc;

		for(auto mi : Roots)
		{
			mi->Export(&doc, Zip);
			c++;
			UI.SetProgressOverall(75+(25*c)/(int)Roots.size());
		}

		auto s = Zip->OpenWriteStream(L"Graph.bon");
		doc.Save(&CXonBinaryWriter(s));
		Zip->Close(s);
	}	
}
