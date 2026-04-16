羁绊AveMujica:
1.属于乐队羁绊。成员包括丰川祥子，三角初华，祐天寺若麦，八幡海玲，若叶睦。tag为AveMujica

2.羁绊激活时效果为：监听 

```
EventController.Instance.AddListener<Chess>(EventName.WhenPlantChess.ToString(), AddBuff);
```

AddBuff(Chess c):若c的tags包含AveMujica 则为其添加Buff_AveMujica;

维护int feverCurrent和int feverMax，当feverCurrent和feverMax触发时 场上所有AveMujica成员进入Fever状态（ChangeState(State.Fever)）并添加持续20s的不屈Buff。 fever状态期间fevercurrent不会增加，fever状态结束后15s也不会获得fever

3.Buff_AveMujica:

BuffEffect(Chess target):根据target的不同有不同的效果：

若为丰川祥子：注册OnTakeDamage事件，每次造成伤害使得fever+1

若为三角初华：注册OnTakeDamage(DamageMessage dm)事件。如dm的类型为Heal 且dm.target为AveMujica成员 使得fever+1

若为若叶睦：注册OnSetDamage:每次受到伤害时,使得fever+1

若为八幡海玲：注册onUseSkill 当使用技能时，使得fever+10

若为祐天寺若麦：Timer:time:每秒增加1点fever

4.FeverState:

进入状态时：播放animtor.Play("fever");

离开状态时：如果prestate是skillState 则ReturnCD()