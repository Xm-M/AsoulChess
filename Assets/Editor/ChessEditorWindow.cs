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
            GameObject gameObject = new GameObject();
            string foldpath = "Assets/Prefab/ChessPrefab/" + chessName + "/";
            //创建预制体
            AssetDatabase.CreateFolder("Assets/Prefab/ChessPrefab", chessName);
            AssetDatabase.CreateFolder("Assets/Prefab/ChessPrefab/" + chessName , "动画");
            AssetDatabase.CreateFolder("Assets/Prefab/ChessPrefab/" + chessName , "图片");
            gameObject.AddComponent<Chess>();
            Chess myScript = gameObject.GetComponent<Chess>();
            //添加碰撞体
            gameObject.AddComponent<BoxCollider2D>();
            gameObject.GetComponent<Collider2D>().isTrigger = true;
            gameObject.AddComponent<Animator>();
            gameObject.GetComponent<Chess>().animator = gameObject.GetComponent<Animator>();
            //创建子物体
            GameObject childObject = new GameObject("sprite");
            childObject.transform.SetParent(myScript.transform);
            childObject.transform.localPosition = Vector3.zero;
            childObject.AddComponent<SpriteRenderer>();
            myScript.sprite = childObject.GetComponent<SpriteRenderer>();
            myScript.sprite.sprite = chessSprite;
            myScript.sprite.material= AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            myScript.sprite.sortingLayerName = "Chess";
            myScript.sprite.sortingOrder = 1;
            //接下来要创建AnimtorOverrideController
            //首先我们要先搞一个模板
            AnimatorController animModel = AssetDatabase.LoadAssetAtPath<AnimatorController>(AnimPath);
            AnimatorOverrideController overrideController = new AnimatorOverrideController();
            overrideController.runtimeAnimatorController = animModel;
            myScript.animator.runtimeAnimatorController = overrideController;
            AssetDatabase.CreateAsset(overrideController, foldpath + "动画/" + chessName + "Anim.overrideController");
            //再下来就是要创建State了
            //首先也是获得模板
            //这个模板的路径也应该是动态的
            StateGraph stateGraph = AssetDatabase.LoadAssetAtPath<StateGraph>
                (statePath);
            myScript.stateController = new StateController();
            myScript.stateController.stateGraph = stateGraph;
            //然后是PropertyCreator
            PropertyCreator propertyCreator = new PropertyCreator();
            AssetDatabase.CreateAsset(propertyCreator, foldpath + chessName + ".asset");
            myScript.propertyController = new PropertyController();
            myScript.propertyController.creator = propertyCreator;
            propertyCreator.chessName=chessName;
            propertyCreator.chessSprite=chessSprite;
            InitProperty(propertyCreator);
            
            
            //武器初始化
            myScript.equipWeapon = new AttackController();
            InitWeapon(myScript.equipWeapon);

            //移动初始化
            myScript.moveController = new MoveController();
            myScript.moveController.tileMethod = findTileMethod;

            GameObject pre= PrefabUtility.SaveAsPrefabAsset(gameObject, foldpath + chessName + ".prefab");
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
}
