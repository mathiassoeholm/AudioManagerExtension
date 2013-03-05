using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

[InitializeOnLoad]
public class AudioWindow : EditorWindow
{
    static List<AudioItem> audioItems = new List<AudioItem>();

    private static string currentScene;

    private Vector2 scrollPos;

    private AudioItem itemToRemove;

    private static AudioWindow window;

    [MenuItem("Window/Audio Manager")]
	static void Init ()
    {
        // Get existing open window or if none, make a new one:
        window = (AudioWindow)GetWindow(typeof(AudioWindow));

        // Set window title
        window.title = "Audio Manager";

        // Load audio items
        LoadAllAudioItems();

        // Apply sounds to the manager prefab
        ApplySoundsToManagerPrefab();

        Debug.Log("loaded");
	}

    static AudioWindow()
    {
        // Set current scene field
        currentScene = EditorApplication.currentScene;

        // Load audio items
        LoadAllAudioItems();

        Debug.Log("static constructor");
    }

    private void Update()
    {
        // Check if we changed scene
        if (currentScene != EditorApplication.currentScene)
        {
            Debug.Log("Changing scene from " + currentScene);
            
            // Update which scene we are in
            currentScene = EditorApplication.currentScene;

            // Call scene change method
            OnSceneChange();
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

        // Runs through a copy of the audio item collection, this enables us to remove from the original collection
        foreach (AudioItem audioItem in audioItems.ToArray())
        {
            // Begin a horizontal layout for path, play btn and remove btn
            EditorGUILayout.BeginHorizontal();
            
            // FilePath label
            EditorGUILayout.LabelField(Path.GetFileName(audioItem.FilePath), EditorStyles.boldLabel);

            // If the sound is playing, allow us to stop it
            if (AudioManager.Instance.IsAudioItemPlaying(audioItem))
            {
                if (GUILayout.Button("Stop"))
                {
                    AudioManager.Instance.StopAudioItem(audioItem);
                    requireRepaint = true;
                }
            }
            else if (GUILayout.Button("Play"))
            {
                AudioManager.Instance.PlaySound(audioItem);

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
                    RemoveAudioItem(itemToRemove);

                    // Stop removing an item
                    itemToRemove = null;
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
    /// We call this when changing the scene.
    /// </summary>
    private void OnSceneChange()
    {
        Debug.Log("Changed scene to " + currentScene);
        
        // Apply sounds to manager once again
        ApplySoundsToManagerPrefab();
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
            ) || audioItems.Select(item => item.FilePath).Contains(filePath))
        {
            // Return if the file is already in the list
            return;
        }

        // Add the file to the list
        audioItems.Add(new AudioItem {FilePath = filePath, Volume = 1} );

        Debug.Log("Added " + filePath);
    }

    public static void RemoveAudioItem(AudioItem item)
    {
        // Stop any sources with this sound from playing
        AudioManager.Instance.StopAudioItem(item);

        // Delete the audio items saved data
        item.DeleteSaveData();

        // Remove the audio item from the original list
        audioItems.Remove(item);

        // Reload audio items
        LoadAllAudioItems();

        // Apply sounds to the manager
        ApplySoundsToManagerPrefab();
    }

    /// <summary>
    /// Apply all changes made, and generate the required code.
    /// </summary>
    private void ApplyChanges()
    {
        // Stop removing any item
        itemToRemove = null;
        
        // Stop all audio sources from playing
        AudioManager.Instance.StopAllSounds();
        
        // Save audio items
        SaveAllAudioItems();

        // Generate partial AudioManager class
        GenerateCode();

        // Apply modified properties to audio manager prefab
        ApplySoundsToManagerPrefab();
        
        // Repaint
        Repaint();
    }

    /// <summary>
    /// Applies sounds to the audio manager prefab, and instantiates a new one.
    /// </summary>
    static void ApplySoundsToManagerPrefab()
    {
        // Get the audio manager prefab
        var audioManager = (GameObject)Resources.LoadAssetAtPath("Assets/Plugins/AudioManager/AudioManager.prefab", typeof(GameObject));

        // Apply the audio items to the prefab
        audioManager.GetComponent<AudioManager>().AudioItems = audioItems.ToArray();

        //Create a new instance of the prefab
        AudioManager.CreateNewInstance(audioManager);
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

            for (int i = 0; i < audioItems.Count; i++)
            {
                // PLay sound method
                writer.WriteLine(@"    public static void Play{0}()", Path.GetFileNameWithoutExtension(audioItems[i].FilePath));
                writer.WriteLine(@"    {");
                writer.WriteLine(@"        PlaySound({0});", i);
                writer.WriteLine(@"    }");

                // Stop sound method
                writer.WriteLine(@"    public static void Stop{0}()", Path.GetFileNameWithoutExtension(audioItems[i].FilePath));
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
    /// Loads all the audio items from their appropriate text files.
    /// </summary>
    private static void LoadAllAudioItems()
    {
        audioItems = new List<AudioItem>();

        // If the directory doesn't exist nothing has been saved and we can return
        if (!Directory.Exists(@"ProjectSettings/AudioItems"))
        {
            return;
        }

        foreach (string dataPath in Directory.GetFiles(@"ProjectSettings/AudioItems"))
        {
            // Load audio item from the path
            var audioItem = new AudioItem();
            audioItem.LoadItem(dataPath);

            // Add te loaded item to the list
            audioItems.Add(audioItem);
        }
    }

    /// <summary>
    /// Saves all audio items to text files.
    /// </summary>
    void SaveAllAudioItems()
    {
        // Save items
        foreach (AudioItem audioItem in audioItems.ToArray())
        {
            audioItem.SaveItem();
        }
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

                    // Call OnDropAudioFile for each file that was dropped
                    foreach (string path in DragAndDrop.paths)
                    {
                        OnDropAudioFile(path);
                    }

                    // Save state of audio items
                    SaveAllAudioItems();

                    // Apply changes
                    ApplyChanges();
                }

                Event.current.Use();
                break;
        }
    }
}