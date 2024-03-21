#pragma once
#include "targetver.h"
#include "Assembly.h"

#include <include/cef_app.h>
#include <include/cef_browser.h>
#include <include/cef_client.h>
#include <include/cef_version.h>

#include <nlohmann/json.hpp>

#include <vmime/vmime/vmime.hpp>
#include <vmime/vmime/platforms/posix/posixHandler.hpp>

#include "../../Shell/Shell/Include.h"

namespace uc
{
	auto constexpr	COMMANDER_1			= L"Commander-11111111111111111111111111111111";
	auto constexpr	EARTH_1				= L"Earth-11111111111111111111111111111111";
	auto constexpr	BROWSER_1			= L"Browser-11111111111111111111111111111111";
	auto constexpr	GROUP_1				= L"Group-11111111111111111111111111111111";

	auto constexpr	COMMANDER_AT_HOME_1 = L"Commander-22222222222222222222222222222222";
	auto constexpr	COMMANDER_AT_HOME_2 = L"Commander-33333333333333333333333333333333";
		
	auto constexpr	BROWSER_AT_GROUP	= L"Browser-4444444444444444444444444444444";
	auto constexpr	COMMANDER_AT_GROUP	= L"Commander-4444444444444444444444444444444";
	auto constexpr	EARTH_AT_GROUP		= L"Earth-4444444444444444444444444444444";
}

