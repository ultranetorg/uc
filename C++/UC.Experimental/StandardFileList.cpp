#include "stdafx.h"
#include "StandardFileList.h"

using namespace uc;

CStandardFileList::CStandardFileList(CExperimentalLevel * l) : CRectangle(l->World, GetClassName())
{
	Level = l;

	UseClipping(EClipping::Inherit, true);

	Active->MouseEvent[EListen::NormalAll]		+= ThisHandler(OnMouse);
	Active->KeyboardEvent[EListen::NormalAll]	+= ThisHandler(OnKeyboard);

	Selector = new CRectangle(l->World, L"Selector");
	Selector->Express(L"B", [this]{ return CFloat6(1.f); });
	Selector->Express(L"H", [this]{ return Current->Size.H; });
	Selector->Express(L"W", [this]{ return Columns[Current->Column]; });

	Selector->BorderMaterial = Level->World->Materials->GetMaterial(CFloat3(252/255.f, 172.f/255, 41.f/255));

	IconMesh = new CSolidRectangleMesh(l->Engine->Level);
	IconMesh->Generate(0, 0, 16, 16);

	Express(L"C",	[this](auto apply)
					{
						auto l = Climits;

						Columns.clear();

						float y = l.Smax.H;
						float x = Scroll.x;
						float wmax = 0.f;

						if(!Items.Empty())
						{
							for(auto i : Items)
							{
								y -= i->Size.H;
	
								if(y < 0)
								{
									Columns.push_back(min(l.Smax.W, wmax));
	
									x += min(l.Smax.W, wmax);
									y = l.Smax.H - i->Size.H;
									wmax = 0.f;
								}
									
								i->Column = Columns.size();
		
								i->Transform(CTransformation(x, y, Z_STEP*2));

								wmax = max(wmax, i->FullArea.W);

								if(IsVisible(this, i) && !i->Parent)
								{
									AddNode(i);
								} 
								if(!IsVisible(this, i) && i->Parent)
								{
									RemoveNode(i);
								}
							}
					
							Columns.push_back(min(l.Smax.W, wmax));
	
							for(auto i : Items)
							{
								if(i->Parent)
								{
									i->UpdateLayout(l, apply);
								}
							}
	
							if(Current)
							{
								auto t = Current->Transformation;
								t.Position.z = Z_STEP;
								Selector->Transform(t);
								Selector->UpdateLayout(l, apply);
							}
						}

						return CSize::Empty;
					});
}

CStandardFileList::~CStandardFileList()
{
	for(auto i : Items)
	{
		if(i->Parent)
		{
			RemoveNode(i);
		}
	}
	Selector->Free();

	IconMesh->Free();
}

void CStandardFileList::SetSource(CString & u)
{
	ImageExtractor = Level->Nexus->Connect(this, IImageExtractor::InterfaceName);

	Source = u;

	Load(u);
}

void CStandardFileList::Load(CString path)
{
	Columns.clear();
	SetCurrent(null);
	Scroll = CFloat2(0,0);

	for(auto i : Items)
	{
		if(i->Parent)
		{
			RemoveNode(i);
		}
	}
		
	Items.Clear();
	
	auto & entries = Level->Storage->Enumerate(path, L".*");

	CString prev;
		
	auto i = Paths.Findi(path);
	if(i != Paths.end())
	{
		prev = Paths.back();
		Paths.erase(i, Paths.end());
	}

	if(!Paths.empty())
	{
		CFileSystemEntry e;
		e.NameOverride = L"..";
		//e.Name = Paths.back();
		e.Type = CFileSystemEntry::Directory;
		entries.push_front(e);
	}

	for(auto & f : entries)
	{
		auto fi = new CFileItemElement(	Level, 
										f.NameOverride == L".." ? Paths.back() : CPath::Join(path, f.Name), 
										f.NameOverride, 
										f.Type == CFileSystemEntry::Directory, 
										IconMesh, 
										ImageExtractor);

		fi->Express(L"M", []{ return CFloat6(1.f); });
		fi->CalculateFullSize();

		fi->Express(L"W", [this, fi]{ return Columns[fi->Column]; });
		fi->Text->Express(L"W", [fi]{ return fi->Text->Slimits.Smax.W - fi->Icon->W; });
		
		Items.Add(fi);
		fi->Free();
	}


	Items.Sort([](auto a, auto b){ return a->IsDirectory && !b->IsDirectory; });

	UpdateLayout();
	
	if(!Items.Empty())
	{
		auto fi = Items.Find(	[prev](CFileItemElement * i)
								{ 
									return i->Path == prev;
									//return i->Info.dwVolumeSerialNumber == info.dwVolumeSerialNumber && i->Info.nFileIndexHigh == info.nFileIndexHigh && i->Info.nFileIndexLow == info.nFileIndexLow; 
								});

		if(fi)
		{
			SetCurrent(fi);
		}
		else
		{
			SetCurrent(Items.front());
		}

		Paths.push_back(path);
	}

	PathChanged(path);
}

void CStandardFileList::OnMouse(CActive * r, CActive * s, CMouseArgs * arg)
{
	if((arg->Control == EMouseControl::LeftButton || arg->Control == EMouseControl::RightButton) && arg->Action == EMouseAction::On)
	{
		auto fi = s->AncestorOwnerOf<CFileItemElement>();

		if(fi)
		{
			SetCurrent(fi);
		}
	}

	if(arg->Control == EMouseControl::LeftButton && arg->Action == EMouseAction::DoubleClick)
	{
		if(Current)
		{
			Level->Core->Open(Level->Storage->ToUol(Current->Path), sh_new<CShowParameters>(arg, Level->Style));
		}
	}
}

void CStandardFileList::OnKeyboard(CActive * r, CActive * s, CKeyboardArgs * arg)
{
	if(arg->Action == EKeyboardAction::On)
	{
		switch(arg->Control)
		{
			case EKeyboardControl::Down:
				if(Current != Items.back())
				{
					SetCurrent( *(++Items.Findi(Current)) );
				}
				break;

			case EKeyboardControl::Up:
				if(Current != Items.front())
				{
					auto c = Current;
					SetCurrent( *(--Items.Findi(Current)) );
				}
				break;

			case EKeyboardControl::Right:
			{
				auto c = Current;

				auto it = Items.Findi(Current);
				for(int i = 0; i < (int)IH/18 && *it != Items.back(); i++)
				{
					it++;
				}

				SetCurrent(*it);
				break;
			}

			case EKeyboardControl::Left:
			{
				auto c = Current;

				auto it = Items.Findi(Current);
				for(int i = 0; i < (int)IH/18 && *it != Items.front(); i++)
				{
					it--;
				}

				SetCurrent(*it);
				break;
			}

			case EKeyboardControl::Return:
				
				if(Current->IsDirectory)
					Load(Current->Path);
				else
					Level->Core->Open(Level->Storage->ToUol(Current->Path), sh_new<CShowParameters>(arg, Level->Style));

				break;
		}
	}
}

void CStandardFileList::SetCurrent(CFileItemElement * i)
{
	Current = i;

	if(Current && !Selector->Parent)
	{
		AddNode(Selector);
	}
	if(!Current && Selector->Parent)
	{
		RemoveNode(Selector);
	}
		
	if(Current)
	{
		auto t = Current->Transformation;
		t.Position.z = -1;
		Selector->Transform(t);
		if(!Columns.empty() && Current->Column != -1)
		{
			Selector->UpdateLayout();

			if(Selector->Transformation.Position.x + Selector->Size.W > IW)
			{
				Scroll.x = 0;
				for(auto i = 0; i < Current->Column+1; i++)
				{
					Scroll.x -= Columns[i];
				}

				Scroll.x += IW;
				UpdateLayout();
			}

			if(Selector->Transformation.Position.x < 0)
			{
				Scroll.x = 0;
				for(auto i = 0; i < Current->Column; i++)
				{
					Scroll.x -= Columns[i];
				}

				UpdateLayout();
			}

		}
	}
}

void CStandardFileList::PropagateLayoutChanges(CElement * n)
{
	if(Current != null) // means not loading
	{
		__super::ProcessLayoutChanges(n);
	}
}
