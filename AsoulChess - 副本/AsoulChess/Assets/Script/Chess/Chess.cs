using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
public class Chess : MonoBehaviour
{
    public int instanceID;
    public Weapon equipWeapon;
    public StateController stateController;
    public SkillController skillController;
    public ItemController itemController;
    public PropertyController propertyController;
    public BuffController buffController;
    
    public Tile standTile;//
    public Tile touchTile;
    public Chess target;
    public Animator animator;
    public bool unAttackable;
    public bool unSelectable;
    public bool ifMove = false;
    public bool FacingRight=true;
    public List<Fetter> fetters;
    public List<string> fetterNames;
    public bool ifDeath;
    public bool silence;
    
    protected virtual void Awake()
    {
        buffController=new BuffController(this);
        AddFetter();
        stateController = GetComponent<StateController>();
        stateController.InitStateController();
        animator = GetComponent<Animator>();    
        //if(skillController==null)skillController=new SkillController(this,skill);
        if(itemController==null)itemController=new ItemController(this);
        EventController.Instance.AddListener(EventName.RestartGame.ToString(), Death);
        EventController.Instance.AddListener(EventName.GameStart.ToString(), skillController.EnterGame);     
        //property.chess = this;
        ResetAll();
    }
    public void AddFetter()
    {
        fetters = new List<Fetter>();
        if (fetterNames == null) return;
        
        
        foreach(var fetterName in fetterNames)
        {
            Type t=Type.GetType(fetterName);
            Fetter f = Activator.CreateInstance(t) as Fetter;
            if (f == null) Debug.LogError("Non Fetter");
            else
            {
                f.self = this;
                f.Start();
                fetters.Add(f);
            }
        }
    }
    public void FindTarget( )
    {
        Chess chess=null;
        int minDistance;
        List<Chess> enemyTeam=ChessFactory.instance.FindEnemyList(tag);
        if (enemyTeam.Count > 0)
        {
            chess =null;
            minDistance = 100;
            for (int i = 0; i < enemyTeam.Count; i++){
                int dis=MapManage.instance.Distance(standTile,enemyTeam[i].standTile);
                if (!enemyTeam[i].ifDeath&&!enemyTeam[i].unSelectable&&dis < minDistance)
                {
                    minDistance = dis;
                    chess = enemyTeam[i];
                }else  if(dis==minDistance&& MapManage.instance.RealDis(chess.standTile.mapPos,standTile.mapPos)>
                    MapManage.instance.RealDis(enemyTeam[i].standTile.mapPos,standTile.mapPos)) {
                    chess=enemyTeam[i];
                }
            }
        }
        target = chess;
    }

    public void Update()
    {
        Flap();
        buffController.Update();
    }
    public void LateUpdate()
    {
        buffController.LateUpdate();
    }
    public void Flap()
    {
        if (target) 
        if ((FacingRight && target.transform.position.x < transform.position.x) || (!FacingRight && target.transform.position.x > transform.position.x))
        {
            transform.Rotate(0, 180, 0);
                //property.transform.Rotate(0, 180, 0);
            FacingRight = !FacingRight;
        }
    }
    public void ResetAll()
    {
        target = null;
        //property.ReSet();
 
        foreach(var fetter in fetters)fetter.ResetFetter();
        if(buffController!=null)
        buffController.ResetList();
        itemController.ResetAllItem();
        stateController.LeaveWar();
        ifMove = false;
        ifDeath = false;
        silence = false;
        EventController.Instance.TriggerEvent<Chess>(instanceID+EventName.ResetChess.ToString(),this);
    }
    public virtual void Death()
    {
        if (ifDeath == true) return;       
        stateController.LeaveWar();
        if (standTile)
        {
            standTile.ChessLeave();
            standTile = null;
        }
        //property.ReSet();
 
        ifDeath = true;       
        ChessFactory.instance.RecycleChess(this);
        EventController.Instance.TriggerEvent<Chess>(instanceID + EventName.WhenDeath.ToString(),this);
        GameManage.instance.GameOver(tag);
    }
    private void OnDestroy()
    {
        EventController.Instance.TriggerEvent(instanceID.ToString()+EventName.WhenChessDestroy.ToString());
        EventController.Instance.RemoveListener(EventName.RestartGame.ToString(), Death);
        EventController.Instance.RemoveListener(EventName.GameStart.ToString(), skillController.EnterGame);
    }
    IEnumerator ColorChange(float time)
    {
        float times = 0;
        GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount", 1);
        while (times < time)
        {
            times += Time.deltaTime;
            yield return null;
        }
        GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount", 0);
    }
        public void OnMouseDrag()
    {
        if(!CompareTag("Player"))return;
        GameManage.instance.HandChess=this;
        Vector2 mousePos = GameManage.instance.mainCamera.ScreenToWorldPoint(Input.mousePosition);
        transform.position = mousePos;  
    }
    private void OnMouseDown()
    {
        //UIManage.instance.ShowChessMessage(this);
        EventController.Instance.TriggerEvent<Chess>(EventName.WhenSelectChess.ToString(),this);
        if (CompareTag("Player") && !GameManage.instance.ifGameStart)
        {
            MapManage.instance.AwakeTile();
            GameManage.instance.HandChess = this;
            if (standTile != null) standTile.ChessLeave();           
        }
    }
    private void OnMouseUp()
    {
        if (CompareTag("Player")&&GameManage.instance.HandChess==this)
        {
            unAttackable = false;      
            MapManage.instance.SleepTile();
            if(touchTile!=null){
                if(touchTile.ifPrePareTile&&!standTile.ifPrePareTile){
                    EventController.Instance.TriggerEvent<Chess>(EventName.ChessLeaveDesk.ToString(),this);
                }else if(!touchTile.ifPrePareTile&&standTile.ifPrePareTile){
                    EventController.Instance.TriggerEvent<Chess>(EventName.ChessEnterDesk.ToString(),this);
                }
                standTile.ChessLeave();
                touchTile.ChessEnter(this);
                touchTile=null;
            }else{
                transform.position=standTile.transform.position;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (GameManage.instance.HandChess==this&&stateController.currentState==PrepareState.instance&&
        if(collision.GetComponent<Tile>()&& collision.GetComponent<Tile>().IfMoveable) 
            touchTile = collision.GetComponent<Tile>();
    }
    
}
[Serializable]
public class SkillController
{
    public Chess user;
    public Skill skill;
    public float t;
    public bool skillOver=false;
    public SkillController(Chess use,Skill skill)
    {
        user=use;
        this.skill = skill;
    }
    public SkillController()
    {
        
    }
    public void EnterGame()
    {
        if(skill!=null)
            skill.OnSkillEnter(user);
    }
    public void UseSkll()
    {
        skillOver = false;
        if(skill!=null)
            skill.SkillEffect(user);
        t = 0;
    }
    public void TimeAdd()
    {
        if (skill == null) return;
        if(t<skill.Interval)
            t+=Time.deltaTime;
        else 
        {
            //user.property.GetValue(ValueType.Mana).currentValue = 0;
            skill.OnSkillExit(user,user.target);
            skillOver=true;
        }
    }
}

public class ItemController{
    public List<Item> items;
    public Chess chess;
    public ItemController(Chess chess){
        this.chess=chess;
        items=new List<Item>();
    }

    //装备武器时触发对应的效果
    public void AddItem(Item item){
        items.Add(item);
        item.SetUser(chess);
        item.ItemEffect();
        UIManage.instance.ShowChessMessage(chess);
    }
    //卖出棋子后要归还所有武器
    public void SellChess(){
        //归还所有的武器
        foreach(var item in items)
            item.ResetItemAll();
    }
    public void ResetAllItem(){
        //重置所有的装备
        foreach(var item in items)
            item.ResetItem();
    }
}