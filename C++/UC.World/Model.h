#pragma once
#include "Avatar.h"
#include "Positioning.h"

namespace uc
{
	class IAvatarProtocol;
	class CUnit;
	class CWorld;

	enum class ELifespan
	{
		Null, Visibility, Session, Permanent
	};

	enum class EModelAction
	{
		Null, Positioning, Transfering
	};
	
	enum class EPreferedPlacement
	{
		Null, Default, Exact, Convenient
	};

	struct CShowParameters : public CExecutionParameters
	{
		CPick			Pick;
		CActiveArgs * 	Args = null;
		CAnimation 		Animation;
		bool			PlaceOnBoard = false;
		bool			Activate = true;
		bool			Maximize = false;

		CShowParameters(){}
		CShowParameters(CPick & p) : Pick(p) {}
		CShowParameters(CInputArgs * arg, CStyle * s)
		{
			if(auto a = arg->As<CMouseArgs>())
			{
				Pick = a->Pick;
				PlaceOnBoard = a->Control == EMouseControl::MiddleButton;
			}
			if(auto a = arg->As<CTouchArgs>())
			{
				Pick = a->GetPick();
			}

			Animation = s->GetAnimation(L"Animation");
			Args = arg;
			Args->Take();
		}

		~CShowParameters()
		{
			if(Args)
				Args->Free();
		}
	};

	struct CHideParameters : public CExecutionParameters
	{
		CActiveArgs * 		Args = null;
		CAnimation 			Animation;
		CTransformation		End = CFloat3::Nan;

		CHideParameters(){}
		CHideParameters(CActiveArgs * a, CStyle * s)
		{
			Args = a;
			Args->Take();
			Animation = s->GetAnimation(L"Animation");
		}

		~CHideParameters()
		{
			if(Args)
				Args->Free();
		}
	};
	
	class UOS_WORLD_LINKING CModel : public CAvatar
	{
		public:
			ELifespan									Lifespan;
			CUnit *										Unit = null;
			CWorldCapabilities *						Capabilities = null;
			bool										UseHeader = false;
			CMap<CString, EPreferedPlacement>			PreferedPlacement;
			CList<CString>								Tags = {L"Apps"};

			UOS_RTTI
			CModel(CWorld * l, CServer * s, ELifespan life, CString const & name);
			virtual ~CModel();

			using CAvatar::UpdateLayout;

			virtual void								Open(CWorldCapabilities * caps, CUnit * a);
			virtual	void								Close(CUnit * a);

			void										DetermineSize(CSize & smax, CSize & s) override;
			virtual CTransformation						DetermineTransformation(CPositioning * ps, CPick & pk, CTransformation & t);
			EPreferedPlacement							GetPreferedPlacement();

			void										UpdateLayout(CLimits const & l, bool apply) override;

	};
}
