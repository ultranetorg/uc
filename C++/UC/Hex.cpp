#include "stdafx.h"
#include "Hex.h"

using namespace uc;

int char2int(char input)
{
    if(input >= '0' && input <= '9')
        return input - '0';
    if(input >= 'A' && input <= 'F')
        return input - 'A' + 10;
    if(input >= 'a' && input <= 'f')
        return input - 'a' + 10;
    throw std::invalid_argument("Invalid input string");
}

CArray<byte> CHex::ToBytes(CString const & text)
{
    assert(text.size() % 2 == 0);

    auto & o = CArray<byte>(text.length() / 2);
    
    auto target = o.data();
    auto src = text.data();

    while(*src && src[1])
    {
        *(target++) = char2int(*src)*16 + char2int(src[1]);
        src += 2;
    }

    return o;
}

CString CHex::ToString(CArray<byte> & bytes)
{
    auto n = bytes.size();
    CString s(n/2, L'0');
    auto xp = s.data();
    auto bb = bytes.data();

    const char xx[]= "0123456789ABCDEF";
    while (--n >= 0) 
        xp[n] = xx[(bb[n>>1] >> ((1 - (n&1)) << 2)) & 0xF];

    return s;
}