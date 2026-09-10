local global = require("global")

-- 命中阶段
local function _hit_attacker_before_hit(damage_info)
    if(damage_info.defender.tag:Check(global.enum['Battle.Tag'].Dead)) then
        damage_info.isAbort = true;
        --print("defender is dead, attacker cannot hit")
    end
end

local function _hit_attacker_check_hit(damage_info)
    damage_info.isHit = math.random(1, 20) ~= 1;
end


local function _cal_attacker_check_crit(damage_info)
    damage_info.isCrit = math.random(1, 20) == 20;
end

local function _cal_attacker_check_extra_damage(damage_info)
    for i = 1, damage_info.damage.Count - 1 do
        local d = damage_info.damage:get_Item(i)
        if d.type == global.enum['Battle.DamageType'].Physical then
            d.addValue = 5
        elseif d.type == global.enum['Battle.DamageType'].Magical then
            d.mulValue = 1.2
        end
    end
end

local function _cal_defender_check_defence(damage_info)
    for i = 1, damage_info.damage.Count - 1 do
        local d = damage_info.damage:get_Item(i)
        if d.type == global.enum['Battle.DamageType'].Physical then
            d.deductionRatio = 0.2
        elseif d.type == global.enum['Battle.DamageType'].Magical then
            d.deductionValue = 5
        end
    end
end

local function _final_defender_evaluation(damage_info)
    local deResource = damage_info.defender.resource

    for i = 1, damage_info.damage.Count - 1 do
        local d = damage_info.damage:get_Item(i)
        local v = (d.value * d.mulValue + d.addValue) * (1 - d.deductionRatio) - d.deductionValue
        deResource: ChangeValue(global.enum['Battle.Attribute'].Health, -v)
    end
end

local function _final_defender_check_state(damage_info)
    local dead = damage_info.defender.resource:Get(global.enum['Battle.Attribute'].Health) <= 0
    damage_info.defender.tag: Add(global.enum['Battle.Tag'].Dead)
end

return {
    hit_attacker_before_hit = _hit_attacker_before_hit,
    hit_attacker_check_hit = _hit_attacker_check_hit,

    cal_attacker_check_crit = _cal_attacker_check_crit,
    cal_attacker_check_extra_damage = _cal_attacker_check_extra_damage,
    cal_defender_check_defence = _cal_defender_check_defence,

    final_defender_evaluation = _final_defender_evaluation,
    final_defender_check_state = _final_defender_check_state,
}