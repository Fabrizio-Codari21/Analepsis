using UnityEngine;

[CreateAssetMenu(fileName = "Loacalization_LanguageName", menuName = "Loacalization/New LanguageType")]
public class Language : ScriptableObject
{
    [Header("Language Code")] public string languageCode;
    [Header("Display")] public string displayName;
    public Font languageFont;
    public Sprite languageFlag;
}