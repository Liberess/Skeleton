using UnityEngine;

[CreateAssetMenu(fileName = "Equipment Data SO", menuName = "Scriptable Object/Equipment Data", order = int.MaxValue)]
public class EquipmentDataSO : ScriptableObject
{
    [SerializeField] private EquipmentData equipmentData;
    public EquipmentData EquipmentData => equipmentData;
}
