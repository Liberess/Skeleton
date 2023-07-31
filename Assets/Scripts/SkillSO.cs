using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "Skill Data", menuName = "Scriptable Object/Skill Data", order = int.MaxValue)]
public class SkillSO : ScriptableObject
{
    [BoxGroup("# Base Skill Settings")]
    public string skillName = "None";

    [BoxGroup("# Base Skill Settings"), ResizableTextArea]
    public string skillDescription = "None";

    [BoxGroup("# Base Skill Settings")]
    public ESkillType skillType;
    [BoxGroup("# Base Skill Settings")]
    public int skillImpactAmount = 0;
    [BoxGroup("# Base Skill Settings")]
    public float skillCoolTime;
    [BoxGroup("# Base Skill Settings"), ShowAssetPreview]
    public Sprite skillIcon;

    [BoxGroup("# Range Skill Settings")]
    public bool isRangeSkill = false;
    [BoxGroup("# Range Skill Settings"), EnableIf("isRangeSkill")]
    public float skillImpactRange = 0.0f;

    [BoxGroup("# Projectile Skill Settings")]
    public bool isProjectileSkill = false;
    [BoxGroup("# Projectile Skill Settings"), EnableIf("isProjectileSkill")]
    public float projectileDistance = 0.0f;
    [BoxGroup("# Projectile Skill Settings"), EnableIf("isProjectileSkill")]
    public float projectileVelocity = 0.0f;

}