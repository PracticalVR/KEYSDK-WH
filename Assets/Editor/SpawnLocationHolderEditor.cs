using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using PracticalManaged.Practical.SpatialSpawning;

namespace Practical.Internal
{

    [CustomEditor(typeof(PracticalLocationBuilder))]
    public class SpawnLocationHolderEditor : Editor
    {
        private int selectedGroupIndex;

        [SerializeField]
        private PracticalLocator holder;
        private PracticalLocator real;
        private string newGroupName;            // We we want to create a new group
        private string tempEditingGroupName;    // When we want to change an existing group

        
        [SerializeField]
        private Vector2 scrollPosition = Vector2.zero;

        private SpawnLocation workingSpawnLocation = new SpawnLocation(SelectedSpawnType.Wall, SelectedSpawnSize.Small);

        // Custom styles (currently unused)
        //private GUIStyle groupBoxStyle = null;
        //private Color xButtonColor = new Color(.7f, .2f, .3f);

        private bool isEditingNameField = false;
        private Texture editButtonTex;
        private GUIContent editButtonTexContent;


        #region CONSTANTS
        private const int UNSELECTED = -1;
        private const float addButtonWidth = 20f;
        private const float saveCancelButtonWidth = 40f;
        private const string editButtonString = "Edit";
        private const string doneButtonString = "Done";
        private const string cancelButtonString = "Cancel";
        #endregion

        void OnEnable()
        {
            holder = (PracticalLocationBuilder)target;
            real = (PracticalLocator)target;
            selectedGroupIndex = UNSELECTED;
            isEditingNameField = false;
            editButtonTex = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Editor/Images/edit.png", typeof(Texture));
            if (editButtonTex != null)
            {
                editButtonTexContent = new GUIContent(editButtonTex);
            }
            else
            {
                editButtonTexContent = new GUIContent(editButtonString);
            }
        }

        //private void InitStyles() {
        //	if(groupBoxStyle == null) {
        //		groupBoxStyle = new GUIStyle(GUI.skin.box);
        //	}
        //}

        //private Texture2D MakeTex(int width, int height, Color col) {
        //	Color[] pix = new Color[width * height];
        //	for(int i = 0; i < pix.Length; ++i) {
        //		pix[i] = col;
        //	}
        //	Texture2D result = new Texture2D(width, height);
        //	result.SetPixels(pix);
        //	result.Apply();
        //	return result;
        //}


        public override void OnInspectorGUI()
        {

            //InitStyles();

            //serializedObject.Update();

            float addToListButtonWidth = Screen.width * 0.4f;
            int previousIndentLevel = EditorGUI.indentLevel;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.BeginHorizontal();
            newGroupName = EditorGUILayout.TextField(new GUIContent("New Group", "Add a new group"), newGroupName);

            if (GUILayout.Button("+", GUILayout.Width(addButtonWidth)))
            {
                if (FindNameMatch(newGroupName))
                {
                    Debug.LogWarning("Group name: " + newGroupName + " already exists");
                }
                else
                {
                    AddNewGroup();
                }
            }

            EditorGUILayout.EndHorizontal();


            EditorGUI.indentLevel++;
            Color initialColor = GUI.backgroundColor;

            for (int g = 0; g < holder.DefinedGroups.Count; g++)
            {

                LocationGroup grp = holder.DefinedGroups[g];

                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.BeginHorizontal();

                bool showGroup = false;

                if (selectedGroupIndex == g)
                {

                    // Use the same rect and render "on top" of it so it gives the appearance of editing the label

                    GUIContent foldoutName = new GUIContent(holder.DefinedGroups[g].groupName);
                    GUIStyle style = new GUIStyle();
                    style.fixedHeight = 20f;

                    Rect itemRect = GUILayoutUtility.GetRect(foldoutName, style);
                    itemRect.x += 15f;
                    itemRect.width *= 0.4f;

                    // Set the edit button right next to it
                    Rect editButtonRect = itemRect;
                    editButtonRect.width = saveCancelButtonWidth;
                    editButtonRect.x = itemRect.width + saveCancelButtonWidth * 0.75f;

                    if (isEditingNameField)
                    {
                        tempEditingGroupName = GUI.TextField(itemRect, tempEditingGroupName);

                        if (GUI.Button(editButtonRect, doneButtonString))
                        {
                            if (!tempEditingGroupName.Equals(holder.DefinedGroups[g].groupName) && FindNameMatch(tempEditingGroupName))
                            {
                                Debug.LogWarning("Group name: " + tempEditingGroupName + " already exists");
                            }
                            else
                            {
                                holder.DefinedGroups[g].groupName = tempEditingGroupName;
                                isEditingNameField = false;
                                var scene = SceneManager.GetActiveScene();
                                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
                            }
                        }

                        editButtonRect.x += saveCancelButtonWidth + 5f;
                        editButtonRect.width += 10f;

                        if (GUI.Button(editButtonRect, cancelButtonString))
                        {
                            isEditingNameField = false;
                        }
                    }
                    else
                    {

                        GUI.Label(itemRect, holder.DefinedGroups[g].groupName);
                        if (GUI.Button(editButtonRect, editButtonTexContent))
                        {

                            tempEditingGroupName = holder.DefinedGroups[g].groupName;
                            isEditingNameField = true;
                        }
                    }

                    // Prepare to draw the Foldout
                    itemRect.x -= 15f;
                    itemRect.width = itemRect.width * 2.5f - addButtonWidth;

                    showGroup = EditorGUI.Foldout(itemRect, (selectedGroupIndex == g), "", true);


                    editButtonRect.xMin = itemRect.width + addButtonWidth * 0.3f;
                    //editButtonRect.xMax = itemRect.width +  addButtonWidth * 0.3f; // Offset it just a tad
                    editButtonRect.width = addButtonWidth;
                    // SET COLOR
                    //GUI.backgroundColor = xButtonColor;

                    if (GUI.Button(editButtonRect, "X"))
                    {
                        DeleteGroup(g);
                        showGroup = false;
                    }

                    //if(GUILayout.Button("X", GUILayout.Width(addButtonWidth))) {
                    //	DeleteGroup(g);
                    //}
                    GUI.backgroundColor = initialColor;
                }
                else
                {
                    showGroup = EditorGUILayout.Foldout((selectedGroupIndex == g), holder.DefinedGroups[g].groupName, true);
                }

                EditorGUILayout.EndHorizontal();


                if (showGroup)
                {

                    if (selectedGroupIndex != g)
                    {
                        selectedGroupIndex = g;
                        isEditingNameField = false;
                    }

                    #region Display the Whole Group

                    EditorGUILayout.Separator();

                    #region Display the New Spawn location

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.BeginVertical();
                    workingSpawnLocation.selectedSpawnType = (SelectedSpawnType)EditorGUILayout.EnumPopup(workingSpawnLocation.selectedSpawnType);

                    if (workingSpawnLocation.selectedSpawnType == SelectedSpawnType.Object)
                    {
                        workingSpawnLocation.selectedSpawnObject = (SelectedSpawnObject)EditorGUILayout.EnumPopup(workingSpawnLocation.selectedSpawnObject);
                    }
                    EditorGUILayout.EndVertical();

                    // Row 2

                    EditorGUILayout.BeginVertical();
                    workingSpawnLocation.selectedSpawnSize = (SelectedSpawnSize)EditorGUILayout.EnumPopup(workingSpawnLocation.selectedSpawnSize);

                    EditorGUILayout.BeginVertical();
                    if (workingSpawnLocation.selectedSpawnSize == SelectedSpawnSize.Custom)
                    {
                        //float x = EditorGUILayout.FloatField("x", workingSpawnLocation.customSize.x);
                        //float y = EditorGUILayout.FloatField("y", workingSpawnLocation.customSize.y);
                        //float z = EditorGUILayout.FloatField("z", workingSpawnLocation.customSize.z);

                        float x = EditorGUILayout.FloatField("Size", workingSpawnLocation.customSize.x);
                        float y = 0.0f;
                        float z = 0.0f;

                        workingSpawnLocation.customSize = new Vector3(x, y, z);
                    }

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndHorizontal();

                    #endregion

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Add To List", GUILayout.Width(addToListButtonWidth)))
                    {
                        SpawnLocation newSpawn = CreateSpawnLocation(workingSpawnLocation.selectedSpawnType, workingSpawnLocation.selectedSpawnSize, workingSpawnLocation.selectedSpawnObject);
                        newSpawn.customSize = workingSpawnLocation.customSize;
                        newSpawn.selectedSpawnObject = workingSpawnLocation.selectedSpawnObject;

                        holder.DefinedGroups[g].spawnLocations.Add(newSpawn);
                        var scene = SceneManager.GetActiveScene();
                        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
                    }

                    EditorGUILayout.EndHorizontal();

                    for (int s = 0; s < holder.DefinedGroups[g].spawnLocations.Count; s++)
                    {

                        EditorGUILayout.BeginHorizontal("Box");

                        SpawnLocation spawn = holder.DefinedGroups[g].spawnLocations[s];

                        StringBuilder spawnLabel = new StringBuilder();

                        spawnLabel.Append(spawn.selectedSpawnType == SelectedSpawnType.Object ?
                            spawn.selectedSpawnObject.ToString() :
                            spawn.selectedSpawnType.ToString());

                        spawnLabel.Append(" - ");

                        if (spawn.selectedSpawnSize == SelectedSpawnSize.Custom)
                        {
                            spawnLabel.AppendFormat("Custom Size: (" + spawn.customSize.x + ")");
                        }
                        else
                        {
                            spawnLabel.Append(spawn.selectedSpawnSize.ToString());
                        }

                        EditorGUILayout.LabelField(spawnLabel.ToString());

                        GUILayout.FlexibleSpace();
                        // SET COLOR
                        //GUI.backgroundColor = xButtonColor;
                        if (GUILayout.Button("X", GUILayout.Width(addButtonWidth)))
                        {
                            holder.DefinedGroups[g].spawnLocations.RemoveAt(s);
                            var scene = SceneManager.GetActiveScene();
                            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
                        }
                        GUI.backgroundColor = initialColor;
                        EditorGUILayout.EndHorizontal();

                    }
                    #endregion

                }
                else if (selectedGroupIndex == g)
                {
                    selectedGroupIndex = UNSELECTED;
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUI.indentLevel = previousIndentLevel;

            EditorGUILayout.EndScrollView();

            real.DefinedGroups = holder.DefinedGroups;

            //serializedObject.UpdateIfRequiredOrScript();
        }

        public SpawnLocation CreateSpawnLocation(SelectedSpawnType _spawnType, SelectedSpawnSize _selectedSpawnSize, SelectedSpawnObject _selectedSpawnObject )
        {
            switch (_spawnType)
            {
                case SelectedSpawnType.Wall:
                    return new WallSpawnLocation(_spawnType, _selectedSpawnSize);
                case SelectedSpawnType.Floor:
                    return new FloorSpawnLocation(_spawnType, _selectedSpawnSize);
                case SelectedSpawnType.Ceiling:
                    return new CeilingSpawnLocation(_spawnType, _selectedSpawnSize);
                case SelectedSpawnType.Air:
                    return new AirSpawnLocation(_spawnType, _selectedSpawnSize);
                case SelectedSpawnType.Object:
                    return new ObjectSpawnLocation(_spawnType, _selectedSpawnSize, _selectedSpawnObject);
                default:
                    break;
            }
            return new SpawnLocation(_spawnType, _selectedSpawnSize);
        }

        public void OnInspectorUpdate()
        {
            this.Repaint();
            var scene = SceneManager.GetActiveScene();
            real.DefinedGroups = holder.DefinedGroups;
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        }

        public void SetGroupIndex(int newIndex)
        {
            selectedGroupIndex = newIndex;
        }

        private bool FindNameMatch(string nameToMatch)
        {
            for (int i = 0; i < holder.DefinedGroups.Count; i++)
            {
                if (nameToMatch.Equals(holder.DefinedGroups[i].groupName))
                {
                    return true;
                }
            }
            return false;
        }

        public void AddNewGroup()
        {

            bool foundMatch = FindNameMatch(newGroupName);

            if (foundMatch)
            {
                // Add a number to the end
                int newIndex = 1;

                char[] digits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                string result = newGroupName.TrimEnd(digits);

                // Only try 100 times, if they keep doing it just stop, it's insane
                while (newIndex < 100)
                {
                    string newResult = result + newIndex;
                    foundMatch = FindNameMatch(newResult);
                    if (!foundMatch)
                    {

                        holder.DefinedGroups.Add(new LocationGroup(newResult));
                        break;
                    }

                    // We didn't find a match, increment and try again
                    newIndex++;
                }

            }
            else
            {
                holder.DefinedGroups.Add(new LocationGroup(newGroupName));
            }
            newGroupName = "";
            GUIUtility.keyboardControl = 0;
            var scene = SceneManager.GetActiveScene();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            real.DefinedGroups = holder.DefinedGroups;
        }

        public void DeleteGroup(int index)
        {
            holder.DefinedGroups.RemoveAt(index);
            var scene = SceneManager.GetActiveScene();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            real.DefinedGroups = holder.DefinedGroups;
        }
    }
}