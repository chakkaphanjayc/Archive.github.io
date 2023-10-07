using PlayFab.ClientModels;

namespace ArchivementProject.Script;

public class UserDataService
{
    private static UserDataService _instance;
    private Dictionary<string, string> userData = new Dictionary<string, string>();

    public LoginResult _LoginResult;
    // Private constructor to prevent external instantiation
    private UserDataService()
    {
    }

    public static UserDataService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new UserDataService();
            }
            return _instance;
        }
    }

    public void SetUserData(string key, string value)
    {
        userData[key] = value;
    }

    public string GetUserData(string key)
    {
        if (userData.ContainsKey(key))
        {
            return userData[key];
        }
        return null;
    }
}