/// <summary>
/// UserDataManager에서 사용하는 UserData Interface
/// </summary>
public interface IUserData
{
    // 기본값으로 데이터 초기화
    void SetDefaultData();

    // 데이터 로드
    bool LoadData();

    // 데이터 저장
    bool SaveData();
}
