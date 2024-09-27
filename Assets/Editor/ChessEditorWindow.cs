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

    [LabelText("����ģ����õ�·��")]
    public string AnimPath= "Assets/Animation/ģ��.controller";
    [LabelText("״̬��ģ�����·��")]
    public string statePath = "Assets/SO/State/ģ��.asset";
    [LabelText("���ʻ�������·��")]
    public string materialPath = "Assets/Shader/BaseShader/BaseMaterial.mat";
    [LabelText("����")]
    public string chessName;
    [LabelText("ͼƬ")]
    public Sprite chessSprite;
    [Serializable]
    public class WeaponMes
    {
        [SerializeReference]
        public Weapon weapon;
    }
    [FoldoutGroup("������Ϣ")]
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
    [FoldoutGroup("������Ϣ")]
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
    [Button("����")]
    public void CreateChess()
    {
        //1.��һ�� ����һ��GameObject
       
        if (Directory.Exists("Assets/Prefab/ChessPrefab/" + chessName))
        {
            Debug.Log("�Ѿ���������ļ���");
            return;
        }
        else
        {
            GameObject gameObject = new GameObject();
            string foldpath = "Assets/Prefab/ChessPrefab/" + chessName + "/";
            //����Ԥ����
            AssetDatabase.CreateFolder("Assets/Prefab/ChessPrefab", chessName);
            AssetDatabase.CreateFolder("Assets/Prefab/ChessPrefab/" + chessName , "����");
            AssetDatabase.CreateFolder("Assets/Prefab/ChessPrefab/" + chessName , "ͼƬ");
            gameObject.AddComponent<Chess>();
            Chess myScript = gameObject.GetComponent<Chess>();
            //�����ײ��
            gameObject.AddComponent<BoxCollider2D>();
            gameObject.GetComponent<Collider2D>().isTrigger = true;
            gameObject.AddComponent<Animator>();
            gameObject.GetComponent<Chess>().animator = gameObject.GetComponent<Animator>();
            //����������
            GameObject childObject = new GameObject("sprite");
            childObject.transform.SetParent(myScript.transform);
            childObject.transform.localPosition = Vector3.zero;
            childObject.AddComponent<SpriteRenderer>();
            myScript.sprite = childObject.GetComponent<SpriteRenderer>();
            myScript.sprite.sprite = chessSprite;
            myScript.sprite.material= AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            myScript.sprite.sortingLayerName = "Chess";
            myScript.sprite.sortingOrder = 1;
            //������Ҫ����AnimtorOverrideController
            //��������Ҫ�ȸ�һ��ģ��
            AnimatorController animModel = AssetDatabase.LoadAssetAtPath<AnimatorController>(AnimPath);
            AnimatorOverrideController overrideController = new AnimatorOverrideController();
            overrideController.runtimeAnimatorController = animModel;
            myScript.animator.runtimeAnimatorController = overrideController;
            AssetDatabase.CreateAsset(overrideController, foldpath + "����/" + chessName + "Anim.overrideController");
            //����������Ҫ����State��
            //����Ҳ�ǻ��ģ��
            //���ģ���·��ҲӦ���Ƕ�̬��
            StateGraph stateGraph = AssetDatabase.LoadAssetAtPath<StateGraph>
                (statePath);
            myScript.stateController = new StateController();
            myScript.stateController.stateGraph = stateGraph;
            //Ȼ����PropertyCreator
            PropertyCreator propertyCreator = new PropertyCreator();
            AssetDatabase.CreateAsset(propertyCreator, foldpath + chessName + ".asset");
            myScript.propertyController = new PropertyController();
            myScript.propertyController.creator = propertyCreator;
            propertyCreator.chessName=chessName;
            propertyCreator.chessSprite=chessSprite;
            InitProperty(propertyCreator);
            
            
            //������ʼ��
            myScript.equipWeapon = new AttackController();
            InitWeapon(myScript.equipWeapon);

            //�ƶ���ʼ��
            myScript.moveController = new MoveController();
            myScript.moveController.tileMethod = findTileMethod;

            GameObject pre= PrefabUtility.SaveAsPrefabAsset(gameObject, foldpath + chessName + ".prefab");
            //AssetDatabase.RenameAsset("", "");
            propertyCreator.chessPre = pre.GetComponent<Chess>();
            DestroyImmediate(gameObject);
            Debug.Log("�����ɹ���");
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
