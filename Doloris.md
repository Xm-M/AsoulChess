Doloris:

1.被动技能Passive_Mujica_Doloris：每秒为攻击范围内的所有友军施加Buff_Mujica_Doloris（ damageRate取30%）和Buff_Bard（数值为10%攻击力）；每3s为攻击范围内受伤且生命值最低的友军治疗相当于自身攻击力30%的生命值（如果范围内存在受伤的丰川祥子，则优先以其为目标，且治疗效果增加25%）；
2.主动技能SkillEffect_Mujica_Doloris:为范围内的友军施加Buff_BaseValueBuff_TimeValueBuff;对攻击范围内的随机一名敌人造成攻击力×config.damages[0]的伤害；并治疗一名受伤友军受伤且生命值最低的友军治疗相当于自身攻击力×config.damages[0]的生命值（如果范围内存在受伤的丰川祥子，则优先以其为目标，且治疗效果增加25%）；
关于Buff:

Buff_Mujica_Doloris:

属性：public Chess buffFrom:buff来源

public float damageRate;伤害倍率

BuffEffect():target.OnSetDamage监听SetDamage;

SetDamage(DamageMassage DM):buffFrom对DM damageTo 造成 damageRate*buffFrom攻击力的魔法伤害

BuffReset:如果damageRate数值更好 则替换buffFrom和damageRate;

Buff_Bard：

属性：Timer:time 

public float heal;

BuffEffect():time=每秒治疗target heal;

BuffReset():取heal最高值 