#pragma once
#include "Link.h"

namespace uc
{
	class CLinkProperties : public CEnvironmentWindow
	{
		public:
			CObject<CLink>								Entity;
			CShellLevel *								Level;
			CRectangleSizer								Sizer;

			CRectangle *								Content;

			CText *										TitleText;
			CTextEdit *									TitleEdit;
			CText *										TargetText;
			CTextEdit *									TargetEdit;
			CText *										ExecutorText;
			CTextEdit *									ExecutorEdit;
			CTable *									Table;

			CButton *									OkButton;

			UOS_RTTI
			CLinkProperties(CShellLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CLinkProperties();

			void										SetEntity(CUol & e);
			void										OnDependencyDestroying(CBaseNexusObject * d);

			virtual void								SaveInstance() override;
			virtual void								LoadInstance() override;
			
			void										OnOK(CButton * b);

			virtual void UpdateLayout(CLimits const & l, bool apply) override;

			virtual void								DetermineSize(CSize & smax, CSize & s) override;
			virtual CTransformation						DetermineTransformation(CPositioning * ps, CPick & pk, CTransformation & t) override;
	};
}
