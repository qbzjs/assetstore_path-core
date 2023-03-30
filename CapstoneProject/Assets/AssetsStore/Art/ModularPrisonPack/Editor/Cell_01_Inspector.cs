using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Cell_01))]
public class Cell_01_Inspector : Editor {

    Cell_01 cell;

    int sli;

    private void Awake()
    {
        var c01 = target as Cell_01;
        cell = c01;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        if (cell.cellRenderer == null)
        {
            base.OnInspectorGUI();
        }
        else
        {
            cell.rN = EditorGUILayout.IntSlider("Row Number", cell.rN, 0, cell.totalRows() - 1);
            cell.cN = EditorGUILayout.IntSlider("Column Number", cell.cN, 0, cell.totalColumns() - 1);
        }
        if (GUILayout.Button("Apply changes")) {
            cell.ChangeNumberDisplay();
            EditorUtility.SetDirty(cell);
        }
        
    }


}
