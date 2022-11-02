#pragma once
#include "Xon.h"

namespace uc
{
	class UOS_LINKING CXonTextWriter : public IXonWriter
	{
		public:
			bool										IsWriteTypes = false;
			CStream *									OutStream = null;
			bool										Eol = true;

			void										Write(CXon * s) override;
			void										Write(std::wostringstream & s, CXon * n, int d);

			CXonTextWriter();
			CXonTextWriter(CStream * s)
			{
				OutStream = s;
			}
			CXonTextWriter(CStream * s, bool writeTypes) : IsWriteTypes(writeTypes)
			{
				OutStream = s;
			}
//			CXonTextWriter(bool writeTypes) : IsWriteTypes(writeTypes){}
			~CXonTextWriter(){}

	};

	class UOS_LINKING CXonBinaryWriter : public IXonWriter
	{
		public:
			CStream * Stream;
	
			virtual void								Write(CXon * s) override;

			CXonBinaryWriter(CStream * s);
			~CXonBinaryWriter(){}

		private:

	};
}
