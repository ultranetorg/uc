#pragma once
#include "String.h"

namespace uc
{
	class CDiagGrid;

	struct UOS_LINKING CDiagGridRow
	{
		CArray<CString>		Cells;
		CArray<DWORD>		Colors;
		CDiagGrid *			Grid;
		int					NextCell = 0;

		void				SetValue(int r, const CString & value);
		void				SetNext(const CString & value);

		template<typename... Args>	void SetNext(const std::wstring& format, Args const & ... args)
		{
			Cells[NextCell].Printf(format, args ...);
			Colors[NextCell] = 0;
			Grid->Columns[NextCell].MaxWidth = max(Grid->Columns[NextCell].MaxWidth, Cells[NextCell].size());
			NextCell++;
		}

		template<typename... Args>	void SetNext(DWORD c, const std::wstring& format, Args const & ... args)
		{
			Cells[NextCell].Printf(format, args ...);
			Colors[NextCell] = c;
			Grid->Columns[NextCell].MaxWidth = max(Grid->Columns[NextCell].MaxWidth, Cells[NextCell].size());
			NextCell++;
		}

		CDiagGridRow(){}
		CDiagGridRow(CDiagGrid * g) : Grid(g)
		{
			Cells.reserve(10);
		}	
	}; 

	struct UOS_LINKING CDiagGridColumn
	{
		CString				Text;
		CString				Format;
		size_t				MaxWidth = 0;
		CDiagGrid *			Grid;

		CDiagGridColumn(){}
		CDiagGridColumn(CDiagGrid * g) : Grid(g){}	
	}; 

	class UOS_LINKING CDiagGrid 
	{
		public:
			CDiagGridColumn &							AddColumn(const CString & name, const CString & f = L" %%-%ds ");
			CDiagGridRow &								AddRow();
			int											GetSize(){ return int(2 + Used); } // 2 = column names + next divider
			void Clear();

			CArray<CDiagGridColumn>						Columns;
			CArray<CDiagGridRow>						Rows;
			int											Used = 0;

			CDiagGrid();
			~CDiagGrid();
		
		private:
	};
}
