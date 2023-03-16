#pragma once
#include "ScreenRenderTarget.h"

namespace uc
{
	class UOS_ENGINE_LINKING CScreenViewport : public CViewport
	{
		public:
			CScreenRenderTarget *	Target;
			float					SW;
			float					SH;

			CScreenViewport(CEngineLevel * l, CScreenRenderTarget * t, float w, float h, float tx, float ty, float tw, float th, float sw, float sh) : CViewport(l)
			{
				Target		= t;
				Tags		= Target->Screen->Tags;

				W = w;
				H = h;

				TX = tx;
				TY = ty;

				TW = tw;
				TH = th;

				SW = sw;
				SH = sh;
			}

			CFloat2 ScreenToTarget(CFloat2 & sp)
			{
				return CFloat2(sp.x * TW/SW - TX, sp.y * TH/SH - TY);
			}

			CFloat2 ScreenToViewport(CFloat2 & sp)
			{
				return TargetToViewport(ScreenToTarget(sp));
			}

			bool Contains(CFloat2 & sp)
			{
				auto vpp = ScreenToViewport(sp);
				return CRect(0, 0, W, H).Contains(vpp);
			}

			CFloat2 ScreenToLogicalRatio()
			{
				return {SW/W, SH/H};
			}

			void Apply()
			{
				Target->SetViewport(TX, Target->Size.H - TY - TH, TW, TH, MinZ, MaxZ);
			}
	};
}
