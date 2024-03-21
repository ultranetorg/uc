#pragma once
#include "ExperimentalLevel.h"

namespace uc
{
	class CFileItemElement : public CRectangle
	{
		public:
			CString					Path;
			CText *					Text;
			CElement	*			Icon;
			CExperimentalLevel *	Level;
			int						Column = -1;
			bool					IsDirectory;
			CSize					FullArea;
			CSize					LastImax;
			CImageExtractorProtocol *		FileExtractor;
			//CString					Object;
			float					LastColumnW = 0.f;

			UOS_RTTI
			CFileItemElement(CExperimentalLevel * l, CString const & path, CString const & nameoverride, bool isdir, CMesh * m, CImageExtractorProtocol * fe) : CRectangle(l->World, GetClassName())
			{
				Level = l;
				FileExtractor = fe;

				//Object = se->Path;

				UseClipping(EClipping::Apply, false);

				Text = new CText(l->World, l->Style);
				
				Icon = new CElement(l->World, L"FileIcon");
				Icon->Express(L"W", []{ return 16.f; });
				Icon->Express(L"H", []{ return 16.f; });
				Icon->Visual->SetMesh(m);
				Icon->Active->SetMesh(m);
				Icon->Transform(0, 0, Z_STEP);
				//Express(L"EElementProp::M", [this](auto n){ Margin = 1; });
				
				AddNode(Text);
				AddNode(Icon);


				Path = path;
				IsDirectory = isdir;

				if(nameoverride != L"..")
				{
					Text->SetText(CPath::GetName(Path));
					Take();
					FileExtractor->GetIconMaterial(this, (CUrl)Level->Storage->ToUol(Path), 16)->Done =	[this](auto m)
																										{
																											Icon->Visual->SetMaterial(m); 
																											Free();
																										};
				}
				else
				{
					Text->SetText(L"..");
					Icon->Visual->SetMaterial(null);
				}

				Icon->UpdateLayout();

				Express(L"C",	[this](auto apply)
								{
									auto l = Climits;

									Text->UpdateLayout(l, apply);

									if(isfinite(Icon->H) && isfinite(Text->H))
									{
										float y = (Icon->H - Text->H)/2;
										Text->Transform(Icon->Size.W, y, Z_STEP); 
									}

									return CSize(Icon->W + Text->W, max(Icon->H, Text->H), 0);
								});
			}

			virtual ~CFileItemElement()
			{
				RemoveNode(Text);
				RemoveNode(Icon);
				Text->Free();
				Icon->Free();
			}

			void CalculateFullSize()
			{
				UpdateLayout(CLimits::Max, false);
				FullArea = Size;
			}

			///void Draw(CCanvas * c, CFloat2 & p, CCanvasFont * f, float w)
			///{
			///	auto b = c->CreateBitmap(FileExtractor->GetIcon(Object, 16));
			///	c->DrawBitmap(b, p);
			///	delete b;
			///
			///	c->DrawText(CUfl::GetFileName(Path), f, null, CRect(p, CFloat2(16, w - 16)));
			///}
	};
}