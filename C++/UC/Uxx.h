#pragma once
#include "Url.h"

namespace uc
{
	/// <summary>
	/// {protocol=worldentity realization=uc.experimental.winx64 server=Experimental host=127.0.0.1 class=UC.Experimental.Commander id=4444444444444444444444444444444}
	/// {protocol=worldentity server=Experimental class=UC.Experimental.Commander id=4444444444444444444444444444444}
	/// {protocol=worldentity class=UC.Experimental.Commander id=4444444444444444444444444444444}
	/// 
	/// worldentity://127.0.0.1/uc/experimental/winx64/0.0.0.0/Experimental/Experimental0/UC.Experimental.Commander/4444444444444444444444444444444
	/// worldentity://///////UC.Experimental.Commander/4444444444444444444444444444444
	///	worldentity:///UC.Experimental.Browser/4444444444444444444444444444444
	///	worldentity:///UC.Experimental.Earth/4444444444444444444444444444444
	/// </summary>

	class UOS_LINKING CUol : public ISerializable // universal object locator
	{
		public:
			CString										Scheme;
			CString										Server;
			CString										Object;
			CMap<CString, CString>						Parameters;

			const static std::wstring					TypeName;

			CUol();
			CUol(CString const & protocol, CString const & server, CString const & object);
			CUol(CString const & protocol, CString const & server, CString const & object, CMap<CString, CString> & parameters);
			CUol(const CUrl & addr);

			static CString								GetObjectClass(CString const & name);
			static CString								GetObjectId(CString const & name);

			bool static									IsValid(const CUrl & u);
			bool										IsEmpty() const;
			CString										ToString() const;
			CString										GetObjectClass();
			CString										GetObjectId();

			bool										operator!= (const CUol & a) const;
			bool										operator== (const CUol & a) const;
			CUol &										operator=  (CUrl & addr);
			operator									CUrl() const;
			
			/// ISerializable
			virtual std::wstring						GetTypeName() override;
			virtual void								Read(CStream * s)  override;
			virtual int64_t								Write(CStream * s) override;
			virtual void								Write(std::wstring & s) override;
			virtual void								Read(std::wstring const & addr) override;
			virtual ISerializable *						Clone() override;
			virtual bool								Equals(const ISerializable & a) const override;
	};


}
