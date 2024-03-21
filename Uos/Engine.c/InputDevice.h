#pragma once
#include "Screen.h"

namespace uc
{
	enum class EInputClass
	{
		Null, Mouse, Keyboard, TouchScreen
	};
	
	class CInputDevice : public virtual IType
	{
		public:
			UOS_RTTI
			virtual ~CInputDevice(){}
	};

	class UOS_ENGINE_LINKING CInputMessage : public CShared, public IType
	{
		public:
			int				Id;
			EInputClass		Class;

			UOS_RTTI
	};
}