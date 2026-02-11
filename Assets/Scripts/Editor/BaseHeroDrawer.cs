#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BaseHero))]
public class BaseHeroDrawer : PropertyDrawer
{
    // simple foldout state (per drawer instance)
    private bool foldout = true;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // if not folded, only one line height
        if (!foldout) return EditorGUIUtility.singleLineHeight + 4f;

        // folded: compute height: fold label + popup (1 line) + all child properties heights
        float total = EditorGUIUtility.singleLineHeight + 4f; // fold label
        total += EditorGUIUtility.singleLineHeight + 2f; // popup field height

        SerializedProperty iterator = property.Copy();
        SerializedProperty end = iterator.GetEndProperty();
        // move to first child
        if (iterator.NextVisible(true))
        {
            while (!SerializedProperty.EqualContents(iterator, end))
            {
                // skip the characterName since we handle it separately
                if (iterator.name == "characterName")
                {
                    if (!iterator.NextVisible(false)) break;
                    continue;
                }

                float h = EditorGUI.GetPropertyHeight(iterator, true);
                total += h + 2f; // add spacing
                if (!iterator.NextVisible(false)) break;
            }
        }

        return total;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // draw foldout label
        Rect foldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        foldout = EditorGUI.Foldout(foldRect, foldout, label, true);

        float y = foldRect.y + EditorGUIUtility.singleLineHeight + 2f;

        if (!foldout)
        {
            EditorGUI.EndProperty();
            return;
        }

        // find characterName property
        SerializedProperty nameProp = property.FindPropertyRelative("characterName");
        Rect popupRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);

        if (nameProp == null)
        {
            EditorGUI.HelpBox(popupRect, "'characterName' property tidak ditemukan di BaseHero!", MessageType.Error);
            EditorGUI.EndProperty();
            return;
        }

        // draw dropdown (or fallback to normal field)
        if (CharacterDatabase.Instance != null && CharacterDatabase.Instance.characters.Count > 0)
        {
            string[] heroNames = new string[CharacterDatabase.Instance.characters.Count];
            CharacterDatabase.Instance.characters.GetCharacterNames().CopyTo(heroNames, 0);

            int index = Mathf.Max(0, System.Array.IndexOf(heroNames, nameProp.stringValue));
            index = EditorGUI.Popup(popupRect, "Character", index, heroNames);
            nameProp.stringValue = heroNames[index];
        }
        else
        {
            // fallback simple property field if DB belum siap
            EditorGUI.PropertyField(popupRect, nameProp, new GUIContent("Character"));
        }

        y += EditorGUIUtility.singleLineHeight + 4f;

        // draw all other child properties (except characterName) safely
        SerializedProperty iterator = property.Copy();
        SerializedProperty end = iterator.GetEndProperty();

        // move into children
        if (iterator.NextVisible(true))
        {
            while (!SerializedProperty.EqualContents(iterator, end))
            {
                // skip the name because we already drew it
                if (iterator.name == "characterName")
                {
                    if (!iterator.NextVisible(false)) break;
                    continue;
                }

                float h = EditorGUI.GetPropertyHeight(iterator, true);
                Rect childRect = new Rect(position.x, y, position.width, h);
                EditorGUI.PropertyField(childRect, iterator, true);
                y += h + 2f;

                if (!iterator.NextVisible(false)) break;
            }
        }

        EditorGUI.EndProperty();
    }
}
#endif
