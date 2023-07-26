using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill Data", menuName = "Scriptable Object/Skill Data", order = int.MaxValue)]
public class SkillSO : ScriptableObject
{
    public string skillName = "None";
    [ResizableTextArea] public string skillDescription = "None";
    public ESkillType skillType;
    public float originEffectAmount = 0.0f;
    public float coolTime;
    
    [ShowAssetPreview] public Sprite skillIcon;
    [ShowAssetPreview] public GameObject skillPrefab;
}
