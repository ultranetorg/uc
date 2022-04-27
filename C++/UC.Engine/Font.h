#pragma once
#include "ScreenEngine.h"

namespace uc
{
	class UOS_ENGINE_LINKING CFont : public virtual CShared
	{
		public:
			//IDWriteTextFormat *							Format;
			HFONT										HFont;
			HFONT										HFontOld;
			HDC											FontDC;

			CString										Family;
			float										Size;
			bool										IsBold;
			bool										IsItalic;

			float										Height;
			float										PixelHeight;
			CArray<float>								Widths;
			TEXTMETRIC									TextMetric;	

			CScreenEngine *								ScreenEngine;

			CFont(CDirectSystem * ge, CScreenEngine * se, CString const & family, float size, bool bold, bool italic);
			~CFont();

			void										Draw(CString const & t, HDC dc, RECT & r, CFloat4 & c, bool wrap, bool ells, int tabs, EXAlign xa, EYAlign ya);
			CSize										Measure(const CString & t, float wmax, float hmax, bool wrap = false, bool ells = false, int tabs = 1, EXAlign xa = EXAlign::Left, EYAlign ya = EYAlign::Top);
			CSize										MeasureInPixels(const CString & t, float wmax, float hmax, bool wrap = false, bool ells = false, int tabs = 1, EXAlign xa = EXAlign::Left, EYAlign ya = EYAlign::Top);
			UINT										GetDrawFlags(bool wrap, bool ells, EXAlign xa, EYAlign ya);
			///int											Pick(const CString & t, int tabs, float x);
		
	};

}