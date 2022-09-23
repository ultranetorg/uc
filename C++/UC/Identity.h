#pragma once
#include "ApplicationAddress.h"

namespace uc
{
	class CUserStorageProvider
	{
		public:
			CString					Name = L"LocalStorage";
			CApplicationAddress		StorageProvider = CApplicationAddress(L"uc", L"uos", L"FileSystem");
	};

	class CIdentity
	{
		public:
			CString					User = L"Guest";
			CString					Password;
			CUserStorageProvider *	Provider;

			CString ToString()
			{
				return User + L"-" + Provider->Name;
			}
	};
}

