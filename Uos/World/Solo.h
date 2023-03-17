#pragma once
#include "Unit.h"

namespace uc
{
	class UOS_WORLD_LINKING CSolo : public CUnit
	{
		public:
			CObject<CModel>								Model;

			UOS_RTTI
			CSolo(CWorldServer * l, CModel * m,											CView * hview);
			CSolo(CWorldServer * l, CString & gpath, CUol & entity, CString const & type,	CView * hview);
			CSolo(CWorldServer * l, CString & gpath,										CView * hview, CString const & name);
			~CSolo();

			using CArea::Save;
			using CArea::Load;

			void										Initialize();
			
 			CSpaceBinding<CVisualSpace> &				AllocateVisualSpace(CViewport * vp) override;
 			CSpaceBinding<CActiveSpace> &				AllocateActiveSpace(CViewport * vp) override;
 			void 										DeallocateVisualSpace(CVisualSpace * s) override;
 			void 										DeallocateActiveSpace(CActiveSpace * s) override;
 
			CSize										DetermineSize(CSize & smax, CSize & s) override;
			CTransformation								DetermineTransformation(CPositioning * ps, CPick & pk, CTransformation & t) override;
			EPreferedPlacement 							GetPreferedPlacement() override;
			CCamera 	*								GetPreferedCamera() override;

 			CSize										Measure() override;

			void 										Activate(CArea * a, CViewport * vp, CPick & pick, CTransformation & origin) override;
			void 										Update() override;

			void										UpdateHeader();
			void 										AdjustTransformation(CTransformation & t) override;

			void 										Interact(bool e) override;

			bool 										ContainsEntity(CUol & o) override;
			bool 										ContainsAvatar(CUol & o) override;

			CString &									GetDefaultInteractiveMasterTag() override;

			void										Save(CXon * x) override;
			void										Load(CXon * x) override;
	};
}