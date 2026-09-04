local global = require("global")

-- 命中阶段
local function _hit_attacker_before_hit(damage_info)
    if(damage_info.defender.tag:Check(global.enum['Battle.Tag'].Dead)) then
        damage_info.isHit = false;
        --print("defender is dead, attacker cannot hit")
    end
end

local function _hit_defender_before_hit(damage_info)
    
end

local function _hit_attacker_check_hit(damage_info)
    damage_info.isHit = math.random(1, 20) ~= 1;
end

local function _hit_attacker_after_hit(damage_info)
end

local function _hit_defender_after_hit(damage_info)
end

-- 计算阶段
local function _cal_attacker_before_cal(damage_info)
    --print(os.clock())
    --print("cal_attacker_before_cal", damage_info: ToString())
end

local function _cal_defender_before_cal(damage_info)
    --print(os.clock())
    --print("cal_defender_before_cal", damage_info: ToString())
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

local function _cal_attacker_check_derive(damage_info)
    --print(os.clock())
    --print("cal_attacker_check_derive", damage_info: ToString())
end

local function _cal_defender_check_derive(damage_info)
    --print(os.clock())
    --print("cal_defender_check_derive", damage_info: ToString())
end

local function _cal_attacker_after_cal(damage_info)
    --print(os.clock())
    --print("cal_attacker_after_cal", damage_info: ToString())
end

local function _cal_defender_after_cal(damage_info)
    --print(os.clock())
    --print("cal_defender_after_cal", damage_info: ToString())
end

-- 结算阶段
local function _final_attacker_before_final(damage_info)
    --print(os.clock())
    --print("final_attacker_before_final", damage_info: ToString())
end

local function _final_defender_before_final(damage_info)
    --print(os.clock())
    --print("final_defender_before_final", damage_info: ToString())
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

local function _final_attacker_derive(damage_info)
    --print(os.clock())
    --print("final_attacker_derive", damage_info: ToString())
end

local function _final_defender_derive(damage_info)
    --print(os.clock())
    --print("final_defender_derive", damage_info: ToString())
end

local function _final_attacker_after_final(damage_info)
    --print(os.clock())
    --print("final_attacker_after_final", damage_info: ToString())
end

local function _final_defender_after_final(damage_info)
    --print(os.clock())
    --print("final_defender_after_final", damage_info: ToString())
end

return {
    hit_attacker_before_hit = _hit_attacker_before_hit,
    hit_defender_before_hit = _hit_defender_before_hit,
    hit_attacker_check_hit = _hit_attacker_check_hit,
    hit_attacker_after_hit = _hit_attacker_after_hit,
    hit_defender_after_hit = _hit_defender_after_hit,

    cal_attacker_before_cal = _cal_attacker_before_cal,
    cal_defender_before_cal = _cal_defender_before_cal,
    cal_attacker_check_crit = _cal_attacker_check_crit,
    cal_attacker_check_extra_damage = _cal_attacker_check_extra_damage,
    cal_defender_check_defence = _cal_defender_check_defence,
    cal_attacker_check_derive = _cal_attacker_check_derive,
    cal_defender_check_derive = _cal_defender_check_derive,
    cal_attacker_after_cal = _cal_attacker_after_cal,
    cal_defender_after_cal = _cal_defender_after_cal,

    final_attacker_before_final = _final_attacker_before_final,
    final_defender_before_final = _final_defender_before_final,
    final_defender_evaluation = _final_defender_evaluation,
    final_defender_check_state = _final_defender_check_state,
    final_attacker_derive = _final_attacker_derive,
    final_defender_derive = _final_defender_derive,
    final_attacker_after_final = _final_attacker_after_final,
    final_defender_after_final = _final_defender_after_final,
}