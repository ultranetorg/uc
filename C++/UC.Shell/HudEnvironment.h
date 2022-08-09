#pragma once
#include "HudFieldElement.h"

namespace uc
{
	class CHudEnvironment : public CModel
	{
		public:
			CShellLevel *								Level;
			CHudFieldElement *							FieldElement;
			CObject<CFieldServer>						Entity;

			UOS_RTTI
			CHudEnvironment(CShellLevel * l, CString const & name = CGuid::Generate64(GetClassName()));
			~CHudEnvironment();

			void										SetEntity(CUol & e) override;

			virtual void								DetermineSize(CSize & smax, CSize & s) override;
			virtual CTransformation						DetermineTransformation(CPositioning * ps, CPick & pk, CTransformation & t) override;
			virtual bool								StartWorldOperation(EModelAction a, CActive * n){ return a != EModelAction::Positioning; }

			void										OnDependencyDestroying(CBaseNexusObject *);

			virtual void								SetDirectories(CString const & dir) override;
			virtual void								SaveInstance() override;
			virtual void								LoadInstance() override;



	};
}
