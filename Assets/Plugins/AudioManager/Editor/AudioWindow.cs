using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public class AudioWindow : EditorWindow
{
    private Vector2 scrollPos;

    private AudioItem itemToRemove;

    private static AudioWindow window;

    private AudioManager audioManagerInScene;

    private AudioManager audioManagerPrefab;

    private AudioManager AudioManagerPrefab
    {
        get
        {
            if (audioManagerPrefab == null)
            {
                // Load asset
                audioManagerPrefab =
                    (Resources.LoadAssetAtPath("Assets/Plugins/AudioManager/AudioManager.prefab", typeof (GameObject))
                     as GameObject).GetComponent<AudioManager>();
            }

            return audioManagerPrefab;
        }
    }

    [MenuItem("Window/Audio Manager")]
	static void Init ()
    {
        // Get existing open window or if none, make a new one:
        window = (AudioWindow)GetWindow(typeof(AudioWindow));

        // Set window title
        window.title = "Audio Manager";
	}

    private void Update()
    {
        // Check if there is an audio manager in the scene
        if (audioManagerInScene == null)
        {
            // Check if amount of audio managers in the scene is corred
            Object[] audioManagers = FindSceneObjectsOfType(typeof(AudioManager));

            if (audioManagers.Length == 0)
            {
                // Instantiate an audiomanager in the scene
                audioManagerInScene = (Instantiate(AudioManagerPrefab.gameObject) as GameObject).GetComponent<AudioManager>();
            }
            else if (audioManagers.Length >= 1)
            {
                for (int i = audioManagers.Length - 1; i >= 0; i--)
                {
                    if (i == 0)
                    {
                        // Assign to found audio manager
                        audioManagerInScene = (audioManagers[i] as GameObject).GetComponent<AudioManager>();
                    }
                    else
                    {
                        // Remove potential extra audio managers
                        DestroyImmediate(audioManagers[i] as GameObject);
                    }
                }
            }
        }
        else
        {
            // Clone audio items from prefab to scene instance
            audioManagerInScene.AudioItems = audioManagerPrefab.AudioItems;
        }
    }

    private void OnGUI()
    {
        // This gets set to true if the gui needs repaint during this method
        bool requireRepaint = false;

        // Cache the old color
        Color oldColor = GUI.color;

        // Renders the drop area
        DropAreaGui();

        // Begin a scrollview
        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandWidth(true));

        bool removeITemToRemove = false;

        // Runs through a copy of the audio item collection, this enables us to remove from the original collection
        foreach (AudioItem audioItem in AudioManagerPrefab.AudioItems)
        {
            // Begin a horizontal layout for path, play btn and remove btn
            EditorGUILayout.BeginHorizontal();
            
            // FilePath label
            EditorGUILayout.LabelField(Path.GetFileName(audioItem.FilePath), EditorStyles.boldLabel);

            // If the sound is playing, allow us to stop it
            if (AudioManagerPrefab.IsAudioItemPlaying(audioItem))
            {
                if (GUILayout.Button("Stop"))
                {
                    AudioManagerPrefab.StopAudioItem(audioItem);
                    requireRepaint = true;
                }
            }
            else if (GUILayout.Button("Play"))
            {
                AudioManagerPrefab.PlaySound(audioItem);

                requireRepaint = true;
            }

            // Make sure this audio item is not currently being removed
            if (audioItem != itemToRemove)
            {
                // Change color to red
                GUI.color = new Color(1, 0.3f, 0.3f);
                
                if (GUILayout.Button("Remove"))
                {
                    requireRepaint = true;

                    itemToRemove = audioItem;
                }

                // Reset color
                GUI.color = oldColor;
            }
            else
            {
                // Don't remove if nope is pressed
                if (GUILayout.Button("Nope"))
                {
                    itemToRemove = null;
                }
                
                // Change color to red
                GUI.color = new Color(1, 0.3f, 0.3f);
                
                // Remove the item if ok is pressed
                if (GUILayout.Button("Ok"))
                {
                    // This will make sure we remove the audio item, after loop has finished
                    removeITemToRemove = true;
                }

                // Reset color
                GUI.color = oldColor;
            }

            // End the top horizontal layout for this item
            EditorGUILayout.EndHorizontal();

            // Volume slider
            audioItem.Volume = EditorGUILayout.Slider("Volume", audioItem.Volume, 0, 1);

            // Looping toggle
            audioItem.Loop = EditorGUILayout.Toggle("Looping", audioItem.Loop);

            // Play on awake toggle
            audioItem.PlayOnAwake = EditorGUILayout.Toggle("Play on Awake", audioItem.PlayOnAwake);
        }

        if (removeITemToRemove)
        {
            RemoveAudioItem(itemToRemove);

            // Stop removing an item
            itemToRemove = null;
        }

        // Apply all changes if we hit the apply button
        if (GUILayout.Button("Apply"))
        {
            ApplyChanges();
        }

        // End scrollable view
        GUILayout.EndScrollView();

        if (requireRepaint)
        {
            Repaint();
        }
    }

    /// <summary>
    /// This is automaticly called when the window loses focus.
    /// </summary>
    private void OnLostFocus()
    {
        // Cancel removing an item when window loses focus
        itemToRemove = null;

        // Repaint the window
        Repaint();
    }

    /// <summary>
    /// This method gets called for each audio file you drop in the drop area.
    /// </summary>
    /// <param name="filePath">
    /// The path of the file dropped.
    /// </param>
    void OnDropAudioFile(string filePath)
    {
        // Get the extension of the dropped file
        string extension = Path.GetExtension(filePath);

        // Check for a valid file extension, and make sure the file isn't already added
        if(
            (  extension != ".wav"
            && extension != ".ogg"
            && extension != ".wma"
            && extension != ".mp3"
            ) || audioManagerPrefab.AudioItems.Select(item => item.FilePath).Contains(filePath))
        {
            // Return if the file is already in the list
            return;
        }

        // Add an audio item to the array
        List<AudioItem> audioItems = audioManagerPrefab.AudioItems.ToList();
        audioItems.Add(new AudioItem { FilePath = filePath, Volume = 1 });
        audioManagerPrefab.AudioItems = audioItems.ToArray();
        
        Debug.Log("Added " + filePath);
    }

    /// <summary>
    /// Removes an audio item and deletes its save data.
    /// </summary>
    /// <param name="item">
    /// The item to delete.
    /// </param>
    public void RemoveAudioItem(AudioItem item)
    {
        // Stop any sources with this sound from playing
        AudioManagerPrefab.StopAudioItem(item);

        // Remove an audio item from the array
        List<AudioItem> audioItems = audioManagerPrefab.AudioItems.ToList();
        audioItems.Remove(item);
        audioManagerPrefab.AudioItems = audioItems.ToArray();
    }

    /// <summary>
    /// Apply all changes made, and generate the required code.
    /// </summary>
    private void ApplyChanges()
    {
        // Stop removing any item
        itemToRemove = null;
        
        // Generate partial AudioManager class
        GenerateCode();
        
        // Repaint
        Repaint();
    }

    /// <summary>
    /// Creates the generated AudioManagerGenerated.cs file
    /// </summary>
    private void GenerateCode()
    {
        // Generate AudioManager methods
        using (TextWriter writer = File.CreateText(@"Assets\Plugins\AudioManager\AudioManagerGenerated.cs"))
        {
            writer.WriteLine(@"// THIS FILE IS AUTO GENERATED, DO NO MODIFY!");
            writer.WriteLine(@"public partial class AudioManager");
            writer.WriteLine(@"{");

            for (int i = 0; i < AudioManagerPrefab.AudioItems.Length; i++)
            {
                // PLay sound method
                writer.WriteLine(@"    public static void Play{0}()", Path.GetFileNameWithoutExtension(AudioManagerPrefab.AudioItems[i].FilePath));
                writer.WriteLine(@"    {");
                writer.WriteLine(@"        PlaySound({0},{1}f);", i, AudioManagerPrefab.AudioItems[i].Volume);
                writer.WriteLine(@"    }");

                // PLay sound method
                writer.WriteLine(@"    public static void Play{0}(float volume)", Path.GetFileNameWithoutExtension(AudioManagerPrefab.AudioItems[i].FilePath));
                writer.WriteLine(@"    {");
                writer.WriteLine(@"        PlaySound({0},volume);", i);
                writer.WriteLine(@"    }");

                // Stop sound method
                writer.WriteLine(@"    public static void Stop{0}()", Path.GetFileNameWithoutExtension(AudioManagerPrefab.AudioItems[i].FilePath));
                writer.WriteLine(@"    {");
                writer.WriteLine(@"        StopSound({0});", i);
                writer.WriteLine(@"    }");
            }

            writer.WriteLine(@"}");
        }

        // Reimport generated file
        AssetDatabase.ImportAsset(@"Assets\Plugins\AudioManager\AudioManagerGenerated.cs", ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
    }

    /// <summary>
    /// Renders and handles events for the drop area gui.
    /// </summary>
    private void DropAreaGui()
    {
        Event evt = Event.current;

        // Draw drop area box
        Rect dropArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drop sounds here");

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                // Check if the mouse position is within the droparea when drag is performed
                if (!dropArea.Contains(evt.mousePosition))
                {
                    break;
                }

                // Enable drag and drop by setting the visual mode to copy
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                // Drag perform is sent when mouse button is lifted
                if (evt.type == EventType.DragPerform)
                {
                    // Register that the drag event has been handled
                    DragAndDrop.AcceptDrag();

                    // Call OnDropAudioFile for each file that was dropped in alphabetical order
                    foreach (string path in DragAndDrop.paths.ToList().OrderBy(s => s))
                    {
                        OnDropAudioFile(path);
                    }
                }

                Event.current.Use();
                break;
        }
    }
}