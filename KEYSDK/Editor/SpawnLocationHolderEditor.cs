using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
#if UNITY_WSA && ENABLE_AR
using PracticalManaged.Practical.SpatialSpawning;

namespace Practical.Internal
{

    [CustomEditor(typeof(SpatialPlacementTool))]
    public class SpawnLocationHolderEditor : Editor
    {
        private int selectedGroupIndex;

        [SerializeField]
        private PracticalLocator holder;
        private string newGroupName;            // We we want to create a new group
        private string tempEditingGroupName;    // When we want to change an existing group


        [SerializeField]
        private Vector2 scrollPosition = Vector2.zero;

        private SpawnLocation workingSpawnLocation = new SpawnLocation();

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
            holder = (SpatialPlacementTool)target;
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

        public override void OnInspectorGUI()
        {
            float addToListButtonWidth = Screen.width * 0.4f;
            int previousIndentLevel = EditorGUI.indentLevel;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("New Group:");
            newGroupName = EditorGUILayout.TextField("", newGroupName, GUILayout.Width(155));

            //+ button click
            if (GUILayout.Button("+", GUILayout.Width(addButtonWidth)))
            {
                if (FindNameMatch(newGroupName))
                {
                    Debug.LogWarning("Group name: " + newGroupName + " already exists");
                }
                else if (newGroupName == "")
                {
                    Debug.LogWarning("Group name: can not be empty");
                }
                else
                {
                    AddNewGroup();
                }
                Undo.RecordObject(holder, "Added New Location Group");
            }
            GUILayout.Label("Unit");
            DistanceUnit tempUnit = (DistanceUnit)EditorGUILayout.EnumPopup(workingSpawnLocation.GetParam<DistanceUnit>("distanceUnit"), GUILayout.Width(50));
            workingSpawnLocation.SetParam("distanceUnit", tempUnit);

            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            Color initialColor = GUI.backgroundColor;

            for (int i = 0; i < holder.DefinedGroups.Count; i++)
            {
                LocationGroup grp = holder.DefinedGroups[i];

                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.BeginHorizontal();

                bool showGroup = false;

                //Edit existed element
                if (selectedGroupIndex == i)
                {
                    // Use the same rect and render "on top" of it so it gives the appearance of editing the label
                    GUIContent foldoutName = new GUIContent(holder.DefinedGroups[i].groupName);
                    GUIStyle style = new GUIStyle();
                    style.fixedHeight = 20f;

                    Rect itemRect = GUILayoutUtility.GetRect(foldoutName, style);
                    itemRect.x += 15f;
                    itemRect.width *= 0.4f;

                    // Set the edit button right next to it
                    Rect editButtonRect = itemRect;
                    editButtonRect.width = saveCancelButtonWidth;
                    editButtonRect.x = itemRect.width + saveCancelButtonWidth * 0.75f;

                    //Editing name field
                    if (isEditingNameField)
                    {
                        tempEditingGroupName = GUI.TextField(itemRect, tempEditingGroupName);

                        //Edit save button click
                        if (GUI.Button(editButtonRect, doneButtonString))
                        {
                            if (!tempEditingGroupName.Equals(holder.DefinedGroups[i].groupName) && FindNameMatch(tempEditingGroupName))
                            {
                                Debug.LogWarning("Group name: " + tempEditingGroupName + " already exists");
                            }
                            else if (tempEditingGroupName == "")
                            {
                                Debug.LogWarning("Group name: can not be empty");
                            }
                            else
                            {
                                holder.DefinedGroups[i].groupName = tempEditingGroupName;
                                isEditingNameField = false;
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
                        GUI.Label(itemRect, holder.DefinedGroups[i].groupName);
                        if (GUI.Button(editButtonRect, editButtonTexContent))
                        {
                            tempEditingGroupName = holder.DefinedGroups[i].groupName;
                            isEditingNameField = true;
                        }
                    }

                    // Prepare to draw the Foldout
                    itemRect.x -= 15f;
                    itemRect.width = itemRect.width * 2.5f - addButtonWidth;

                    showGroup = EditorGUI.Foldout(itemRect, (selectedGroupIndex == i), "", true);

                    editButtonRect.xMin = itemRect.width + addButtonWidth * 0.3f;
                    editButtonRect.width = addButtonWidth;
                    // SET COLOR
                    if (GUI.Button(editButtonRect, "X"))
                    {
                        DeleteGroup(i);
                        showGroup = false;
                    }
                    GUI.backgroundColor = initialColor;
                }
                else
                {
                    showGroup = EditorGUILayout.Foldout((selectedGroupIndex == i), holder.DefinedGroups[i].groupName, true);
                }

                EditorGUILayout.EndHorizontal();

                //if one of them is selected
                if (showGroup)
                {
                    if (selectedGroupIndex != i)
                    {
                        selectedGroupIndex = i;
                        isEditingNameField = false;
                    }

                    #region Display the Whole Group

                    EditorGUILayout.Separator();

                    #region Display the New Spawn location

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginVertical("Box");

                    GUILayout.Label("Spawn Type");
                    EditorGUILayout.BeginHorizontal();
                    workingSpawnLocation.SetParam("spawnType", EditorGUILayout.EnumPopup(workingSpawnLocation.GetParam<SpawnType>("spawnType"), GUILayout.MaxWidth(130)));
                    EditorGUILayout.EndHorizontal();

                    if (workingSpawnLocation.GetParam<SpawnType>("spawnType") == SpawnType.Wall)
                    {
                        GUILayout.Label("Min Height - Max Height");
                        EditorGUILayout.BeginHorizontal();
                        workingSpawnLocation.SetParam("heightMin", EditorGUILayout.FloatField("", workingSpawnLocation.GetParam<float>("heightMin"), GUILayout.MaxWidth(60)));
                        workingSpawnLocation.SetParam("heightMax", EditorGUILayout.FloatField("", workingSpawnLocation.GetParam<float>("heightMax"), GUILayout.MaxWidth(60)));
                        EditorGUILayout.EndHorizontal();
                    }
                    if (workingSpawnLocation.GetParam<SpawnType>("spawnType") == SpawnType.RoomObjects)
                    {
                        GUILayout.Label("Spawn Room Object");
                        workingSpawnLocation.SetParam("spawnObject", EditorGUILayout.EnumPopup(workingSpawnLocation.GetParam<SpawnObject>("spawnObject"), GUILayout.MaxWidth(130)));
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();

                    // Row 2
                    EditorGUILayout.BeginVertical("Box");
                    EditorGUILayout.BeginVertical();
                    GUILayout.Label("Spawn Option");
                    workingSpawnLocation.SetParam("fromOption", EditorGUILayout.EnumPopup(workingSpawnLocation.GetParam<FromOption>("fromOption"), GUILayout.MaxWidth(130)));

                    if (workingSpawnLocation.GetParam<FromOption>("fromOption") == FromOption.Away)
                    {
                        GUILayout.Label("From");
                        workingSpawnLocation.SetParam("awayFrom", EditorGUILayout.EnumPopup(workingSpawnLocation.GetParam<AwayFrom>("awayFrom"), GUILayout.MaxWidth(130)));
                    }
                    else if (workingSpawnLocation.GetParam<FromOption>("fromOption") == FromOption.Near)
                    {
                        GUILayout.Label("To");
                        workingSpawnLocation.SetParam("nearTo", EditorGUILayout.EnumPopup(workingSpawnLocation.GetParam<NearTo>("nearTo"), GUILayout.MaxWidth(130)));
                    }

                    if (workingSpawnLocation.GetParam<FromOption>("fromOption") != FromOption.None && (workingSpawnLocation.GetParam<AwayFrom>("awayFrom") != AwayFrom.None || workingSpawnLocation.GetParam<AwayFrom>("nearTo") != AwayFrom.None))
                    {
                        if ((workingSpawnLocation.GetParam<FromOption>("fromOption") == FromOption.Away && workingSpawnLocation.GetParam<AwayFrom>("awayFrom") == AwayFrom.GameObject) || (workingSpawnLocation.GetParam<FromOption>("fromOption") == FromOption.Near && workingSpawnLocation.GetParam<NearTo>("nearTo") == NearTo.GameObject))
                        {
                            GUILayout.Label("GameObject");
                            workingSpawnLocation.fromObject = (GameObject)EditorGUILayout.ObjectField(workingSpawnLocation.fromObject, typeof(GameObject), true, GUILayout.MaxWidth(130));
                        }

                        if (workingSpawnLocation.GetParam<FromOption>("fromOption") == FromOption.Near && workingSpawnLocation.GetParam<NearTo>("nearTo") != NearTo.None)
                        {
                            GUILayout.Label("Min Distance - Max Distance");
                            EditorGUILayout.BeginHorizontal();
                            workingSpawnLocation.SetParam("minDistance", EditorGUILayout.FloatField("", workingSpawnLocation.GetParam<float>("minDistance"), GUILayout.MaxWidth(60)));
                            workingSpawnLocation.SetParam("maxDistance", EditorGUILayout.FloatField("", workingSpawnLocation.GetParam<float>("maxDistance"), GUILayout.MaxWidth(60)));
                            EditorGUILayout.EndHorizontal();
                        }
                        else if (workingSpawnLocation.GetParam<FromOption>("fromOption") == FromOption.Away && workingSpawnLocation.GetParam<AwayFrom>("awayFrom") != AwayFrom.None)
                        {
                            GUILayout.Label("Min Distance");
                            EditorGUILayout.BeginHorizontal();
                            workingSpawnLocation.SetParam("minDistance", EditorGUILayout.FloatField("", workingSpawnLocation.GetParam<float>("minDistance"), GUILayout.MaxWidth(60)));
                            EditorGUILayout.EndHorizontal();
                        }

                        if ((workingSpawnLocation.GetParam<FromOption>("fromOption") == FromOption.Near && workingSpawnLocation.GetParam<NearTo>("nearTo") == NearTo.Walls) || (workingSpawnLocation.GetParam<FromOption>("fromOption") == FromOption.Away && workingSpawnLocation.GetParam<AwayFrom>("awayFrom") == AwayFrom.Walls))
                        {
                            GUILayout.Label("Min Wall Height");
                            EditorGUILayout.BeginHorizontal();
                            workingSpawnLocation.SetParam("minWallHeight", EditorGUILayout.FloatField("", workingSpawnLocation.GetParam<float>("minWallHeight"), GUILayout.MaxWidth(60)));
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.Label("Size");
                    workingSpawnLocation.SetParam("spawnSize", EditorGUILayout.EnumPopup(workingSpawnLocation.GetParam<SpawnSize>("spawnSize"), GUILayout.MaxWidth(130)));

                    EditorGUILayout.BeginVertical();
                    if (workingSpawnLocation.GetParam<SpawnSize>("spawnSize") == SpawnSize.Custom)
                    {
                        GUILayout.Label("Custom Size");
                        workingSpawnLocation.SetParam("halfDims", EditorGUILayout.Vector3Field("", workingSpawnLocation.GetParam<Vector3>("halfDims")));
                    }
                    else if (workingSpawnLocation.GetParam<SpawnSize>("spawnSize") == SpawnSize.CopyFromObject)
                    {
                        GUILayout.Label("Size From GameObject");
                        workingSpawnLocation.customSizeFromObject = (GameObject)EditorGUILayout.ObjectField(workingSpawnLocation.customSizeFromObject, typeof(GameObject), true, GUILayout.MaxWidth(130));
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    #endregion

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    //Add To List button click
                    if (GUILayout.Button("Add To List", GUILayout.Width(addToListButtonWidth)))
                    {
                        if (workingSpawnLocation.GetParam<SpawnType>("spawnType") == SpawnType.RoomObjects && workingSpawnLocation.GetParam<SpawnObject>("spawnObject") == SpawnObject.None)
                        {
                            Debug.LogWarning("Spawn room object has to be selected");
                        }
                        else
                        {
                            SpawnLocation newSpawn = CreateSpawnLocation(workingSpawnLocation.GetParam<SpawnType>("spawnType"));
                            newSpawn.SetParam("halfDims", workingSpawnLocation.GetParam<Vector3>("halfDims"));
                            newSpawn.SetParam("minDistance", workingSpawnLocation.GetParam<float>("minDistance"));
                            newSpawn.SetParam("spawnType", workingSpawnLocation.GetParam<SpawnType>("spawnType"));
                            newSpawn.SetParam("spawnSize", workingSpawnLocation.GetParam<SpawnSize>("spawnSize"));
                            newSpawn.SetParam("fromOption", workingSpawnLocation.GetParam<FromOption>("fromOption"));
                            newSpawn.SetParam("distanceUnit", workingSpawnLocation.GetParam<DistanceUnit>("distanceUnit"));


                            if (workingSpawnLocation.GetParam<FromOption>("fromOption") == FromOption.Near && workingSpawnLocation.GetParam<NearTo>("nearTo") != NearTo.None)
                            {
                                newSpawn.SetParam("maxDistance", workingSpawnLocation.GetParam<float>("maxDistance"));
                            }

                            if (workingSpawnLocation.GetParam<SpawnType>("spawnType") == SpawnType.Wall)
                            {
                                newSpawn.SetParam("heightMin", workingSpawnLocation.GetParam<float>("heightMin"));
                                newSpawn.SetParam("heightMax", workingSpawnLocation.GetParam<float>("heightMax"));
                            }

                            if (workingSpawnLocation.GetParam<FromOption>("fromOption") != FromOption.None)
                            {
                                newSpawn.SetParam("awayFrom", workingSpawnLocation.GetParam<AwayFrom>("awayFrom"));
                                newSpawn.SetParam("nearTo", workingSpawnLocation.GetParam<NearTo>("nearTo"));
                                if (workingSpawnLocation.GetParam<AwayFrom>("awayFrom") == AwayFrom.GameObject || workingSpawnLocation.GetParam<NearTo>("nearTo") == NearTo.GameObject)
                                {
                                    newSpawn.fromObject = workingSpawnLocation.fromObject;
                                }

                                if ((workingSpawnLocation.GetParam<FromOption>("fromOption") == FromOption.Near && workingSpawnLocation.GetParam<NearTo>("nearTo") == NearTo.Walls) || (workingSpawnLocation.GetParam<FromOption>("fromOption") == FromOption.Away && workingSpawnLocation.GetParam<AwayFrom>("awayFrom") == AwayFrom.Walls))
                                {
                                    newSpawn.SetParam("minWallHeight", workingSpawnLocation.GetParam<float>("minWallHeight"));
                                }
                            }

                            if (workingSpawnLocation.GetParam<SpawnType>("spawnType") == SpawnType.RoomObjects)
                            {
                                newSpawn.SetParam("spawnObject", workingSpawnLocation.GetParam<SpawnObject>("spawnObject"));
                            }

                            if (workingSpawnLocation.GetParam<SpawnSize>("spawnSize") == SpawnSize.CopyFromObject)
                            {
                                newSpawn.customSizeFromObject = workingSpawnLocation.customSizeFromObject;
                            }
                            holder.DefinedGroups[i].spawnLocations.Add(newSpawn);
                            Undo.RecordObject(holder, "Added Spawn Type");
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    //Added subspawn elements show part
                    for (int j = 0; j < holder.DefinedGroups[i].spawnLocations.Count; j++)
                    {
                        EditorGUILayout.BeginHorizontal("Box");
                        SpawnLocation spawn = holder.DefinedGroups[i].spawnLocations[j];
                        StringBuilder spawnLabel = new StringBuilder();
                        spawnLabel.Append(spawn.GetParam<SpawnType>("spawnType") == SpawnType.RoomObjects ?
                            spawn.GetParam<SpawnObject>("spawnObject").ToString() :
                            spawn.GetParam<SpawnType>("spawnType").ToString());

                        spawnLabel.Append("-");
                        if (spawn.GetParam<FromOption>("fromOption") == FromOption.Away && spawn.GetParam<AwayFrom>("awayFrom") != AwayFrom.None)
                        {
                            spawnLabel.Append(spawn.GetParam<AwayFrom>("awayFrom").ToString() + "(" + spawn.GetParam<float>("minDistance").ToString() + ")");
                            spawnLabel.Append("-");
                        }
                        else if (spawn.GetParam<FromOption>("fromOption") == FromOption.Near && spawn.GetParam<NearTo>("nearTo") != NearTo.None)
                        {
                            spawnLabel.Append(spawn.GetParam<NearTo>("nearTo").ToString() + "(" + spawn.GetParam<float>("minDistance").ToString() + "-" + spawn.GetParam<float>("maxDistance").ToString() + ")");
                            spawnLabel.Append("-");
                        }

                        if (spawn.GetParam<SpawnSize>("spawnSize") == SpawnSize.Custom)
                        {
                            spawnLabel.AppendFormat("Custom Size:(" + spawn.GetParam<Vector3>("halfDims").x + "-" + spawn.GetParam<Vector3>("halfDims").y + "-" + spawn.GetParam<Vector3>("halfDims").z + ")");
                        }
                        else
                        {
                            spawnLabel.Append(spawn.GetParam<SpawnSize>("spawnSize").ToString());
                        }
                        GUIStyle guiStyle = new GUIStyle();
                        guiStyle.fontSize = 9;
                        GUILayout.Label(spawnLabel.ToString(), guiStyle);
                        GUILayout.FlexibleSpace();
                        // SET COLOR
                        if (GUILayout.Button("X", GUILayout.Width(addButtonWidth)))
                        {
                            holder.DefinedGroups[i].spawnLocations.RemoveAt(j);
                        }
                        GUI.backgroundColor = initialColor;
                        EditorGUILayout.EndHorizontal();
                    }
                    #endregion
                }
                else if (selectedGroupIndex == i)
                {
                    selectedGroupIndex = UNSELECTED;
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUI.indentLevel = previousIndentLevel;
            EditorGUILayout.EndScrollView();

            if (holder != null)
            {
                Undo.RecordObject(holder, "Updated Location Group");
            }
        }

        public SpawnLocation CreateSpawnLocation(SpawnType _spawnType)
        {
            SpawnLocation retVal = null;
            switch (_spawnType)
            {
                case SpawnType.Wall:
                    retVal = new WallSpawnLocation();
                    break;
                case SpawnType.Floor:
                    retVal = new FloorSpawnLocation();
                    break;
                case SpawnType.Ceiling:
                    retVal = new CeilingSpawnLocation();
                    break;
                case SpawnType.Air:
                    retVal = new AirSpawnLocation();
                    break;
                case SpawnType.RoomObjects:
                    retVal = new RoomObjectsSpawnLocation();
                    break;
                default:
                    retVal = new SpawnLocation();
                    break;
            }
            return retVal;
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
                var newGrp = new LocationGroup(newGroupName);
                holder.DefinedGroups.Add(newGrp);
            }
            GUIUtility.keyboardControl = 0;
            Undo.RecordObject(holder, "Added Location Group");
            newGroupName = "";
        }

        public void DeleteGroup(int index)
        {
            holder.DefinedGroups.RemoveAt(index);
            Undo.RecordObject(holder, "Deleted Location Group");
        }
    }
}
#endif