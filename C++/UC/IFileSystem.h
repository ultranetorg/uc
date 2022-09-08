#pragma once
#include "IFileSystemProvider.h"

namespace uc
{
	class CMappingExcepion : public CException
	{
		public:
			CMappingExcepion(wchar_t const * m, int l)  : CException(m, l, L"")
			{
			}
	};

	class IFileSystem : public virtual IInterface
	{
		public:
			const static inline CString		This = L"/This";
			const static inline CString		System = L"/System";
			const static inline CString		SystemTmp = L"/System/Tmp";
			const static inline CString		Servers = L"/Servers";
			const static inline CString		Software = L"/Software";
			const static inline CString		UserGlobal	= L"/User/Global";
			const static inline CString		UserLocal = L"/User/Local";
			const static inline CString		UserTmp = L"/User/Tmp";

			inline static const CString   	InterfaceName = L"IFileSystem";

			virtual void					Mount(CString const & path, CApplicationAddress & provider, CXon * parameters)=0;
			virtual CString					UniversalToNative(CString const & path)=0;
			virtual CString					NativeToUniversal(CString const & path)=0;
			virtual CList<CFileSystemEntry>	Enumerate(CString const & dir, CString const & regex)=0;
			virtual void					CreateDirectory(CString const & path)=0;
			virtual IDirectory *			OpenDirectory(CString const & path)=0;
			virtual CStream *				WriteFile(CString const & path)=0;
			virtual CStream *				ReadFile(CString const & path)=0;
			virtual CAsyncFileStream *		ReadFileAsync(CString const & path)=0;
			virtual void					Close(CStream *)=0;
			virtual void					Close(IDirectory *) = 0;
			virtual void					Delete(CString const & path)=0;
			virtual bool					Exists(CString const & name)=0;
			virtual CString					GetType(CString const & path)=0;
			virtual CUol					ToUol(CString const & path) = 0;

			virtual							~IFileSystem(){}
	};
}


