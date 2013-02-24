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

    private Vector2 scrollPos;

    private AudioItem itemToRemove;

    private GameObject audioManagerInstance;

    private static string currentScene;

    private static AudioWindow window;

    [MenuItem("Window/Audio Manager")]
	static void Init ()
    {
        // Get existing open window or if none, make a new one:
        window = (AudioWindow)GetWindow(typeof(AudioWindow));

        // Load audio items
        LoadAllAudioItems();

        window.title = "Audio Manager";

        window.position = new Rect(0,0, 200, 200);

        Debug.Log("loaded");
	}

    static AudioWindow()
    {
        currentScene = EditorApplication.currentScene;
        
        // Load audio items
        LoadAllAudioItems();

        //ApplySoundsToManager();

        Debug.Log("static constructor");
    }

    private void OnDestroy()
    {
        SaveAllAudioItems();

        Debug.Log("Destroyed");
    }

    private void OnLostFocus()
    {
        // Cancel removing an item when window loses focus
        itemToRemove = null;
        Repaint();
    }

    void OnDropAudioFile(string filePath)
    {
        string extension = Path.GetExtension(filePath);

        if(
            (  extension != ".wav"
            && extension != ".ogg"
            && extension != ".wma"
            && extension != ".mp3"
            ) || audioItems.Select(item => item.Path).Contains(filePath))
        {
            // Return if the file is already in the list
            return;
        }

        // Add the file to the list
        audioItems.Add(new AudioItem {Path = filePath, Volume = 1} );

        // Save audio item
        audioItems.Last().SaveItem(audioItems.Count - 1);

        // Save new amount
        EditorPrefs.SetInt("amountOfAudioItems", audioItems.Count);
    }

    static void ApplySoundsToManager()
    {
        var audioManager = (GameObject)Resources.LoadAssetAtPath("Assets/Plugins/AudioManager/AudioManager.prefab", typeof(GameObject));

        audioManager.GetComponent<AudioManager>().AudioItems = audioItems.ToArray();

        // Generate AudioManager meethods
        using (TextWriter writer = File.CreateText(@"Assets\Plugins\AudioManager\AudioManagerGenerated.cs"))
        {
            writer.WriteLine(@"// THIS FILE IS AUTO GENERATED, DO NO MODIFY!");
            writer.WriteLine(@"public partial class AudioManager");
            writer.WriteLine(@"{");

            for (int i = 0; i < audioItems.Count; i++)
            {
                writer.WriteLine(@"    public static void Play{0}()", Path.GetFileNameWithoutExtension(audioItems[i].Path));
                writer.WriteLine(@"    {");
                writer.WriteLine(@"        PlaySound({0});", i);
                writer.WriteLine(@"    }");
            }

            writer.WriteLine(@"}");
        }

        // Reimport generated file
        AssetDatabase.ImportAsset(@"Assets\Plugins\AudioManager\AudioManagerGenerated.cs", ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
    }

    private void OnSceneChange()
    {
        Debug.Log("Changed scene to " + currentScene);
        
        // Instantiate new manager when changing scene
        InstantiateNewAudioManager();
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
        
        if (audioManagerInstance == null)
        {
            // Instantiate new manager if current one is null
            InstantiateNewAudioManager();
        }
    }

    void InstantiateNewAudioManager()
    {
        ApplySoundsToManager();
        
        var audioManager = (GameObject)Resources.LoadAssetAtPath("Assets/Plugins/AudioManager/AudioManager.prefab", typeof(GameObject));
        
        // Find other audio managers in the scene
        var objects = FindObjectsOfType(typeof(AudioManager));

        // Destoy other audio managers
        for (int i = objects.Length - 1; i >= 0; i--)
        {
            DestroyImmediate((objects[i] as AudioManager).gameObject);
        }

        // Instantiate new manager
        audioManagerInstance = (GameObject)Instantiate(audioManager);

        // Hide new manager
        audioManagerInstance.hideFlags = HideFlags.HideInHierarchy;

        Debug.Log("New maaaaanaaaaager");
    }

    void OnGUI()
    {
        bool changedAudioCollection = false;

        Color oldColor = GUI.color;

        // Reset color
        GUI.color = oldColor;

        DropAreaGui();

        // Begin a scrollview
        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandWidth(true));

        foreach (AudioItem audioItem in audioItems.ToArray())
        {
            EditorGUILayout.BeginHorizontal();
            
            // Path label
            EditorGUILayout.LabelField(Path.GetFileName(audioItem.Path), EditorStyles.boldLabel);

            // Play button
            GUILayout.Button("Play");

            // Delete button
            if (audioItem != itemToRemove)
            {
                // Change color to red
                GUI.color = new Color(1, 0.3f, 0.3f);
                
                if (GUILayout.Button("Remove"))
                {
                    changedAudioCollection = true;

                    itemToRemove = audioItem;
                }

                // Reset color
                GUI.color = oldColor;
            }

            // Check if this is the desired item to remove
            if (audioItem == itemToRemove)
            {
                // Don't remove if nope is pressed
                if (GUILayout.Button("Nope"))
                {
                    itemToRemove = null;
                }
                
                // Change color to red
                GUI.color = new Color(1, 0.3f, 0.3f);
                
                // Remove the item if k is pressed
                if (GUILayout.Button("Ok"))
                {
                    audioItems.Remove(itemToRemove);
                    
                    itemToRemove = null;
                }

                // Reset color
                GUI.color = oldColor;
            }

            EditorGUILayout.EndHorizontal();

            // Volume slider
            audioItem.Volume = EditorGUILayout.Slider("Volume", audioItem.Volume, 0, 1);

            // Looping toggle
            audioItem.Loop = EditorGUILayout.Toggle("Looping", audioItem.Loop);

            // Play on awake toggle
            audioItem.PlayOnAwake = EditorGUILayout.Toggle("Play on Awake", audioItem.PlayOnAwake);
        }

        if (GUILayout.Button("Apply"))
        {
            SaveAllAudioItems();

            ApplySoundsToManager();

            InstantiateNewAudioManager();
        }

        GUILayout.EndScrollView();

        if (changedAudioCollection)
        {
            SaveAllAudioItems();

            InstantiateNewAudioManager();

            Repaint();
        }
    }

    private static void LoadAllAudioItems()
    {
        audioItems = new List<AudioItem>();
        
        // Load previous audio items
        for (int i = 0; i < EditorPrefs.GetInt("amountOfAudioItems"); i++)
        {
            audioItems.Add(new AudioItem
            {
                Path = EditorPrefs.GetString(i + "_Path"),
                Loop = EditorPrefs.GetBool(i + "_Loop"),
                PlayOnAwake = EditorPrefs.GetBool(i + "_PlayOnAwake"),
                Volume = EditorPrefs.GetFloat(i + "_Volume"),
            });
        }
    }

    void SaveAllAudioItems()
    {
        // Save new amount
        EditorPrefs.SetInt("amountOfAudioItems", audioItems.Count);

        // Save items
        foreach (var audioItem in audioItems.Select((item, index) => new {item, index}))
        {
            audioItem.item.SaveItem(audioItem.index);
        }
    }

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

                    foreach (string path in DragAndDrop.paths)
                    {
                        OnDropAudioFile(path);
                    }

                    // Save state of audio items
                    SaveAllAudioItems();
                }

                Event.current.Use();

                break;
        }
    }
}
