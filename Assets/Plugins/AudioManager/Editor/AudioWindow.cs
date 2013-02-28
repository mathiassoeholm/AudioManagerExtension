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

        window.title = "Audio Manager";

        // Load audio items
        LoadAllAudioItems();

        ApplySoundsToManager();

        window.InstantiateNewAudioManager();


        Debug.Log("loaded");
	}

    static AudioWindow()
    {
        currentScene = EditorApplication.currentScene;
        
        // Load audio items
        LoadAllAudioItems();

        Debug.Log("static constructor");
    }

    private void OnLostFocus()
    {
        // Cancel removing an item when window loses focus
        itemToRemove = null;

        // Repaint the window
        Repaint();
    }

    private void OnSceneChange()
    {
        Debug.Log("Changed scene to " + currentScene);
        
        // Instantiate new manager when changing scene
        InstantiateNewAudioManager();
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
    }

    private void Update()
    {
        Debug.Log(Application.dataPath);
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

            if (AudioManager.Instance.IsAudioItemPlaying(audioItem))
            {
                if (GUILayout.Button("Stop"))
                {
                    AudioManager.Instance.StopAudioItem(audioItem);
                    changedAudioCollection = true;
                }
            }
            else if (GUILayout.Button("Play"))
            {
                AudioManager.Instance.PlaySound(audioItem);

                changedAudioCollection = true;
            }

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
                    // Stop any sources with this sound from playing
                    AudioManager.Instance.StopAudioItem(audioItem);
                    
                    audioItems.Remove(itemToRemove);
                    
                    itemToRemove = null;

                    SaveAllAudioItems();

                    InstantiateNewAudioManager();
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
            ApplyChanges();
        }

        GUILayout.EndScrollView();

        if (changedAudioCollection)
        {
            Repaint();
        }
    }

    private void ApplyChanges()
    {
        // Stop removing any item
        itemToRemove = null;
        
        // Stop all audio sources from playing
        AudioManager.Instance.StopAllSounds();
        
        // Save state of editor using editorprefs
        SaveAllAudioItems();
        
        // Apply modified properties to audio manager prefab
        ApplySoundsToManager();
        
        // Generate partial AudioManager class
        GenerateCode();

        // Instantiate Audio Manager
        InstantiateNewAudioManager();
        
        // Repaint
        Repaint();
    }

    static void ApplySoundsToManager()
    {
        var audioManager = (GameObject)Resources.LoadAssetAtPath("Assets/Plugins/AudioManager/AudioManager.prefab", typeof(GameObject));

        audioManager.GetComponent<AudioManager>().AudioItems = audioItems.ToArray();
    }

    private void GenerateCode()
    {
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
        //audioManagerInstance.hideFlags = HideFlags.HideInHierarchy;

        Debug.Log("New maaaaanaaaaager");
    }

    private static void LoadSerializedAudio()
    {
        if (!Directory.Exists(Application.persistentDataPath + @"\AudioSaveData"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + @"\AudioSaveData");
        }

        using (var fileStream = new FileStream(Application.persistentDataPath + @"\AudioSaveData\AudioItems.dat",  FileMode.OpenOrCreate, FileAccess.Read))
        {
            var formatter = new BinaryFormatter();

            fileStream.Position = 0;

            if (fileStream.Length > 0)
            {
                audioItems = (List<AudioItem>)formatter.Deserialize(fileStream);


            }
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
        for (int i = 0; i < audioItems.Count; i++)
        {
            audioItems[i].SaveItem(i);
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
