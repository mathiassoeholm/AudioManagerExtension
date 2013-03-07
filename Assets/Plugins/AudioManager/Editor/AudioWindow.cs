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

    private int selectedAudioIndex;

    private bool requireRepaint;

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

                        Debug.Log((audioManagers[i]).ToString());

                        // Assign to found audio manager
                        audioManagerInScene = (audioManagers[i] as AudioManager);
                    }
                    else
                    {
                        // Remove potential extra audio managers
                        DestroyImmediate((audioManagers[i] as AudioManager).gameObject);
                    }
                }
            }
        }
        else
        {
            // Clone audio items and settings from prefab to scene instance
            audioManagerInScene.AudioItems = audioManagerPrefab.AudioItems;
            audioManagerInScene.Settings = audioManagerPrefab.Settings;
        }
    }

    private void OnGUI()
    {
        // This gets set to true if the gui needs repaint during this method
        requireRepaint = false;

        DrawSeperator("Global Settings");

        GUILayout.BeginHorizontal();
        AudioManagerPrefab.Settings.MasterVolume = 0.01f * EditorGUILayout.Slider("Volume", AudioManagerPrefab.Settings.MasterVolume * 100, 0, 100);
        GUILayout.Label("%");
        GUILayout.EndHorizontal();


        DrawSeperator("Audio Files");


        if (AudioManagerPrefab.AudioItems.Length > 0)
        {
            selectedAudioIndex = EditorGUILayout.Popup(selectedAudioIndex,
                                                     AudioManagerPrefab.AudioItems.Select(a => a.Name).ToArray());
 
            Debug.Log("Selected audio index " + selectedAudioIndex);

            DrawAudioItemGui(AudioManagerPrefab.AudioItems[selectedAudioIndex]);
        }

        // Renders the drop area
        DropAreaGui();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(AudioManagerPrefab.gameObject);
            
            Debug.Log("Dirty bitches");

            Repaint();
        }
    }

    private void DrawAudioItemGui(AudioItem audioItem)
    {
        // Cache the old color
        Color oldColor = GUI.color;

        EditorGUILayout.BeginHorizontal();

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
                RemoveAudioItem(audioItem);

                
            }

            // Reset color
            GUI.color = oldColor;
        }

        // End the top horizontal layout for this item
        EditorGUILayout.EndHorizontal();

        // Volume slider
        EditorGUILayout.BeginHorizontal();
        audioItem.Volume = 0.01f * EditorGUILayout.Slider("Volume", audioItem.Volume * 100, 0, 100);
        GUILayout.Label("%");
        EditorGUILayout.EndHorizontal();

        // Looping toggle
        audioItem.Loop = EditorGUILayout.Toggle("Looping", audioItem.Loop);

        // Play on awake toggle
        audioItem.PlayOnAwake = EditorGUILayout.Toggle("Play on Awake", audioItem.PlayOnAwake);
    }

    private void DrawSeperator(string text)
    {
        // Cache the old color
        Color oldColor = GUI.color;
        
        GUI.color = new Color(0.7f, 0.7f, 1);

        Rect seperator = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
        GUI.Box(seperator, text);

        // Reset color
        GUI.color = oldColor;
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
        AudioItem newAudiItem = new AudioItem {FilePath = filePath, Volume = 1};
        newAudiItem.LoadAudioClipFromPath();
        audioItems.Add(newAudiItem);
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

        selectedAudioIndex = 0;

        ApplyChanges();
    }

    /// <summary>
    /// Apply all changes made, and generate the required code.
    /// </summary>
    private void ApplyChanges()
    {
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
        Rect dropArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
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

                ApplyChanges();

                EditorUtility.SetDirty(AudioManagerPrefab.gameObject);
            
                Debug.Log("Dirty bitches");

                Event.current.Use();
                break;
        }
    }
}