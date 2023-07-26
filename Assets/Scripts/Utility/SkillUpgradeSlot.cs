using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillUpgradeSlot : MonoBehaviour
{
    private SkillSO skillData;

    [SerializeField] private ESkillType skillType;
    
    [SerializeField] private TextMeshProUGUI skillLvTxt;
    [SerializeField] private TextMeshProUGUI skillNameTxt;
    [SerializeField] private TextMeshProUGUI skillDescriptionTxt;

    [SerializeField] private TextMeshProUGUI costTxt;
    [SerializeField] private Button upBtn;
    
    private void Start()
    {
        skillData = DataManager.Instance.GetSkillSO(skillType);
        
        skillNameTxt.text = skillData.skillName;
        skillDescriptionTxt.text = skillData.skillDescription;

        upBtn.onClick.AddListener(OnClickStatUp);

        UpdateUI();
    }

    private void UpdateUI()
    {
        int curSkillLv = DataManager.Instance.GameData.skillUpLevels[(int)skillData.skillType];
        skillLvTxt.text = curSkillLv.ToString();

        float effectAmount = DataManager.Instance.GetSkillEffectAmount(skillType);
        skillDescriptionTxt.text = string.Format(skillData.skillDescription, effectAmount);

        if (curSkillLv >= DataManager.Instance.GameData.MaxStatLevel)
        {
            costTxt.gameObject.SetActive(false);
        }
        else
        {
            int nextCost = Mathf.RoundToInt(curSkillLv * 100 * 1.5f);
            costTxt.gameObject.SetActive(true);
            costTxt.text = string.Concat("<sprite=0>", nextCost > 0 ? $"{nextCost:#,###}" : "0");
        }
    }
    
    private void OnClickStatUp()
    {
        // AudioManager.Instance.PlaySFX("UIClick");

        int curSkillLv = DataManager.Instance.GameData.skillUpLevels[(int)skillData.skillType];
        int maxSkillLevel = DataManager.Instance.GameData.MaxSkillLevel;
        if (curSkillLv >= maxSkillLevel)
            return;

        int requiredKarma = Mathf.RoundToInt(curSkillLv * 100 * 1.5f);
        int availableKarma = DataManager.Instance.GameData.karma;
        if (availableKarma < requiredKarma)
            return;

        GameManager.Instance.SetKarma(-requiredKarma);

        float effectAmount = DataManager.Instance.GetSkillEffectAmount(skillType);
        float increaseAmount = GetSkillIncreaseAmount(skillData.skillType, effectAmount);

        DataManager.Instance.GameData.skillEffectAmounts[(int)skillData.skillType] =
            Mathf.Clamp(increaseAmount, 0.0f, float.MaxValue);

        DataManager.Instance.GameData.skillUpLevels[(int)skillData.skillType]++;

        UpdateUI();
    }
    
    private float GetSkillIncreaseAmount(ESkillType skillType, float originAmount)
    {
        switch (skillType)
        {
            case ESkillType.PhantomBlade:
                return originAmount + 50.0f;
            case ESkillType.FireBall:
                return originAmount + 10.0f;
            case ESkillType.Recovery:
                return originAmount + 30.0f;
            default:
                return 0.0f;
        }
    }
}
