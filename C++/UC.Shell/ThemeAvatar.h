#pragma once
#include "ShellLevel.h"
#include "Theme.h"

namespace uc
{
	class CThemeAvatar : public CModel
	{
		public:
			CObject<CTheme>								Entity;

			CShellLevel *								Level;
			CElement *								Root = null;

			CFloat3										CameraPosition = CFloat3(0);
			
			UOS_RTTI
			CThemeAvatar(CShellLevel  * l, const CString & name = CGuid::Generate64(GetClassName()));
			~CThemeAvatar();

			virtual void								SetEntity(CUol & e) override;
			void										OnDependencyDestroying(CNexusObject *);
			virtual void								SaveInstance() override;
			virtual void								LoadInstance() override;

			void										LoadScene();

			void virtual								UpdateLayout(CLimits const & l, bool apply) override;

			virtual void								DetermineSize(CSize & smax, CSize & s) override;
			virtual CTransformation						DetermineTransformation(CPositioning * ps, CPick & pk, CTransformation & t) override;
	};
}
