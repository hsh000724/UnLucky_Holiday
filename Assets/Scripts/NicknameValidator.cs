using System.Linq;

public static class NicknameValidator
{
    private static string[] badWords = { "비속어1", "비속어2" }; // 실제 필터 리스트로 교체

    public static (bool isValid, string message) Validate(string nickname)
    {
        if (string.IsNullOrEmpty(nickname) || nickname.Length < 2 || nickname.Length > 10)
            return (false, "닉네임은 2~10자 사이여야 합니다.");

        if (badWords.Any(word => nickname.Contains(word)))
            return (false, "부적절한 단어가 포함되어 있습니다.");

        return (true, "");
    }
}