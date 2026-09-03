local global = require("global")

-- 命中阶段
local function _hit_attacker_before_hit(damage_info)
    --print(os.clock())
    --print("hit_attacker_before_hit", damage_info: ToString())
end

local function _hit_defender_before_hit(damage_info)
    --print(os.clock())
    --print("hit_defender_before_hit", damage_info: ToString())
end

local function _hit_attacker_check_hit(damage_info)
    --damage_info.isHit = true
    --print(os.clock())
    --print("hit_attacker_check_hit", damage_info: ToString())
end

local function _hit_attacker_after_hit(damage_info)
    --print(os.clock())
    --print("hit_attacker_after_hit", damage_info: ToString())
end

local function _hit_defender_after_hit(damage_info)
    --print(os.clock())
    --print("hit_defender_after_hit", damage_info: ToString())
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
    --print(os.clock())
    --print("cal_attacker_check_crit", damage_info: ToString())
end

local function _cal_attacker_check_extra_damage(damage_info)
    --print(os.clock())
    --print("cal_attacker_check_extra_damage", damage_info: ToString())
end

local function _cal_attacker_check_ratio(damage_info)
    --print(os.clock())
    --print("cal_attacker_check_ratio", damage_info: ToString())
end

local function _cal_defender_check_defence(damage_info)
    --print(os.clock())
    --print("cal_defender_check_defence", damage_info: ToString())
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
    --print(os.clock())
    --print("final_defender_evaluation", damage_info: ToString())
end

local function _final_defender_check_state(damage_info)
    --print(os.clock())
    --print("final_defender_check_state", damage_info: ToString())
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
    cal_attacker_check_ratio = _cal_attacker_check_ratio,
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