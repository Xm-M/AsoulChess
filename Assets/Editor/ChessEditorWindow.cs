using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using System.IO;
using UnityEditor.Animations;
using System;

public class ChessEditorWindow : OdinEditorWindow
{

    [LabelText("动画模板放置的路径")]
    public string AnimPath= "Assets/Animation/模板.controller";
    [LabelText("状态机模板放置路径")]
    public string statePath = "Assets/SO/State/模板.asset";
    [LabelText("材质基础放置路径")]
    public string materialPath = "Assets/Shader/BaseShader/BaseMaterial.mat";
    [LabelText("棋子预制体存放基础路径")]
    public string chessPrefabBasePath = "Assets/Prefab/ChessPrefab";
    [LabelText("棋子是否为敌人(默认为否)")]
    public bool enemy;
    [LabelText("名称")]
    public string chessName;
    [LabelText("图片")]
    public Sprite chessSprite;
    [Serializable]
    public class WeaponMes
    {
        [SerializeReference]
        public Weapon weapon;
    }
    [FoldoutGroup("武器信息")]
    [SerializeReference]
    public WeaponMes weaponData;
    [Searchable]
    public class SkillMes
    {
        [SerializeReference]
        public ISkill activeSkill;//主动技能
        [SerializeReference]
        public ISkill passiveSkill;//被动技能
    }
    [FoldoutGroup("技能信息")]
    [SerializeReference]
    public SkillMes skillData;
    [Serializable]
    public class PropertyMes
    {
        public Property data;
        public List<string> tags;
        public PlantType plantType;
        public TileType tileType;
        [SerializeReference]
        public IPlantFunction plantFunction;
    } 
    [FoldoutGroup("属性信息")]
    [SerializeReference]
    public PropertyMes propertyData;
    [SerializeReference]
    public FindTileMethod findTileMethod;
    [MenuItem("Tools/ChessEditorWindow")]
    private static void OpenWindow()
    {
        var window=EditorWindow.GetWindow<ChessEditorWindow>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 500);
    }
    [Button("创建")]
    public void CreateChess()
    {
        //1.第一步 创建一个GameObject
       
        if (Directory.Exists("Assets/Prefab/ChessPrefab/" + chessName))
        {
            Debug.Log("已经存在这个文件夹");
            return;
        }
        else
        {
            // 1. 第一部分：确定预制体的存储路径
            string folderPath = $"{chessPrefabBasePath}/{chessName}/";

            // 检查并创建顶级文件夹
            if (!AssetDatabase.IsValidFolder(chessPrefabBasePath))
            {
                Debug.LogError($"基础路径不存在: {chessPrefabBasePath}");
                return;
            }

            // 检查并创建棋子文件夹
            if (!AssetDatabase.IsValidFolder(folderPath.TrimEnd('/')))
            {
                AssetDatabase.CreateFolder(chessPrefabBasePath, chessName);
            }

            // 检查并创建“动画”文件夹
            string animFolderPath = folderPath + "动画";
            if (!AssetDatabase.IsValidFolder(animFolderPath))
            {
                AssetDatabase.CreateFolder(folderPath.TrimEnd('/'), "动画");
            }

            // 检查并创建“图片”文件夹
            string imageFolderPath = folderPath + "图片";
            if (!AssetDatabase.IsValidFolder(imageFolderPath))
            {
                AssetDatabase.CreateFolder(folderPath.TrimEnd('/'), "图片");
            }

            // 刷新 AssetDatabase 以确保文件夹的存在
            AssetDatabase.Refresh();


            // 3. 创建棋子GameObject
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<Chess>();
            Chess myScript = gameObject.GetComponent<Chess>();
            //添加碰撞体
            gameObject.AddComponent<BoxCollider2D>();
            gameObject.GetComponent<Collider2D>().isTrigger = true;
            Animator  animator= gameObject.AddComponent<Animator>();
            AnimatorController animatorController= gameObject.AddComponent<AnimatorController>();
            //gameObject.GetComponent<Chess>().animatorController.animator = gameObject.GetComponent<Animator>();
            //创建子物体
            GameObject childObject = new GameObject("sprite");
            childObject.transform.SetParent(myScript.transform);
            childObject.transform.localPosition = Vector3.zero;
            childObject.AddComponent<SpriteRenderer>();
            animatorController.sprite = childObject.GetComponent<SpriteRenderer>();
            animatorController.animator=animator;
            animatorController.sprite.sprite = chessSprite;
            animatorController.sprite.material= AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            animatorController.sprite.sortingLayerName = "Chess";
            animatorController.sprite.sortingOrder = 1;
            //接下来要创建AnimtorOverrideController
            //首先我们要先搞一个模板
            AnimatorController animModel = AssetDatabase.LoadAssetAtPath<AnimatorController>(AnimPath);
            AnimatorOverrideController overrideController = new AnimatorOverrideController();
            //overrideController.runtimeAnimatorController = animModel;
            //myScript.animator.runtimeAnimatorController = overrideController;
            //Debug.Log(folderPath + "动画/" + chessName + "Anim.overrideController");
            AssetDatabase.CreateAsset(overrideController, folderPath + "动画/" + chessName + "Anim.overrideController");
            //再下来就是要创建State了
            //首先也是获得模板
            //这个模板的路径也应该是动态的
            StateGraph stateGraph = AssetDatabase.LoadAssetAtPath<StateGraph>
                (statePath);
            myScript.stateController = new StateController();
            myScript.stateController.stateGraph = stateGraph;
            //然后是PropertyCreator
            PropertyCreator propertyCreator = new PropertyCreator();
            string p = "Player/";
            if (enemy) p = "Enemy/";
            AssetDatabase.CreateAsset(propertyCreator, "Assets/Resources/ChessData/"+p + chessName + ".asset");
            myScript.propertyController = new PropertyController();
            myScript.propertyController.creator = propertyCreator;
            propertyCreator.chessName=chessName;
            propertyCreator.chessSprite=chessSprite;
            InitProperty(propertyCreator);
            //技能初始化
            myScript.skillController = new SkillController();
            InitSkill(myScript.skillController);

            //武器初始化
            myScript.equipWeapon = new AttackController();
            InitWeapon(myScript.equipWeapon);

            //移动初始化
            myScript.moveController = new MoveController();
            myScript.moveController.tileMethod = findTileMethod;

            GameObject pre= PrefabUtility.SaveAsPrefabAsset(gameObject, folderPath + chessName + ".prefab");
            //AssetDatabase.RenameAsset("", "");
            propertyCreator.chessPre = pre.GetComponent<Chess>();
            DestroyImmediate(gameObject);
            Debug.Log("创建成功！");
        }
    }
    void InitWeapon(AttackController weapon)
    {
        if (weaponData != null)
        {
            weapon.weapon = weaponData.weapon;
        }
    }
    void InitProperty(PropertyCreator creator)
    {
        if (propertyData!=null)
        {
            creator.baseProperty = new Property(propertyData.data);
            creator.plantTags = new List<string>(propertyData.tags);
            creator.plantFunction= propertyData.plantFunction;
            creator.plantType= propertyData.plantType;
            //creator.ifo
        }
    }
    void InitSkill(SkillController skillController)
    {
        if (skillData != null)
        {
            skillController.activeSkill = skillData.activeSkill;
            skillController.passiveSkill = skillData.passiveSkill;
        }
    }
    protected override void OnDisable()
    {
        ResetData();
        Debug.Log("关闭窗口");
    }

    // 重置所有引用数据
    private void ResetData()
    {
        weaponData = null;
        skillData = null;
        propertyData = null;
        findTileMethod = null;
        chessSprite = null;
        chessName = string.Empty;
    }

}
