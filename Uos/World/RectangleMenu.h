#pragma once
#include "RectangleSectionMenuItem.h"
#include "World.h"

namespace uc
{
	class UOS_WORLD_LINKING CRectangleMenu : public CModel, public IMenu
	{
		public:
			CRectangleMenuSection *						Section = null;
			CWorldProtocol *									World;
			CSize 										Available;
			CStyle *									Style;

			UOS_RTTI
			CRectangleMenu(CWorldProtocol * w, CStyle * s, const CString & name = GetClassName());
			virtual ~CRectangleMenu();
	
			IMenuSection *								CreateSection(const CString & name = CRectangleMenuSection::GetClassName()); // default style section

			void										SetSection(IMenuSection * m);

			void										Open(CPick & pick, CSize const & size = CSize::Empty);
			void										Close();
			void										DetermineSize(CSize & smax, CSize & s) override;
			CTransformation								DetermineTransformation(CPositioning * ps, CPick & pk, CTransformation & t) override;

			void										OnStateModified(CActive * s, CActive *, CActiveStateArgs * a);
			void										OnMouse(CActive * r, CActive * s, CMouseArgs * a);

		protected:
			virtual void								Close(CUnit * a) override;
	};
}
