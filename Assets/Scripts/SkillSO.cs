using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill Data", menuName = "Scriptable Object/Skill Data", order = int.MaxValue)]
public class SkillSO : ScriptableObject
{
    public string skillName = "None";
    [ResizableTextArea] public string skillDescription = "None";
    public ESkillType skillType;
    public float skillEffectAmount = 0.0f;
    public float skillCoolTime;
    
    [ShowAssetPreview] public Sprite skillIcon;
    [ShowAssetPreview] public GameObject skillPrefab;
}
