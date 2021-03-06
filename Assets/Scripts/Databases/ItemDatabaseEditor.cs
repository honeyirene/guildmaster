using GuildMaster.Items;
using UnityEditor;

namespace GuildMaster.Databases
{
    [CustomEditor(typeof(ItemDatabase))]
    public class ItemDatabaseEditor: DatabaseEditor
    {
        protected override void CurrentItemField(SerializedProperty itemStaticData)
        {
            var itemName = itemStaticData.FindPropertyRelative("itemName");
            var itemDescription = itemStaticData.FindPropertyRelative("itemDescription");
            var isConsumable = itemStaticData.FindPropertyRelative("isConsumable");
            var consumptionEffect = itemStaticData.FindPropertyRelative("consumptionEffect");
            var maxStack = itemStaticData.FindPropertyRelative("maxStack");
            var itemImage = itemStaticData.FindPropertyRelative("itemImage");
            var isEquipable = itemStaticData.FindPropertyRelative("isEquipable");
            var defaultEquipmentStats = itemStaticData.FindPropertyRelative("defaultEquipmentStats");
            var isImportant = itemStaticData.FindPropertyRelative("isImportant");

            EditorGUILayout.PropertyField(itemName);
            EditorGUILayout.PropertyField(itemDescription);
            EditorGUILayout.PropertyField(itemImage);
            EditorGUILayout.PropertyField(maxStack);

            EditorGUILayout.PropertyField(isConsumable);
            if (isConsumable.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(consumptionEffect);
                EditorGUI.indentLevel--;
            }
            else
            {
                consumptionEffect.managedReferenceValue = null;
            }
            
            EditorGUILayout.PropertyField(isEquipable);
            if (isEquipable.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(defaultEquipmentStats);
                EditorGUI.indentLevel--;
            }
            else
            {
                defaultEquipmentStats.managedReferenceValue = null;
            }

            EditorGUILayout.PropertyField(isImportant);
        }
    }
}