#pragma once
#include "Field.h"

namespace uc
{
	class CBoard : public CFieldServer
	{
		public:
			CShellLevel	*	Level;

			UOS_RTTI
			CBoard(CShellLevel * l, CString const & name = CGuid::Generate64(GetClassName())) : CFieldServer(l, name), CWorldEntity(l->Server, name)  
			{
				Level = l;
				Level->World->UnitOpened += ThisHandler(OnModelOpened);
			}

			~CBoard()
			{
				Level->World->UnitOpened -= ThisHandler(OnModelOpened);
			}

			void OnModelOpened(CUnit * a, CTransformation & t, CShowParameters * f)
			{
				if(Level->World->FieldArea->ContainsDescedant(a) || Level->World->MainArea->ContainsDescedant(a))
				{
					if(f && f->PlaceOnBoard)
					{
						auto fi = Items.Find([a](auto i){ return a->Entity.Url == i->Object; });

						if(!fi)
						{
							Add(a->Entity.Url, AVATAR_ICON2D);
						}
					}
				}
			}
	};
}
