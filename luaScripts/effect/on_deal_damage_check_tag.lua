local global = require("global")
local order = 1;

function on_deal_damage_check_tag(damage_info)
    local is_damage_nullified = damage_info.defeder.tag.Contains();
end