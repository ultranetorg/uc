#include "StdAfx.h"
#include "FieldItemElement.h"

using namespace uc;

CFieldItemElement::CFieldItemElement(CShellLevel * l, CFieldWorld * fo, CString const & dir) : CAvatarCard(l->World, GetClassName())
{
	Level = l;
	Operations = fo;
	Path = dir;
	VisualPath = CPath::ReplaceExtension(dir, L"visual");
	
	EnableActiveBody = false;
	Visual->Clipping = EClipping::No;
	Active->Clipping = EClipping::No;

	Active->MouseEvent[EListen::NormalAll] +=	[this](auto, auto s, auto arg)
												{
													if(arg->Control == EMouseControl::LeftButton && arg->Event == EGraphEvent::Click)
													{
														Level->Core->Open(Entity->Object, sh_new<CShowParameters>(arg, Level->Style));
														arg->StopPropagation = true;
													}
												};

	Active->TouchEvent[EListen::NormalAll] +=	[this](auto, auto s, auto arg)
												{
													if(arg->Event == EGraphEvent::Click && arg->Picks.size() == 1)
													{
														Level->Core->Open(Entity->Object, sh_new<CShowParameters>(arg, Level->Style));
														arg->StopPropagation = true;
													}
												};


	Selection = new CVisual(Level->Engine->Level, L"selection", null, null, CMatrix::FromPosition(0, 0, Z_STEP));
	auto m = Level->Engine->CreateMesh();
	Selection->SetMesh(m);
	Selection->SetMaterial(Level->World->Materials->GetMaterial(Level->Style->Get<CFloat4>(L"Selection/Border/Material")));
	Selection->Mesh->SetPrimitiveInfo(EPrimitiveType::LineList);
	m->Free();
}

CFieldItemElement::~CFieldItemElement()
{
	OnDependencyDestroying(Entity);
	
	Selection->Free();
}

void CFieldItemElement::SetItem(CString const & type, CFieldItem * fi)
{
	Type = type;
	Entity = fi;
	Entity->Destroying += ThisHandler(OnDependencyDestroying);
	
	SetEntity(fi->Object);

	auto a = Level->World->GenerateAvatar(fi->Object, Type);
	auto dir = CPath::ReplaceLast(Path, a.Object);

	SetAvatar(a, dir);

	Avatar->As<CFieldable>()->Place(Operations);
}

void CFieldItemElement::Revive()
{
	if(Avatar)
	{
		throw CException(HERE, L"Already revived");
	}
}

void CFieldItemElement::OnDependencyDestroying(CInterObject * o)
{
	///if(Avatar && o == Avatar)
	///{
	///	Avatar->Destroying -= ThisHandler(OnDependencyDestroying);
	///
	///	RemoveNode(Avatar);
	///
	///	Avatar = new CStaticAvatar(Level->World, Level->Server, Avatar, Avatar->Name);
	///	AddNode(Avatar);
	///	Avatar->Free();
	///			
	///	Avatar.Clear();
	///}

	if(Entity && o == Entity)
	{
		Entity->Destroying -= ThisHandler(OnDependencyDestroying);
		Entity.Clear();
	}
}

void CFieldItemElement::Save(IMeshStore * mhs, IMaterialStore * mts)
{
	if(!Size || Size.W <= 0 || Size.H <= 0)
	{
		throw CException(HERE, L"Not allowed");
	}

	CTonDocument d;

	d.Add(L"Entity")->Set(Entity.Url);
	d.Add(L"Avatar")->Set(Avatar.Url);
	d.Add(L"T")->Set(Transformation);
	
	Save(&d);

	auto s = Level->Storage->WriteFile(CPath::Join(IFileSystem::UserGlobal, Path));
	d.Save(&CXonTextWriter(s, true));
	Level->Storage->Close(s);
	
	Avatar->Save();

	///CTonDocument vd;
	///Avatar->SaveBasic(vd.Add(L"Static"), mhs, mts);
	///
	///s = Level->Storage->WriteFile(Level->Storage->MapPath(UOS_MOUNT_SERVER_GLOBAL, VisualPath));
	///vd.Save(&CXonTextWriter(s, true));
	///Level->Storage->Close(s);
}

void CFieldItemElement::Load(IMeshStore * mhs, IMaterialStore * mts, CFieldServer * field)
{
	auto f = Level->Storage->ReadFile(CPath::Join(IFileSystem::UserGlobal, Path));
	auto & d = CTonDocument(CXonTextReader(f));
	Level->Storage->Close(f);

	Entity.Url	= d.Get<CUol>(L"Entity");
	Avatar.Url	= d.Get<CUol>(L"Avatar");
	Transform(d.Get<CTransformation>(L"T"));

	Entity = field->Find(Entity.Url);

	if(Entity)
	{
		Entity->Destroying += ThisHandler(OnDependencyDestroying);

		SetAvatar(Avatar.Url, CPath::ReplaceLast(Path, Avatar.Url.Object));
		SetEntity(Entity->Object);
	
		if(Avatar)
		{
			Avatar->As<CFieldable>()->Place(Operations);
			//Visual->SetInheritableMaterial(null);
		}
		else
		{
			///Visual->SetInheritableMaterial(Service->GetOfflineMaterial());
		}
	
		Load(&d);
	}

	/// load static if failed to revice
	///AvatarNode = new CStaticAvatar(Level->World, Level->Hub, d.One(L"Static"), mhs, mts, Level->World);
	///AvatarNode->TransformZ(DEFAULT_Z);
	///AddNode(AvatarNode);
	///AvatarNode->Free();
}

void CFieldItemElement::Delete()
{
	Level->Storage->Delete(CPath::Join(IFileSystem::UserGlobal, Path));
	Avatar->Delete();
	//Level->World->DestroyAvatar(Avatar);
}

void CFieldItemElement::UpdateLayout(CLimits const & l, bool apply)
{
	__super::UpdateLayout(l, apply);
	ResizeSelection(W, H);

}

void CFieldItemElement::Select(bool e)
{
	if(e && !Selection->Parent)
		Visual->AddNode(Selection);
	else if(!e && Selection->Parent)
		Visual->RemoveNode(Selection);
}

void CFieldItemElement::ResizeSelection(float w, float h)
{
	if(!isfinite(w) || !isfinite(h))
		return;

	w -= 1;
	h -= 1;

	float a = min(w/4, h/4);
	if(a < 3)
		a = 3;
	if(a > 32)
		a = 32;

	CArray<CFloat3> p;
	p.push_back(CFloat3(0, 0, 0)); // 4 horizontal lines
	p.push_back(CFloat3(a, 0, 0));
	p.push_back(CFloat3(0, h, 0));
	p.push_back(CFloat3(a, h, 0));

	p.push_back(CFloat3(w - 0, 0, 0)); 
	p.push_back(CFloat3(w - a, 0, 0));
	p.push_back(CFloat3(w - 0, h, 0));
	p.push_back(CFloat3(w - a, h, 0));

	p.push_back(CFloat3(0, 0, 0)); // 4 vertical lines
	p.push_back(CFloat3(0, a, 0));
	p.push_back(CFloat3(0, h, 0));
	p.push_back(CFloat3(0, h-a, 0));

	p.push_back(CFloat3(w, 0, 0)); 
	p.push_back(CFloat3(w, a, 0));
	p.push_back(CFloat3(w, h, 0));
	p.push_back(CFloat3(w, h-a, 0));

	CArray<int> i = {	0, 1, 2, 3, 4, 5, 6, 7, 
						8, 9, 10, 11, 12, 13, 14, 15 };

	Selection->Mesh->SetIndexes(i);
	Selection->Mesh->SetVertices(UOS_MESH_ELEMENT_POSITION, p);
}

bool CFieldItemElement::IsSelected()
{
	return Selection->Parent != null;
}

void CFieldItemElement::PropagateLayoutChanges(CElement * s)
{
	UpdateLayout();
}
