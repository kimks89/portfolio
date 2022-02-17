using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Object = UnityEngine.Object;

public class RenamePrefixTools : EditorWindow
{
    private bool _isRenaming = false;
    private string _limitChangeValue = "0";
    private string _externalValue = ".prefab/.fbx/.mat/.png/.tga";

    [MenuItem("[Kyosun]/Tool/Rename Prefix File Window")]
    static void Init()
    {
        RenamePrefixTools window = (RenamePrefixTools)EditorWindow.GetWindow(typeof(RenamePrefixTools));
        window.Show();
    }
    
    private void OnGUI()
    {
        LoadMenu();
    }

    // 윈도우 호출
    private void LoadMenu()
    {
        // path(필요한 경로 추가, 외부에서 추가시 실수가 있기 때문에 내부에서 작성)
        var baseEffectPath = "Assets/Game/RemoteResources/Effect";
        var dicPath = new Dictionary<string, string>
        {
            {$"{baseEffectPath}/FX_Mesh", "Fxf_"},
            {$"{baseEffectPath}/FX_Texture/Material", "Fxtm_"},
            {$"{baseEffectPath}/FX_Texture/Texture", "Fxt_"},
            {$"{baseEffectPath}/FX_Texture/Uimaterial", "Fxtm_"},
            {$"{baseEffectPath}/FX_Texture/UiTexture", "Fxtui_"},
        };
        
        // gui layout
        const float bigSpace = 12f;
        const float smallSpce = 2f;

        GUILayout.Label("Effect Rename Path List", EditorStyles.boldLabel);
        GUILayout.Space(baseSpace);
        GUILayout.Label("0은 제한없음, 1부터 갯수 적용되며 숫자형식만 입력 가능합니다.");
        _limitChangeValue = EditorGUILayout.TextField("교체할 파일 갯수", _limitChangeValue);
        GUILayout.Space(smallSpce);
        GUILayout.Label("확장자명 입력시 .prefab형식(앞에 점 필수)로 입력하고 여러개 입력시 /로 구별 => ex).prefab/.png");
        _externalValue = EditorGUILayout.TextField("확장자명", _externalValue);
        GUILayout.Space(baseSpace);

        foreach (var path in dicPath)
        {
            EditorGUILayout.BeginHorizontal();
            var key = path.Key;
            var value = path.Value;
            GUILayout.Label($"path : {key}\nprefix : {value}", GUILayout.Height(40));
            if (GUILayout.Button("run", GUILayout.Height(40), GUILayout.Width(100)))
            {
                RenameFolderFile(key, value);
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(baseSpace);
        }
    }
    
    public void RenameFolderFile(string effectPath, string prefix)
    {
        if (_isRenaming)
        {
            return;
        }

        if (_limitChangeValue.IsNullOrEmpty())
        {
            _limitChangeValue = "0";
        }

        if (int.TryParse(_limitChangeValue, out var limitChangeCount) == false)
        {
            Debug.Log($"[RenameTool] Error! Limit Change Value Not Number");
            return;
        }
        
        if (_externalValue.IsNullOrEmpty())
        {
            Debug.Log($"[RenameTool] Error! Please Input External");
            return;
        }
        
        Debug.Log($"[RenameTool] Start! Rename Prefix Folder File");
        var startTime = DateTime.Now;
        _isRenaming = true;

        // External List
        var externals = _externalValue.Split('/');
        var externalList = externals.Length > 0 ? externals.ToList() : new List<string>();

        var isChanged = false;
        var changedCount = 0;
        var noChangedCount = 0;
        var arrPath = new string[1];
        arrPath[0] = effectPath;
        
        var guids = AssetDatabase.FindAssets("", arrPath); // t:Prefab
        foreach (var guid in guids)
        {
            var guidPath = AssetDatabase.GUIDToAssetPath(guid);
            
            // 1. 확장자 체크(안하면 주석)
            var divide = guidPath.Split('/');
            var fileName = divide.Length > 0 ? divide[divide.Length - 1] : string.Empty;
            var matchName = externalList.Find(x => fileName.ToLower().Contains(x));
            if (matchName.IsNullOrEmpty())
            {
                Debug.Log($"{fileName} is Not Apply");
                noChangedCount++;
                continue;
            }
            

            // 2. 해당경로의 prefix가져오기
            if (prefix.IsNullOrEmpty())
            {
                noChangedCount++;
                continue;
            }

            // 3. 적용될 이름 지정 및 중복체크(중복시 패스)
            var originalName = fileName;
            var modifyName = $"{prefix}{originalName}";
            if (originalName.Contains($"{prefix}"))
            {
                noChangedCount++;
                continue;
            }
            
            // 4. 에셋네임 교체
            AssetDatabase.RenameAsset(guidPath, modifyName);

            isChanged = true;
            changedCount++;

            // 5. 최대 몇개까지 변경할지 0이면 제한 없음
            if (0 < limitChangeCount && limitChangeCount <= changedCount)
            {
                break;
            }
        }

        var finishTime = DateTime.Now - startTime;
        Debug.Log($"[RenameTool] Completed! Rename Effect File Changed : {changedCount} / NoChanged : {noChangedCount} \n Time:{finishTime}");
        
        if (isChanged)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        _isRenaming = false;
    }
}
