using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
//right click a prefab and choose deep copy,this script will copy a prefab's art sources to other folder
public class Prefab_DeepCopy
{
    [MenuItem("Assets/Deep Copy")]
    public static void DeepCopy()
    {
        string clickedAssetGuid = Selection.assetGUIDs[0];
        string clickedPath = AssetDatabase.GUIDToAssetPath(clickedAssetGuid);
        string fileExt = Path.GetExtension(clickedPath);
        if (fileExt != ".prefab")
        {
            Debug.LogError("not prefab,return");
            return;
        }

        var lastsplit = clickedPath.LastIndexOf('/');
        string filename = Selection.gameObjects[0].name;
        //new folder
        string parentPath = clickedPath.Substring(0, lastsplit);
        string newFolder = parentPath + "/DeepCopy_"+filename+"/";
        AssetDatabase.CreateFolder(parentPath,"DeepCopy_"+filename);

        //new prefab
        Debug.Log("newfile " + newFolder  + filename + ".prefab");
        var localPath = AssetDatabase.GenerateUniqueAssetPath(newFolder + filename + ".prefab");
        bool prefabSuccess;
        GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(clickedPath);
        GameObject go_copy = GameObject.Instantiate(go);
        PrefabUtility.SaveAsPrefabAssetAndConnect(go_copy, localPath, InteractionMode.UserAction, out prefabSuccess);
        GameObject.DestroyImmediate(go_copy);

        //guid map
        string[] deps = AssetDatabase.GetDependencies(localPath);
        Dictionary<string, string> guidMap = new Dictionary<string, string>();
        for (int i = 0; i < deps.Length; i++)
        {
            var item = deps[i];
            var ext = Path.GetExtension(item);
            if (ext == ".cs") continue;
            if (ext == ".shader") continue;
            if (ext == ".shadergraph") continue;
            //todo maby other fileTypes need to ignore

            int indexSpit = item.LastIndexOf('/');
            string fname = item.Substring(indexSpit);
            AssetDatabase.CopyAsset(item, newFolder + fname);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            string guid = AssetDatabase.AssetPathToGUID(item);
            string copyGuid = AssetDatabase.AssetPathToGUID(newFolder + fname);
            guidMap.Add(guid, copyGuid);
        }

        //replace dependency
        var fullPath = Path.GetFullPath(newFolder);
        var filePaths = GetAllFiles(fullPath);
        foreach (var filePath in filePaths)
        {
            var assetPath = GetRelativeAssetPath(filePath);
            string[] gdeps = AssetDatabase.GetDependencies(assetPath, true);
            var fileString = File.ReadAllText(filePath);
            bool bChanged = false;
            foreach (var v in gdeps)
            {
                var guid = AssetDatabase.AssetPathToGUID(v);
                if (!guidMap.ContainsKey(guid)) continue;
                if (!Regex.IsMatch(fileString, guid)) continue;
                fileString = Regex.Replace(fileString, guid, guidMap[guid]);
                bChanged = true;
                var oldFile = AssetDatabase.GUIDToAssetPath(guid);
                var newFile = AssetDatabase.GUIDToAssetPath(guidMap[guid]);
                Debug.Log(oldFile + "\nTo\n" + newFile);
            }
            if (bChanged)
            {
                File.WriteAllText(filePath, fileString);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static string GetRelativeAssetPath(string fullPath)
    {
        fullPath = fullPath.Replace("\\", "/");
        int index = fullPath.IndexOf("Assets");
        string relativePath = fullPath.Substring(index);
        return relativePath;
    }

    private static string[] GetAllFiles(string fullPath)
    {
        List<string> files = new List<string>();
        foreach (string file in GetFiles(fullPath))
        {
            files.Add(file);
        }
        return files.ToArray();
    }

    private static IEnumerable<string> GetFiles(string path)
    {
        Queue<string> queue = new Queue<string>();
        queue.Enqueue(path);
        while (queue.Count > 0)
        {
            path = queue.Dequeue();
            try
            {
                foreach (string subDir in Directory.GetDirectories(path))
                {
                    queue.Enqueue(subDir);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
            string[] files = null;
            try
            {
                files = Directory.GetFiles(path);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
            if (files != null)
            {
                for (int i = 0; i < files.Length; i++)
                {
                    yield return files[i];
                }
            }
        }
    }
}
