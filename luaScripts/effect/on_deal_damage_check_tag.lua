local global = require("global")
local order = 1;

function on_deal_damage_check_tag(damage_info)
    local is_all_damage_nullified = damage_info.defeder.tag.Contains(global.enum["Battle.Tag"].ImmuneAllDamage);
    if is_all_damage_nullified then
        for dmg in damage_info.damage do
            dmg.value = 0;
        end
    end
end