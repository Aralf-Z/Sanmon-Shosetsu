namespace Sanmon.Battle
{
    /// <summary> 伤害管线事件 </summary>
    public static class DealDamageEvent
    {
        // ============================================================
        // 命中阶段
        // ============================================================
        
        /// <summary> 命中阶段-攻击者-命中前 </summary>
        public const string HIT_ATTACKER_BEFORE_HIT = "hit_attacker_before_hit";
        
        /// <summary> 命中阶段-受击者-命中前 </summary>
        public const string HIT_DEFENDER_BEFORE_HIT = "hit_defender_before_hit";

        /// <summary> 命中阶段-攻击者-命中判断 </summary>
        public const string HIT_ATTACKER_CHECK_HIT = "hit_attacker_check_hit";

        /// <summary> 命中阶段-攻击者-命中后 </summary>
        public const string HIT_ATTACKER_AFTER_HIT = "hit_attacker_after_hit";

        /// <summary> 命中阶段-受击者-命中后 </summary>
        public const string HIT_DEFENDER_AFTER_HIT = "hit_defender_after_hit";
        
        // ============================================================
        // 计算阶段
        // ============================================================

        /// <summary> 计算阶段-攻击者-计算前 </summary>
        public const string CAL_ATTACKER_BEFORE_CAL = "cal_attacker_before_cal";
        
        /// <summary> 计算阶段-受击者-计算前 </summary>
        public const string CAL_DEFENDER_BEFORE_CAL = "cal_defender_before_cal";

        /// <summary> 计算阶段-攻击者-暴击计算 </summary>
        public const string CAL_ATTACKER_CHECK_CRIT = "cal_attacker_check_crit";

        /// <summary> 计算阶段-攻击者-额外伤害计算 </summary>
        public const string CAL_ATTACKER_CHECK_EXTRA_DAMAGE = "cal_attacker_check_extra_damage";

        /// <summary> 计算阶段-受击者-抵消、减伤等计算 </summary>
        public const string CAL_DEFENDER_CHECK_DEFENCE = "cal_defender_check_defence";

        /// <summary> 计算阶段-攻击者-额外效果, 如吸血、伤害衍生 </summary>
        public const string CAL_ATTACKER_CHECK_DERIVE = "cal_attacker_check_derive";
        
        /// <summary> 计算阶段-防御者-额外效果, 如反伤、受伤后启动护盾 </summary>
        public const string CAL_DEFENDER_CHECK_DERIVE = "cal_defender_check_derive";

        /// <summary> 计算阶段-攻击者-计算后 </summary>
        public const string CAL_ATTACKER_AFTER_CAL = "cal_attacker_after_cal";

        /// <summary> 计算阶段-防御者-计算后 </summary>
        public const string CAL_DEFENDER_AFTER_CAL = "cal_defender_after_cal";

        // ============================================================
        // 结算阶段
        // ============================================================

        /// <summary> 结算阶段-攻击者-结算前 </summary>
        public const string FINAL_ATTACKER_BEFORE_FINAL = "final_attacker_before_final";
        
        /// <summary> 结算阶段-受击者-结算前 </summary>
        public const string FINAL_DEFENDER_BEFORE_FINAL = "final_defender_before_final";

        /// <summary> 结算阶段-受击者-赋值, 扣血，扣盾什么的 </summary>
        public const string FINAL_DEFENDER_EVALUATION = "final_defender_evaluation";

        /// <summary> 结算阶段-受击者-状态检测, 死亡、重生等 </summary>
        public const string FINAL_DEFENDER_CHECK_STATE = "final_defender_check_state";
        
        /// <summary> 结算阶段-攻击者-额外效果生效 </summary>
        public const string FINAL_ATTACKER_DERIVE = "final_attacker_derive";

        /// <summary> 结算阶段-防御者-额外效果生效 </summary>
        public const string FINAL_DEFENDER_DERIVE = "final_defender_derive";
        
        /// <summary> 结算阶段-攻击者-结算后 </summary>
        public const string FINAL_ATTACKER_AFTER_FINAL = "final_attacker_after_final";

        /// <summary> 结算阶段-防御者-结算后 </summary>
        public const string FINAL_DEFENDER_AFTER_FINAL = "final_defender_after_final";
    }
}