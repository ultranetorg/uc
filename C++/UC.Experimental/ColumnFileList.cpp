#include "stdafx.h"
#include "ColumnFileList.h"

using namespace uc;

CColumnFileList::CColumnFileList(CExperimentalLevel * l) : CRectangle(l->World, GetClassName())
{
	Level = l;

	UseClipping(EClipping::Inherit, true);

	Selector = new CRectangle(l->World, L"Selector");
	Selector->ApplyStyles(l->Style, {L"Selection"});
	Selector->Express(L"B", [this]{ return CFloat6(1.f); });
	Selector->Express(L"H", [this]{ return Columns.back()->Current->Size.H; });
	Selector->Express(L"W", [this]{ return Columns.back()->Width; });

	Active->MouseEvent[EListen::NormalAll]		+= ThisHandler(OnMouse);
	Active->KeyboardEvent[EListen::NormalAll]	+= ThisHandler(OnKeyboard);

	IconMesh = new CSolidRectangleMesh(l->Engine->Level);
	IconMesh->Generate(0, 0, 16, 16);

	Express(L"C",	[this](auto apply)
					{
						float x = Scroll.x;

						for(auto c : Columns)
						{
							float y = IH - c->Y;
							c->Width = 0.f;

							for(auto i : c->Items)
							{
								y -= i->H;
								i->Transform(x, y, Z_STEP*2);

								if(IsVisibleY(this, i) && !i->Parent)
								{
									AddNode(i);
								} 
								if(!IsVisibleY(this, i) && i->Parent)
								{
									RemoveNode(i);
								}
								if(i->Parent)
								{
									c->Width = max(c->Width, i->FullArea.W);
								}
							}
							c->Width = min(c->Width, IW);
							c->X = x;
							x += c->Width;
	
							for(auto i : c->Items)
							{
								if(i->Parent)
								{
									if(c->Width != i->LastColumnW)
									{
										i->UpdateLayout(Climits, apply);
										i->LastColumnW = c->Width;
									}
								}
								if(i->Transformation.Position.y < 0)
								{
									break;
								}
							}
						}
	
						if(!Columns.empty() && Columns.back()->Current)
						{
							auto t = Columns.back()->Current->Transformation.Position;
							Selector->Transform(t.x, t.y);
							Selector->UpdateLayout(Climits, apply);
						}

						return CalculateSize(Nodes);
					});
}

CColumnFileList::~CColumnFileList()
{
	Clear();

	Selector->Free();

	IconMesh->Free();
}

void CColumnFileList::SetRoot(CString & u)
{
	Root = u;

	Clear();

	AddColumn(u);
}

void CColumnFileList::SetPath(CString const & u)
{
	Clear();

	auto dirs = u.Substring(Root.size()).Split(L"/").Where([](auto & i){ return !i.empty(); });
	CString l;

	AddColumn(Root);

	for(auto & i : dirs)
	{
		l += L"/" + i;

		AddColumn(l);
	}
}

void CColumnFileList::Clear()
{
	for(auto i=Columns.rbegin(); i != Columns.rend(); i++)
	{
		for(auto n : (*i)->Items)
		{
			if(n->Parent)
				RemoveNode(n);
		}
		
		delete *i;
	}

	Columns.clear();
	SetCurrent(null);
	Scroll = CFloat2(0,0);
}

void CColumnFileList::AddColumn(const CString & path)
{
	auto c = new CFileListColumn(Level, path);
			
	auto & entries = Level->Storage->Enumerate(path, L".*");
	
	if(entries.empty())
	{
		CFileSystemEntry di;
		di.NameOverride = L"..";
		entries.push_front(di);
	}

	for(auto & f : entries)
	{
		auto fie = new CFileItemElement(Level, 
										f.NameOverride == L".." ? L"" : CPath::Join(path, f.Name),
										f.NameOverride, 
										f.Type == CFileSystemEntry::Directory, 
										IconMesh, 
										Level->ImageExtractor);

		fie->Express(L"M", []{ return CFloat6(1.f); });
		fie->CalculateFullSize();

		fie->Express(L"W", [this, fie]{ return Columns[fie->Column]->Width; });
		fie->Column = Columns.size();

		c->Items.Add(fie);
		fie->Free();
	}

	c->Items.Sort([](auto a, auto b){ return a->IsDirectory && !b->IsDirectory; });

	Columns.push_back(c);

	UpdateLayout();

	SetCurrent(c->Items.front());

	PathChanged(path);
}

void CColumnFileList::RemoveColumn(CFileListColumn * c)
{
	for(auto n : c->Items)
	{
		if(n->Parent)
			RemoveNode(n);
	}

	Columns.Remove(c);
	delete c;

	SetCurrent(Columns.back()->Current);

	PathChanged(Columns.back()->Path);
}

void CColumnFileList::SetCurrent(CFileItemElement * i)
{
	if(Columns.empty())
	{
		return;
	}

	auto c = Columns.back();
	
	c->Current = i;

	if(c->Current && !Selector->Parent)
	{
		AddNode(Selector);
	}
	if(!c->Current && Selector->Parent)
	{
		RemoveNode(Selector);
	}
		
	if(c->Current)
	{
		auto t = c->Current->Transformation.Position;
		Selector->Transform(t.x, t.y);
		if(!Columns.empty() && c->Current->Column != -1)
		{
			Selector->UpdateLayout();

			if(Selector->Transformation.Position.x + Selector->W > IW)
			{
				Scroll.x -= (c->X + c->Width);
				Scroll.x += IW;
				UpdateLayout();
			}

			if(Selector->Transformation.Position.x < 0)
			{
				Scroll.x += -c->X;
				UpdateLayout();
			}

			if(Columns.Sum<float>([](auto i){ return i->Width; }) < IW && Scroll.x < 0)
			{
				Scroll.x = 0;
				UpdateLayout();
			}

			if(Selector->Transformation.Position.x + Selector->W < IW /*&& Columns.Sum<float>([](auto i){ return i->Width; }) > IW*/ && Scroll.x < 0)
			{
				Scroll.x += (IW - (Selector->Transformation.Position.x + Selector->W));
				UpdateLayout();
			}

			if(Selector->Transformation.Position.y < 0)
			{
				c->Y = c->Y + Selector->Transformation.Position.y;
				UpdateLayout();
			}

			if(Selector->Transformation.Position.y + Selector->H > IH)
			{
				c->Y = c->Y - (IH - Selector->Transformation.Position.y - Selector->H );
				UpdateLayout();
			}
		}
	}
}

void CColumnFileList::OnMouse(CActive * r, CActive * s, CMouseArgs * arg)
{
	if(arg->Action == EMouseAction::On && arg->Control != EMouseControl::MiddleButton)
	{
		auto fi = s->AncestorOwnerOf<CFileItemElement>();
	
		if(fi)
		{
			SetCurrent(fi);
		}
	}
}

void CColumnFileList::OnKeyboard(CActive * r, CActive * s, CKeyboardArgs * arg)
{
	if(arg->Action == EKeyboardAction::On)
	{
		auto c = Columns.back();

		switch(arg->Control)
		{
			case EKeyboardControl::Down:
				if(c->Current != c->Items.back())
				{
					SetCurrent( *(++c->Items.Findi(c->Current)) );
				}
				break;

			case EKeyboardControl::Up:
				if(c->Current != c->Items.front())
				{
					SetCurrent( *(--c->Items.Findi(c->Current)) );
				}
				break;

			case EKeyboardControl::Right:
			case EKeyboardControl::Return:
			{
				if(c->Current->Path.EndsWith(L".."))
				{
					break;
				}
				if(c->Current->IsDirectory)
				{
					AddColumn(c->Current->Path);
				}
				break;
			}

			case EKeyboardControl::Left:
			{
				if(Columns.size() < 2)
				{
					break;
				}

				auto c = Columns.back();

				RemoveColumn(c);

				break;
			}
		}
	}
}

void CColumnFileList::Save(CXon * n)
{
//	n->Add(L"Path")->Set(Columns.back()->Path);
}

void CColumnFileList::Load(CXon * n)
{
//	Clear();
//
//	auto u = n->Get<CString>(L"Path");
//	auto i = u.find(L'/');
//
//	while(i != CString::npos)
//	{
//		AddColumn(u.Substring(0, i+1));
//
//		i = u.find(L'/', i+1);
//	}
}