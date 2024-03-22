#pragma once
#include "ActiveSpace.h"
#include "InputSystem.h"

namespace uc
{
	class CEventNode
	{
		public:
			CRefList<CActive *>		Receivers;
			CActive *				Source;
			CArray<CEventNode *>	Related;
			CActiveArgs *			Args;

			CEventNode(CActive * s, CActiveArgs * a) :	Source(s ? (CActive *)s->Take() : null), 
														Args(a ? (CActiveArgs *)a->Take() : null)
			{
			} 

			~CEventNode()
			{
				if(Args)
					Args->Free();

				if(Source)
					Source->Free();

				for(auto i : Related)
					delete i;
			}

			CEventNode * AddRelated(CActive * s, CActiveArgs * a)
			{
				auto e = new CEventNode(s, a);
				Related.push_back(e);
				return e;
			}
	};

	class UOS_ENGINE_LINKING CActiveGraph : public CActiveSpace
	{
		public:
			CEngineLevel *								Level;
			CString										Name;
			CActive *									Focus = null;
			CActive *									HoverFocus = null;
			CActive *									Root;
			CNodeCapture *								Capture = null;
			CMap<int, CPick>							Picks;
			CDiagnostic *								Diagnostic;
			CDiagGrid									DiagGrid;
			CPick										LastPick;

			#ifdef _DEBUG
			CMap<CActiveSpace *, CList<CPick>>			_Intersections;
			#endif

			UOS_RTTI
			CActiveGraph(CEngineLevel * l, const CString & name);
			~CActiveGraph();

			void										Pick(CScreen * sc, CFloat2 & sp, CActiveSpace * s, CPick * cis, CPick * nis);
			void										Pick(CScreen * sc, CFloat2 & sp, CActiveSpace * as, CActive * root, CActive * n, CCamera * c, CFloat2 & vpp, CRay & r, CPick * cis, CPick * nis);
			void										Pick(CActiveSpace * as, CActive * n, CFloat2 & sp, CCamera * c, CFloat2 & vpp, CRay & r, CPick * nis);

			CPick										ReversePick(CCamera * c, CActiveSpace * s, CActive * a, CFloat3 & p);

			CEventNode *								Fire(CPick & pk, CInputMessage * m, EGraphEvent type, CActive * n);
			void										FireRelated(CEventNode * e, CPick & pk, CInputMessage * m, EGraphEvent type, CActive * n);
			void										On(CEventNode * pevent, CPick & pk, CInputMessage * m);
			void										Off(CEventNode * pevent, CPick & pk, CInputMessage * m);
			void										MovePrimary(CEventNode * pevent, CPick & pk, CInputMessage * m);
			void										ProcessMouse(CMouseInput * m, CPick & pk);
			void										ProcessKeyboard(CKeyboardInput * m);
			void										ProcessTouch(CTouchInput * m, CMap<int, CPick> & pks);
		
			void										Propagate(CEventNode * e, CActive * s, EGraphEvent type); 

			void										OnNodeDetach(CActive * removed);
			
			void										CallEvent(CEventNode * e);

			CActive *									Activate(CActive * f);
			CActive *									Activate(CEventNode * pevent, CActive * newNode, CInputMessage * m);
			
			void										OnDiagnosticsUpdate(CDiagnosticUpdate & a);


	};
}