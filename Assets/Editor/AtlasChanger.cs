using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEditorInternal;
using UObject = UnityEngine.Object;

public class AtlasChanger : EditorWindow {
    private string[] toolbarStrings = {"Changed", "Missing"};

    private enum ToolbarType {
        Changed = 0, Missing = 1
    }

    [MenuItem("Window/Object Atlas Changer")]
    static void Init () {
        AtlasChanger window = EditorWindow.GetWindow<AtlasChanger> ();
        window.minSize = new Vector2(200,300);
    }
    [Serializable]
    public struct TextureData {
        public Texture2D _texture2D;
    }
    [Serializable]
    public class SpriteDetail {
        public Sprite sprite;
        public List<UObject> references;
    }

    private GameObject target;
    private Vector2 missingScrollPos;
    private Vector2 changedScrollPos;
    private List<SpriteDetail> missingSprites;
    private List<SpriteDetail> changedSprites;
    private List<TextureData> textureDatas;
    private ReorderableList list;
    private ToolbarType activeToolbar;

    private void OnEnable() {
        textureDatas = new List<TextureData>();
        missingSprites = new List<SpriteDetail>();
        changedSprites = new List<SpriteDetail>();

        list = new ReorderableList(textureDatas, typeof(TextureData));
        list.drawElementCallback = DrawTextureData;
        list.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "Atlases");
        };
    }

    private void DrawTextureData(Rect rect, int index, bool isActive, bool isFocused) {
        var element = textureDatas[index];
        rect.y += 2;
        element._texture2D = (Texture2D)EditorGUI.ObjectField(
            new Rect(rect.x, rect.y, EditorGUIUtility.currentViewWidth - 50, EditorGUIUtility.singleLineHeight),
            element._texture2D == null ? "None" : element._texture2D.name,
            element._texture2D,
            typeof(Texture2D),
            false
        );
        textureDatas[index] = element;
    }

    void OnGUI () {
        EditorGUILayout.BeginVertical();
        target = EditorGUILayout.ObjectField("Target: ", target, typeof(GameObject), true) as GameObject;
        EditorGUILayout.Space();
        list.DoLayoutList();
        EditorGUILayout.Space();
        activeToolbar = (ToolbarType)GUILayout.Toolbar((int)activeToolbar, toolbarStrings);
        switch (activeToolbar) {
            case ToolbarType.Changed:
                changedScrollPos = EditorGUILayout.BeginScrollView(changedScrollPos);
                ListDetails(changedSprites);
                EditorGUILayout.EndScrollView();
                break;
            case ToolbarType.Missing:
                missingScrollPos = EditorGUILayout.BeginScrollView(missingScrollPos);
                ListDetails(missingSprites);
                EditorGUILayout.EndScrollView();
                break;
        }
        if (GUILayout.Button("Analyze")) {
            changedSprites.Clear();
            missingSprites.Clear();
            Analyze();
            GUIUtility.hotControl = 0;
            GUIUtility.keyboardControl = 0;
        }
        EditorGUILayout.EndVertical();
    }

    void ListDetails(List<SpriteDetail> spriteDetails) {
        foreach (var spriteDetail in spriteDetails) {
            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField(spriteDetail.sprite.name);
            EditorGUILayout.LabelField(spriteDetail.sprite.texture.name);
            if (GUILayout.Button("Select")) {
                Selection.objects = spriteDetail.references.ToArray();
            }
            EditorGUILayout.EndHorizontal ();
        }
    }

    void Analyze () {
        if (target != null) {
            LinkedList<Image> images = new LinkedList<Image>();
            FindAll<Image>(target, ref images);

            List<Sprite[]> textureList = new List<Sprite[]>(textureDatas.Count);
            foreach (var textureData in textureDatas) {
                textureList.Add(GetTextureSprites(textureData._texture2D));
            }

            foreach (Image image in images) {
                Sprite foundSprite = null;
                foreach (var sprites in textureList) {
                    foundSprite = (Array.Find(sprites, sprite => (image.sprite != null && sprite.name == image.sprite.name)));
                    if (foundSprite != null) break;
                }
                if (foundSprite == null) {
                    AddImage(ref missingSprites, image);
                } else {
                    if (image.sprite != foundSprite) {
                        image.sprite = foundSprite;
                        AddImage(ref changedSprites, image);
                    }
                }
            }
        }
    }

    void AddImage(ref List<SpriteDetail> spriteDetails, Image image) {
        SpriteDetail detail = spriteDetails.Find(sd => sd.sprite == image.sprite);
        if (detail == null) {
            detail = new SpriteDetail() {sprite = image.sprite, references = new List<UObject>()};
            detail.references.Add(image.gameObject);
            spriteDetails.Add(detail);
        }
        else {
            detail.references.Add(image.gameObject);
        }
    }

    Sprite[] GetTextureSprites (Texture2D texture2D) {
        string spriteSheet = AssetDatabase.GetAssetPath(texture2D);
        return AssetDatabase.LoadAllAssetsAtPath(spriteSheet).OfType<Sprite>().ToArray();

    }

    private void FindAll<T> (GameObject parent, ref LinkedList<T> list) {
        int childCount = parent.transform.childCount;
        for (int i = 0; i < childCount; i++) {
            GameObject child = parent.transform.GetChild(i).gameObject;
            T image = child.GetComponent<T>();
            if (image != null) list.AddLast(image);
            if (child.transform.childCount > 0) FindAll<T>(child, ref list);
        }
    }
}
