using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UserDataManager))]
public class UserDataManagerEditor : Editor
{
    private void OnEnable()
    {
        // UserDataManager 인스턴스에 접근
        UserDataManager userDataManager = (UserDataManager)target;

        // 동적으로 생성된 UserGameData가 있는 경우 PlayDatas를 표시
        if (userDataManager.UserDataList != null && userDataManager.UserDataList.Count > 0)
        {
            UserGameData userGameData = userDataManager.UserDataList[0] as UserGameData;

            if (userGameData != null && userGameData.PlayDatas != null)
            {
                // PlayDatas를 인스펙터에 표시하도록 강제
                foreach (var playData in userGameData.PlayDatas)
                {
                    EditorGUILayout.LabelField("PlayData", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PlayDatas"), true);
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // 기본 인스펙터를 렌더링합니다
        base.OnInspectorGUI();

        // 인스펙터에 커스텀 표시를 추가합니다
        // UserGameData의 PlayDatas 리스트를 표시할 코드 추가
        UserDataManager userDataManager = (UserDataManager)target;

        if (userDataManager.UserDataList != null && userDataManager.UserDataList.Count > 0)
        {
            UserGameData userGameData = userDataManager.GetUserData<UserGameData>();

            if (userGameData != null && userGameData.PlayDatas != null)
            {
                for (int i = 0; i < userGameData.PlayDatas.Count; i++)
                {
                    var playData = userGameData.PlayDatas[i];
                    bool foldout1 = EditorGUILayout.Foldout(true, $"PlayData : {i}");
                    if (foldout1)
                    {
                        EditorGUILayout.LabelField("PlayData", EditorStyles.boldLabel);
                        // 각 PlayData 항목을 표시
                        EditorGUILayout.LabelField("HasData", playData.HasData.ToString());
                        EditorGUILayout.IntField("StageCount", playData.StageCount);
                        EditorGUILayout.IntField("StageClearCount", playData.StageClearCount);

                        foreach (var mapInfo in playData.MapInfoList)
                        {
                            EditorGUILayout.LabelField("MapInfo", EditorStyles.boldLabel);
                            mapInfo.Floor = EditorGUILayout.IntField("Floor", mapInfo.Floor);
                            mapInfo.Stage = EditorGUILayout.IntField("Stage", mapInfo.Stage);
                            mapInfo.ClearFlag = EditorGUILayout.IntField("ClearFlag", mapInfo.ClearFlag);
                        }
                    }
                    
                    GUILayout.Space(10);
                }
            }
        }

        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Save"))
        {
            userDataManager.SaveUserData();
        }

        if (GUILayout.Button("Load"))
        {
            userDataManager.LoadUserData();
        }

        if (GUILayout.Button("Reset"))
        {
            userDataManager.SetDefaultUserData();
        }
    }
}
