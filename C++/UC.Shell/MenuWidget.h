#pragma once
#include "ShellLevel.h"
#include "DirectoryMenu.h"

namespace uc
{
	class CMenuWidget;

	class CMenuWidgetSectionItem : public CRectangleSectionMenuItem
	{
		public:
			CWorldProtocol *									World;
			CRectangleMenu *							Menu = null;
			CMenuWidget *								Widget;

			bool										ClosingByWidget = false;

			UOS_RTTI
			CMenuWidgetSectionItem(CWorldProtocol * w, CMenuWidget * mw, const CString & name = GetClassName());
			~CMenuWidgetSectionItem();

			virtual void								Highlight(CArea * a, bool e, CSize & s, CPick * p) override;
	};

	class CMenuWidget : public CAvatar, public CFieldable
	{
		public:
			CShellLevel *									Level;
			CRectangleMenuSection *							Section = null;
			CRectangleMenu *								Menu = null;
			CObject<CDirectoryMenu>							Entity;
			CList<CProtocolConnection<IStorage>>	FileSystems;
			CList<CProtocolConnection<CImageExtractorProtocol>>		FileExtractor;
						
			UOS_RTTI
			CMenuWidget(CShellLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			virtual ~CMenuWidget();
			
			void											SetEntity(CUol & d);
			
			void											OnDependencyDestroying(CInterObject * o);

			void											OnStateModified(CActive *, CActive *, CActiveStateArgs *);
			void											OnMouse(CActive *, CActive *, CMouseArgs *);

			virtual void									SaveInstance() override;
			virtual void									LoadInstance() override;

			virtual IMenuSection *							CreateSection(const CString & name = L"MenuSection")/*override*/;
			void											AddMenuWidgetSectionItem(CMenuItem * i);
			CMenuWidgetSectionItem *						AddMenuWidgetSectionItem(CString const & name);

			void											DetermineSize(CSize & smax, CSize & s) override;

	};
}
