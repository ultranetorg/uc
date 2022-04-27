#pragma once
#include "Active.h"
#include "View.h"

namespace uc
{
	class CActiveGraph;

	class UOS_ENGINE_LINKING CActiveSpace : public virtual CShared, public virtual IType
	{
		public:
			CString										Name;
			CString										ParentName;
			CActiveSpace *								Parent = null;
			CList<CActiveSpace *>						Spaces;
			CPerformanceCounter							PcRendering;
			CMatrix										Matrix = CMatrix::Identity;
			CView *										View = null;
			CRefList<CActive *>							Actives;
			CActiveGraph *								Graph = null;

			UOS_RTTI
			CActiveSpace(const CString & n);
			~CActiveSpace();
			
			void										SetGraph(CActiveGraph * g);

			void										AddFront(CActiveSpace * s);
			void										AddBack(CActiveSpace * s);
			void										AddBefore(CActiveSpace * s, CActiveSpace * p);
			void										AddAfter(CActiveSpace * s, CActiveSpace * p);
			void										Insert(CActiveSpace * s, CList<CActiveSpace *>::iterator pos);
			void										Remove(CActiveSpace * s);

			CActiveSpace *								Find(const CString & name);
			bool										IsDescedant(CActiveSpace * s);
			bool										IsUnder(CActiveSpace * parent);
			
			void										SetView(CView * v);
			CView *										GetView();
			CView *										GetActualView();

			void										Save(CXon * n);
			void										Load(CXon * n);
		
			void AddActive(CActive * v);
			void RemoveActive(CActive * v);

			template<class P> CActiveSpace * FindAncestor(P p)
			{
				auto n = this;

				while(n && !p(n))
				{
					n = n->Parent;
				}

				return n;
			}

	};
}
